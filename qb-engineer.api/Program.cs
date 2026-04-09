using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QBEngineer.Api.Behaviors;
using QBEngineer.Api.Data;
using QBEngineer.Api.Hubs;
using QBEngineer.Api.Middleware;
using QBEngineer.Api.Services;
using Microsoft.Extensions.Options;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;
using QBEngineer.Data.Repositories;
using QBEngineer.Integrations;
using QBEngineer.Integrations.Builders;
using System.Threading.RateLimiting;
using Hangfire;
using Hangfire.PostgreSql;
using QBEngineer.Api.Jobs;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using QBEngineer.Api.HealthChecks;
using Scalar.AspNetCore;
using QBEngineer.Api.Extensions;
using Microsoft.AspNetCore.DataProtection;
using Serilog;

QuestPDF.Settings.License = LicenseType.Community;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Load secrets file (gitignored)
    builder.Configuration.AddJsonFile("appsettings.Secrets.json", optional: true, reloadOnChange: true);

    // Serilog
    builder.Host.UseSerilog((context, services, config) => config
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .WriteTo.Console()
        .Enrich.FromLogContext());

    // Clock abstraction — MockClock in development (controllable via /api/v1/dev/clock),
    // SystemClock in production.
    var useMockClock = builder.Environment.IsDevelopment();
    if (useMockClock)
    {
        var mockClock = new MockClock();
        builder.Services.AddSingleton<MockClock>(mockClock);
        builder.Services.AddSingleton<IClock>(mockClock);
        Log.Information("Clock: MockClock (development) — controllable via POST /api/v1/dev/clock");
    }
    else
    {
        builder.Services.AddSingleton<IClock, SystemClock>();
    }

    // EF Core + PostgreSQL (with pgvector for AI embeddings)
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            npgsqlOptions => npgsqlOptions.UseVector()));

    // ASP.NET Identity
    builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

    // JWT Authentication
    var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-secret-key-change-in-production-min-32-chars!!";
    var authBuilder = builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "qb-engineer",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "qb-engineer-ui",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };

        // SignalR sends JWT via query string (WebSocket can't use headers)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/hubs") || path.StartsWithSegments("/api/v1/downloads")))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

    // SSO Configuration (optional — each provider independently enabled)
    var ssoOptions = builder.Configuration.GetSection("Sso").Get<SsoOptions>() ?? new SsoOptions();
    builder.Services.Configure<SsoOptions>(builder.Configuration.GetSection("Sso"));
    var anySsoEnabled = ssoOptions.Google.Enabled || ssoOptions.Microsoft.Enabled || ssoOptions.Oidc.Enabled;

    if (anySsoEnabled)
    {
        // Add a temporary cookie scheme for the OAuth redirect flow
        // (JWT is the default auth scheme; this cookie is only used during SSO round-trip)
        authBuilder.AddCookie("SsoExternalCookie", options =>
        {
            options.Cookie.Name = "qbe-sso-ext";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
        });
    }

    if (ssoOptions.Google.Enabled)
    {
        authBuilder.AddGoogle(options =>
        {
            options.ClientId = ssoOptions.Google.ClientId;
            options.ClientSecret = ssoOptions.Google.ClientSecret;
            options.SignInScheme = "SsoExternalCookie";
        });
        Log.Information("SSO: Google authentication enabled");
    }

    if (ssoOptions.Microsoft.Enabled)
    {
        authBuilder.AddMicrosoftAccount(options =>
        {
            options.ClientId = ssoOptions.Microsoft.ClientId;
            options.ClientSecret = ssoOptions.Microsoft.ClientSecret;
            options.SignInScheme = "SsoExternalCookie";
        });
        Log.Information("SSO: Microsoft authentication enabled");
    }

    if (ssoOptions.Oidc.Enabled)
    {
        authBuilder.AddOpenIdConnect(options =>
        {
            options.Authority = ssoOptions.Oidc.Authority;
            options.ClientId = ssoOptions.Oidc.ClientId;
            options.ClientSecret = ssoOptions.Oidc.ClientSecret;
            options.ResponseType = "code";
            options.SaveTokens = true;
            options.GetClaimsFromUserInfoEndpoint = true;
            options.Scope.Add("email");
            options.Scope.Add("profile");
            options.SignInScheme = "SsoExternalCookie";
        });
        Log.Information("SSO: OIDC authentication enabled ({DisplayName})", ssoOptions.Oidc.DisplayName ?? "Generic");
    }

    builder.Services.AddAuthorization();

    // SignalR
    builder.Services.AddSignalR();

    // Repositories
    builder.Services.AddScoped<IJobRepository, JobRepository>();
    builder.Services.AddScoped<ISubtaskRepository, SubtaskRepository>();
    builder.Services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
    builder.Services.AddScoped<ITrackTypeRepository, TrackTypeRepository>();
    builder.Services.AddScoped<IReferenceDataRepository, ReferenceDataRepository>();
    builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
    builder.Services.AddScoped<IPartRepository, PartRepository>();
    builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
    builder.Services.AddScoped<ILeadRepository, LeadRepository>();
    builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
    builder.Services.AddScoped<IAssetRepository, AssetRepository>();
    builder.Services.AddScoped<ITimeTrackingRepository, TimeTrackingRepository>();
    builder.Services.AddScoped<IUserPreferenceRepository, UserPreferenceRepository>();
    builder.Services.AddScoped<IFileRepository, FileRepository>();
    builder.Services.AddScoped<IJobLinkRepository, JobLinkRepository>();
    builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
    builder.Services.AddScoped<ITerminologyRepository, TerminologyRepository>();
    builder.Services.AddScoped<IReportRepository, ReportRepository>();
    builder.Services.AddScoped<ISearchRepository, SearchRepository>();
    builder.Services.AddScoped<IPlanningCycleRepository, PlanningCycleRepository>();
    builder.Services.AddScoped<IVendorRepository, VendorRepository>();
    builder.Services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
    builder.Services.AddScoped<ISalesOrderRepository, SalesOrderRepository>();
    builder.Services.AddScoped<IQuoteRepository, QuoteRepository>();
    builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();
    builder.Services.AddScoped<ICustomerAddressRepository, CustomerAddressRepository>();
    builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
    builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
    builder.Services.AddScoped<IPriceListRepository, PriceListRepository>();
    builder.Services.AddScoped<IRecurringOrderRepository, RecurringOrderRepository>();
    builder.Services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();
    builder.Services.AddScoped<ISyncQueueRepository, SyncQueueRepository>();
    builder.Services.AddScoped<IStatusEntryRepository, StatusEntryRepository>();
    builder.Services.AddScoped<IReportBuilderRepository, ReportBuilderRepository>();
    builder.Services.AddScoped<IEmbeddingRepository, EmbeddingRepository>();
    builder.Services.AddScoped<IBarcodeService, BarcodeService>();
    builder.Services.AddSingleton<ICsvExportService, CsvExportService>();
    builder.Services.AddSingleton<IImageService, ImageService>();
    builder.Services.AddSingleton<ITokenEncryptionService, TokenEncryptionService>();
    builder.Services.AddHttpContextAccessor();

    // Data Protection (OAuth token encryption, key storage in PostgreSQL)
    builder.Services.AddDataProtection()
        .PersistKeysToDbContext<AppDbContext>()
        .SetApplicationName("qb-engineer");

    // Integration services (mock or real based on config)
    var useMocks = builder.Configuration.GetValue<bool>("MockIntegrations");
    builder.Services.Configure<MinioOptions>(builder.Configuration.GetSection(MinioOptions.SectionName));
    builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.SectionName));
    builder.Services.Configure<QuickBooksOptions>(builder.Configuration.GetSection(QuickBooksOptions.SectionName));
    builder.Services.Configure<AiOptions>(builder.Configuration.GetSection(AiOptions.SectionName));
    builder.Services.Configure<UspsOptions>(builder.Configuration.GetSection(UspsOptions.SectionName));
    builder.Services.Configure<DocuSealOptions>(builder.Configuration.GetSection(DocuSealOptions.SectionName));
    // Shipping carrier options
    builder.Services.Configure<UpsOptions>(builder.Configuration.GetSection(UpsOptions.SectionName));
    builder.Services.Configure<FedExOptions>(builder.Configuration.GetSection(FedExOptions.SectionName));
    builder.Services.Configure<DhlOptions>(builder.Configuration.GetSection(DhlOptions.SectionName));
    builder.Services.Configure<StampsOptions>(builder.Configuration.GetSection(StampsOptions.SectionName));
    // Accounting provider options
    builder.Services.Configure<XeroOptions>(builder.Configuration.GetSection(XeroOptions.SectionName));
    builder.Services.Configure<FreshBooksOptions>(builder.Configuration.GetSection(FreshBooksOptions.SectionName));
    builder.Services.Configure<SageOptions>(builder.Configuration.GetSection(SageOptions.SectionName));
    builder.Services.Configure<NetSuiteOptions>(builder.Configuration.GetSection(NetSuiteOptions.SectionName));
    builder.Services.Configure<WaveOptions>(builder.Configuration.GetSection(WaveOptions.SectionName));
    builder.Services.Configure<ZohoOptions>(builder.Configuration.GetSection(ZohoOptions.SectionName));

    // QuickBooks token service (always registered — handles OAuth token lifecycle)
    builder.Services.AddScoped<IQuickBooksTokenService, QuickBooksTokenService>();

    // Session (used for OAuth state parameter)
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(10);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

    var storageProvider = builder.Configuration.GetValue<string>("Storage:Provider") ?? "minio";
    builder.Services.Configure<LocalStorageOptions>(builder.Configuration.GetSection(LocalStorageOptions.SectionName));

    if (useMocks)
    {
        builder.Services.AddSingleton<IStorageService, MockStorageService>();
        builder.Services.AddSingleton<IAccountingService, MockAccountingService>();
        builder.Services.AddSingleton<IShippingService, MockShippingService>();
        builder.Services.AddSingleton<IAddressValidationService, MockAddressValidationService>();
        builder.Services.AddSingleton<IAiService, MockAiService>();
        builder.Services.AddSingleton<IEmailService, MockEmailService>();
        builder.Services.AddSingleton<IDocumentSigningService, MockDocumentSigningService>();
        builder.Services.AddSingleton<IPdfJsExtractorService, MockPdfJsExtractorService>();
        builder.Services.AddSingleton<IFormDefinitionParser, FormDefinitionParser>();
        builder.Services.AddSingleton<IFormDefinitionVerifier, FormDefinitionVerifier>();
        builder.Services.AddSingleton<IFormRendererService, MockFormRendererService>();
        builder.Services.AddSingleton<IImageComparisonService, MockImageComparisonService>();
        builder.Services.AddSingleton<IWalkthroughGeneratorService, MockWalkthroughGeneratorService>();
        builder.Services.AddSingleton<IPdfFormFillService, MockPdfFormFillService>();
        Log.Information("MockIntegrations=true — using in-memory storage and mock services");
    }
    else
    {
        if (storageProvider == "local")
        {
            builder.Services.AddSingleton<IStorageService, LocalFileStorageService>();
            Log.Information("Storage provider: local filesystem ({RootPath})",
                builder.Configuration.GetValue<string>("LocalStorage:RootPath") ?? "/app/storage");
        }
        else
        {
            builder.Services.AddSingleton<IStorageService, MinioStorageService>();
            Log.Information("Storage provider: MinIO");
        }
        builder.Services.AddSingleton<IEmailService, SmtpEmailService>();
        // Accounting providers — all implementations registered; factory resolves active one from system settings
        builder.Services.AddScoped<IAccountingService, LocalAccountingService>();
        builder.Services.AddScoped<IAccountingService, QuickBooksAccountingService>();
        builder.Services.AddScoped<IAccountingService, XeroAccountingService>();
        builder.Services.AddScoped<IAccountingService, FreshBooksAccountingService>();
        builder.Services.AddScoped<IAccountingService, SageAccountingService>();
        builder.Services.AddScoped<IAccountingService, NetSuiteAccountingService>();
        builder.Services.AddScoped<IAccountingService, WaveAccountingService>();
        builder.Services.AddScoped<IAccountingService, ZohoAccountingService>();
        // Shipping: all configured carriers registered; MultiCarrierShippingService aggregates them
        builder.Services.AddSingleton<IShippingCarrierService, UpsShippingService>();
        builder.Services.AddSingleton<IShippingCarrierService, FedExShippingService>();
        builder.Services.AddSingleton<IShippingCarrierService, UspsShippingService>();
        builder.Services.AddSingleton<IShippingCarrierService, DhlShippingService>();
        builder.Services.AddSingleton<IShippingService, MultiCarrierShippingService>();
        // Address validation: USPS Addresses API v3 (OAuth 2.0) when credentials configured, otherwise mock
        var uspsKey = builder.Configuration.GetSection(UspsOptions.SectionName)["ConsumerKey"];
        if (!string.IsNullOrEmpty(uspsKey))
        {
            builder.Services.AddHttpClient<IAddressValidationService, UspsAddressValidationService>();
            Log.Information("USPS Addresses API v3 address validation enabled");
        }
        else
        {
            builder.Services.AddSingleton<IAddressValidationService, MockAddressValidationService>();
            Log.Information("USPS credentials not configured — using mock address validation");
        }
        builder.Services.AddHttpClient<IAiService, OllamaAiService>();
        builder.Services.AddHttpClient<IDocumentSigningService, DocuSealSigningService>();
        builder.Services.AddSingleton<IPdfJsExtractorService, PdfJsExtractorService>();
        builder.Services.AddSingleton<IFormDefinitionParser, FormDefinitionParser>();
        builder.Services.AddScoped<IFormDefinitionVerifier, FormDefinitionVerifier>();
        builder.Services.AddSingleton<IFormRendererService, PuppeteerFormRendererService>();
        builder.Services.AddSingleton<IImageComparisonService, SkiaImageComparisonService>();
        builder.Services.AddSingleton<IWalkthroughGeneratorService, PuppeteerWalkthroughGeneratorService>();
        builder.Services.AddSingleton<IPdfFormFillService, PdfSharpFormFillService>();
    }

    // Form definition builders — hardcoded definitions for known government forms
    // (registered outside mock/real block since builders work with extraction data, not external APIs)
    builder.Services.AddSingleton<IFormDefinitionBuilder, W4FormDefinitionBuilder>();
    builder.Services.AddSingleton<IFormDefinitionBuilder, I9FormDefinitionBuilder>();
    builder.Services.AddSingleton<IStateFormDefinitionBuilder, IdahoW4FormDefinitionBuilder>();
    builder.Services.AddSingleton<IFormDefinitionBuilderFactory, FormDefinitionBuilderFactory>();

    // Accounting provider factory — resolves active provider from system settings
    builder.Services.AddScoped<IAccountingProviderFactory, AccountingProviderFactory>();

    // Resilient HTTP clients
    builder.Services.AddResilientHttpClients();

    // MediatR
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

    // Controllers + OpenAPI
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
    builder.Services.AddOpenApi();

    // CORS — read allowed origins from CORS_ORIGINS env var (comma-separated),
    // plus internal Docker service names that are always needed.
    var corsOrigins = new List<string>
    {
        "http://localhost:4200",
        "http://localhost:4201",
        "http://localhost:80",
        "http://localhost",
        "http://qb-engineer-ui",
        "http://qb-engineer-ui:80",
    };
    var envOrigins = builder.Configuration["CORS_ORIGINS"];
    if (!string.IsNullOrWhiteSpace(envOrigins))
    {
        foreach (var origin in envOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!corsOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
                corsOrigins.Add(origin);
        }
    }
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins(corsOrigins.ToArray())
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    // Hangfire
    builder.Services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(options =>
            options.UseNpgsqlConnection(
                builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty)));
    builder.Services.AddHangfireServer(options =>
    {
        options.Queues = ["default"];
        options.WorkerCount = Math.Max(Environment.ProcessorCount, 4);
    });
    builder.Services.AddScoped<RecurringOrderJob>();
    builder.Services.AddScoped<OverdueInvoiceJob>();
    builder.Services.AddScoped<ScheduledTaskJob>();
    builder.Services.AddScoped<DailyDigestJob>();
    builder.Services.AddTransient<DatabaseBackupJob>();
    builder.Services.AddScoped<SyncQueueProcessorJob>();
    builder.Services.AddScoped<CustomerSyncJob>();
    builder.Services.AddScoped<AccountingCacheSyncJob>();
    builder.Services.AddScoped<OrphanDetectionJob>();
    builder.Services.AddScoped<ItemSyncJob>();
    builder.Services.AddScoped<RecurringExpenseJob>();
    builder.Services.AddScoped<DocumentIndexJob>();
    builder.Services.AddScoped<OverdueMaintenanceJob>();
    builder.Services.AddScoped<ComplianceFormSyncJob>();
    builder.Services.AddScoped<ReorderAnalysisJob>();

    // Health checks
    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty,
            name: "postgresql")
        .AddHangfire(options => { options.MinimumAvailableServers = 1; }, name: "hangfire")
        .AddCheck<MinioHealthCheck>("minio")
        .AddCheck<SignalRHealthCheck>("signalr");

    // Rate limiting — disabled in Development (simulation, E2E tests need unrestricted access)
    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
        {
            // In Development, disable all rate limiting
            if (builder.Environment.IsDevelopment())
                return RateLimitPartition.GetNoLimiter("dev");

            var path = ctx.Request.Path.Value ?? string.Empty;
            if (path.StartsWith("/hubs/", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/health", StringComparison.OrdinalIgnoreCase)
                || path.Equals("/api/v1/version", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/api/v1/dev/", StringComparison.OrdinalIgnoreCase))
                return RateLimitPartition.GetNoLimiter("infra");

            // Bypass rate limiting for loopback (E2E tests, local dev tools)
            var ip = ctx.Connection.RemoteIpAddress;
            if (ip != null && System.Net.IPAddress.IsLoopback(ip))
                return RateLimitPartition.GetNoLimiter("loopback");

            return RateLimitPartition.GetFixedWindowLimiter(
                ctx.User?.Identity?.Name ?? ctx.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 2000,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                });
        });
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });

    var app = builder.Build();

    // Database initialization (skip migrations for in-memory provider used in integration tests)
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (db.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            await db.Database.EnsureCreatedAsync();
        }
        else
        {
            var forceRecreate = builder.Configuration.GetValue<bool>("RECREATE_DB");

            if (forceRecreate)
            {
                // Manual escape hatch — set RECREATE_DB=true to force a wipe (dev use only)
                Log.Warning("RECREATE_DB=true — dropping and recreating database from migrations...");
                await db.Database.EnsureDeletedAsync();
            }
            else if (await db.Database.CanConnectAsync())
            {
                // DB exists — check for legacy schema (EnsureCreated, no migrations history)
                IEnumerable<string> applied;
                try
                {
                    applied = await db.Database.GetAppliedMigrationsAsync();
                }
                catch
                {
                    // __EFMigrationsHistory table missing — definitely a pre-migration schema
                    applied = [];
                }

                if (!applied.Any())
                {
                    // No migration history. Verify app tables actually exist (vs. brand-new empty DB).
                    bool hasLegacySchema = false;
                    try { hasLegacySchema = await db.Jobs.AnyAsync(); }
                    catch { /* table doesn't exist — this is a fresh DB, not a legacy one */ }

                    if (hasLegacySchema)
                    {
                        Log.Warning(
                            "Legacy schema detected (database exists with tables but no migrations history). " +
                            "Performing one-time forced recreation so the migration baseline is established. " +
                            "This only happens once — all future schema changes will use incremental migrations.");
                        await db.Database.EnsureDeletedAsync();
                    }
                }
            }

            // Apply all pending migrations (creates DB from scratch if it doesn't exist)
            await db.Database.MigrateAsync();
            Log.Information("Database migrations applied successfully");

            // Seed essential data idempotently (roles, track types, reference data).
            // Demo data (users, customers, jobs, etc.) only seeded when SEED_DEMO_DATA=true.
            var seedDemoData = builder.Configuration.GetValue<bool>("SEED_DEMO_DATA");
            await SeedData.SeedAsync(scope.ServiceProvider, seedDemoData);

            // Seed built-in AI assistants (idempotent)
            await QBEngineer.Api.Features.AiAssistants.SeedAiAssistants.EnsureSeededAsync(db);

            // Auto-extract form definitions for templates that have IsAutoSync + SourceUrl but no FormDefinitionVersion yet
            var templatesNeedingExtraction = await db.ComplianceFormTemplates
                .Where(t => t.IsAutoSync && t.SourceUrl != null && !t.FormDefinitionVersions.Any())
                .ToListAsync();

            if (templatesNeedingExtraction.Count > 0)
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                foreach (var tmpl in templatesNeedingExtraction)
                {
                    try
                    {
                        Log.Information("Auto-extracting form definition for template {TemplateId} ({Name})",
                            tmpl.Id, tmpl.Name);
                        await mediator.Send(new QBEngineer.Api.Features.ComplianceForms.ExtractFormDefinitionCommand(tmpl.Id));
                        Log.Information("Form definition extracted successfully for template {TemplateId}", tmpl.Id);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Auto-extraction failed for template {TemplateId} ({Name}) — admin can extract manually",
                            tmpl.Id, tmpl.Name);
                    }
                }
            }
        }
    }

    // Storage bucket/directory initialization (skip when using mock storage)
    if (!useMocks)
    {
        using var scope = app.Services.CreateScope();
        var storageService = scope.ServiceProvider.GetRequiredService<IStorageService>();
        var minioOpts = scope.ServiceProvider.GetRequiredService<IOptions<MinioOptions>>().Value;

        try
        {
            await storageService.EnsureBucketExistsAsync(minioOpts.JobFilesBucket, CancellationToken.None);
            await storageService.EnsureBucketExistsAsync(minioOpts.ReceiptsBucket, CancellationToken.None);
            await storageService.EnsureBucketExistsAsync(minioOpts.EmployeeDocsBucket, CancellationToken.None);
            await storageService.EnsureBucketExistsAsync(minioOpts.PiiDocsBucket, CancellationToken.None);
            Log.Information("Storage buckets/directories verified ({Provider})", storageProvider);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Storage initialization failed — file storage unavailable until provider is reachable");
        }
    }

    // Middleware pipeline
    var forwardedHeadersOptions = new ForwardedHeadersOptions
    {
        ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
            | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
            | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedHost,
    };
    forwardedHeadersOptions.KnownNetworks.Clear();
    forwardedHeadersOptions.KnownProxies.Clear();
    app.UseForwardedHeaders(forwardedHeadersOptions);
    app.UseMiddleware<SecurityHeadersMiddleware>();
    app.UseSerilogRequestLogging();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseCors();
    app.UseRateLimiter();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.WithTitle("QB Engineer API");
            options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });

        // ── Dev-only: clock control for E2E simulation ─────────────────────
        var devClock = app.Services.GetRequiredService<MockClock>();

        app.MapPost("/api/v1/dev/clock", (ClockSetRequest req) =>
        {
            devClock.Set(req.Now);
            return Results.Ok(new { now = devClock.UtcNow });
        });

        app.MapGet("/api/v1/dev/clock", () =>
            Results.Ok(new { now = devClock.UtcNow }));

        app.MapDelete("/api/v1/dev/clock", () =>
        {
            devClock.Set(DateTimeOffset.UtcNow);
            return Results.Ok(new { now = devClock.UtcNow });
        });

        // ── Dev-only: simulation state summary ─────────────────────────────
        app.MapGet("/api/v1/dev/simulation-state", async (AppDbContext db) =>
        {
            var jobsByStage = await db.Jobs
                .Where(j => j.DeletedAt == null)
                .Include(j => j.CurrentStage)
                .GroupBy(j => j.CurrentStage != null ? j.CurrentStage.Name : "Unknown")
                .Select(g => new { Stage = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Stage, x => x.Count);

            var now = devClock.UtcNow;
            return Results.Ok(new
            {
                openLeads       = await db.Leads.CountAsync(l => l.DeletedAt == null
                                    && l.Status != QBEngineer.Core.Enums.LeadStatus.Converted
                                    && l.Status != QBEngineer.Core.Enums.LeadStatus.Lost),
                openQuotes      = await db.Quotes.CountAsync(q => q.DeletedAt == null
                                    && q.Status == QBEngineer.Core.Enums.QuoteStatus.Draft),
                openSalesOrders = await db.SalesOrders.CountAsync(so => so.DeletedAt == null
                                    && so.Status != QBEngineer.Core.Enums.SalesOrderStatus.Completed
                                    && so.Status != QBEngineer.Core.Enums.SalesOrderStatus.Cancelled),
                jobsByStage,
                unpaidInvoices  = await db.Invoices.CountAsync(i => i.DeletedAt == null
                                    && i.Status != QBEngineer.Core.Enums.InvoiceStatus.Paid
                                    && i.Status != QBEngineer.Core.Enums.InvoiceStatus.Voided),
                overduePos      = await db.PurchaseOrders.CountAsync(po => po.DeletedAt == null
                                    && po.Status == QBEngineer.Core.Enums.PurchaseOrderStatus.Submitted
                                    && po.ExpectedDeliveryDate < now),
                activeTimers    = await db.ClockEvents.CountAsync(ce =>
                                    ce.EventType == QBEngineer.Core.Enums.ClockEventType.ClockIn
                                    && !db.ClockEvents.Any(ce2 => ce2.UserId == ce.UserId
                                        && ce2.EventType == QBEngineer.Core.Enums.ClockEventType.ClockOut
                                        && ce2.Timestamp > ce.Timestamp)),
                pendingExpenses = await db.Expenses.CountAsync(e => e.DeletedAt == null
                                    && e.Status == QBEngineer.Core.Enums.ExpenseStatus.Pending),
            });
        }).RequireAuthorization();
    }

    app.UseSession();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapGet("/api/v1/version", async () =>
    {
        var appVersion = Environment.GetEnvironmentVariable("APP_VERSION") ?? "dev";
        var gitCommit = Environment.GetEnvironmentVariable("GIT_COMMIT") ?? string.Empty;

        // In dev, fall back to running git if the env var wasn't set at build time
        if (string.IsNullOrEmpty(gitCommit))
        {
            try
            {
                using var proc = new System.Diagnostics.Process();
                proc.StartInfo = new System.Diagnostics.ProcessStartInfo("git", "rev-parse HEAD")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = "/app",
                };
                proc.Start();
                gitCommit = (await proc.StandardOutput.ReadToEndAsync()).Trim();
                await proc.WaitForExitAsync();
            }
            catch
            {
                gitCommit = string.Empty;
            }
        }

        var shortCommit = gitCommit.Length >= 7 ? gitCommit[..7] : gitCommit;
        return Results.Ok(new
        {
            version = appVersion,
            gitCommit,
            shortCommit,
            buildLabel = string.IsNullOrEmpty(shortCommit) ? appVersion : $"{appVersion} ({shortCommit})",
        });
    }).AllowAnonymous();

    app.MapHealthChecks("/api/v1/health", new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds
                }),
                totalDuration = report.TotalDuration.TotalMilliseconds
            };
            await context.Response.WriteAsJsonAsync(result);
        }
    });

    // Hangfire dashboard + recurring jobs
    app.MapHangfireDashboard("/hangfire");
    RecurringJob.AddOrUpdate<RecurringOrderJob>(
        "generate-recurring-orders",
        job => job.GenerateDueOrdersAsync(),
        Cron.Daily(6)); // 6 AM UTC daily
    RecurringJob.AddOrUpdate<RecurringExpenseJob>(
        "generate-recurring-expenses",
        job => job.GenerateDueExpensesAsync(),
        Cron.Daily(5)); // 5 AM UTC daily
    RecurringJob.AddOrUpdate<OverdueInvoiceJob>(
        "mark-overdue-invoices",
        job => job.MarkOverdueInvoicesAsync(),
        Cron.Daily(1)); // 1 AM UTC daily
    RecurringJob.AddOrUpdate<UninvoicedJobNudgeJob>(
        "nudge-uninvoiced-jobs",
        job => job.NudgeUninvoicedJobsAsync(),
        Cron.Daily(8)); // 8 AM UTC daily
    RecurringJob.AddOrUpdate<ScheduledTaskJob>(
        "run-scheduled-tasks",
        job => job.RunDueTasksAsync(),
        "*/15 * * * *"); // Every 15 minutes
    RecurringJob.AddOrUpdate<DailyDigestJob>(
        "send-daily-digest",
        job => job.SendDailyDigestAsync(),
        Cron.Daily(7)); // 7 AM UTC daily
    RecurringJob.AddOrUpdate<OverdueMaintenanceJob>(
        "check-overdue-maintenance",
        job => job.CheckOverdueMaintenanceAsync(),
        Cron.Daily(2)); // 2 AM UTC daily
    RecurringJob.AddOrUpdate<DatabaseBackupJob>(
        "database-backup",
        job => job.RunBackupAsync(),
        Cron.Daily(3)); // 3 AM UTC daily

    // Accounting sync jobs
    RecurringJob.AddOrUpdate<SyncQueueProcessorJob>(
        "sync-queue-processor",
        job => job.ProcessQueueAsync(),
        "*/2 * * * *"); // Every 2 minutes
    RecurringJob.AddOrUpdate<CustomerSyncJob>(
        "customer-sync",
        job => job.SyncCustomersAsync(),
        "0 */4 * * *"); // Every 4 hours
    RecurringJob.AddOrUpdate<AccountingCacheSyncJob>(
        "accounting-cache-sync",
        job => job.RefreshCacheAsync(),
        "0 */6 * * *"); // Every 6 hours
    RecurringJob.AddOrUpdate<OrphanDetectionJob>(
        "orphan-detection",
        job => job.DetectOrphansAsync(),
        "0 3 * * *"); // Daily at 3 AM
    RecurringJob.AddOrUpdate<ItemSyncJob>(
        "item-sync",
        job => job.SyncItemsAsync(),
        "0 */4 * * *"); // Every 4 hours
    RecurringJob.AddOrUpdate<DocumentIndexJob>(
        "document-index",
        job => job.IndexRecentlyUpdatedAsync(),
        "*/30 * * * *"); // Every 30 minutes
    RecurringJob.AddOrUpdate<DocumentIndexJob>(
        "documentation-index",
        job => job.IndexDocumentationAsync(),
        Cron.Daily(3)); // Daily at 3 AM — docs change rarely; startup job handles initial index

    // Index documentation once on startup (AI may not be ready immediately — Hangfire retries)
    BackgroundJob.Enqueue<DocumentIndexJob>(job => job.IndexDocumentationAsync());

    RecurringJob.AddOrUpdate<ComplianceFormSyncJob>(
        "compliance-form-sync",
        job => job.SyncFederalFormsAsync(),
        Cron.Weekly(DayOfWeek.Sunday, 4)); // Sunday 4 AM UTC
    RecurringJob.AddOrUpdate<CheckI9OverdueJob>(
        "check-i9-section2-overdue",
        job => job.CheckOverdueSection2Async(),
        Cron.Daily(9)); // 9 AM UTC daily
    RecurringJob.AddOrUpdate<CheckI9ReverificationJob>(
        "check-i9-reverification",
        job => job.CheckReverificationDueAsync(),
        Cron.Weekly(DayOfWeek.Monday, 9)); // Monday 9 AM UTC weekly
    RecurringJob.AddOrUpdate<ReorderAnalysisJob>(
        "reorder-analysis",
        job => job.RunAnalysisAsync(),
        Cron.Daily(2)); // 2 AM UTC daily

    // SignalR Hubs
    app.MapHub<BoardHub>("/hubs/board");
    app.MapHub<NotificationHub>("/hubs/notifications");
    app.MapHub<TimerHub>("/hubs/timer");
    app.MapHub<ChatHub>("/hubs/chat");

    Log.Information("QB Engineer API starting on {Urls}", string.Join(", ", app.Urls));
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

// Make Program accessible to integration tests via WebApplicationFactory
public partial class Program { }

// Dev-only request model for the clock control endpoint
internal sealed record ClockSetRequest(DateTimeOffset Now);
