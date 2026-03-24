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
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
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
    builder.Services.Configure<OllamaOptions>(builder.Configuration.GetSection(OllamaOptions.SectionName));
    builder.Services.Configure<UspsOptions>(builder.Configuration.GetSection(UspsOptions.SectionName));
    builder.Services.Configure<DocuSealOptions>(builder.Configuration.GetSection(DocuSealOptions.SectionName));
    builder.Services.Configure<TtsOptions>(builder.Configuration.GetSection("Tts"));
    builder.Services.Configure<CoquiOptions>(builder.Configuration.GetSection(CoquiOptions.SectionName));

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
        builder.Services.AddSingleton<ITtsService, MockTtsService>();
        builder.Services.AddSingleton<ITrainingVideoGeneratorService, MockTrainingVideoGeneratorService>();
        Log.Information("Training video: mock generator (MockIntegrations=true)");
        Log.Information("MockIntegrations=true — using in-memory storage and mock services");
    }
    else
    {
        builder.Services.AddSingleton<IStorageService, MinioStorageService>();
        builder.Services.AddSingleton<IEmailService, SmtpEmailService>();
        // Accounting providers — register all available implementations
        builder.Services.AddScoped<IAccountingService, QuickBooksAccountingService>();
        // Additional providers register here when implemented:
        // builder.Services.AddScoped<IAccountingService, XeroAccountingService>();
        // builder.Services.AddScoped<IAccountingService, FreshBooksAccountingService>();
        // builder.Services.AddScoped<IAccountingService, SageAccountingService>();
        // Shipping: direct carrier integrations registered here when implemented
        // (UPS, FedEx, USPS, DHL — each implements IShippingService directly)
        builder.Services.AddSingleton<IShippingService, MockShippingService>();
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
        // TTS priority: OpenAI (cloud) → Coqui (self-hosted) → Mock
        var ttsKey    = builder.Configuration.GetSection("Tts")["ApiKey"];
        var coquiUrl  = builder.Configuration.GetSection(CoquiOptions.SectionName)["BaseUrl"];
        if (!string.IsNullOrEmpty(ttsKey))
        {
            builder.Services.AddHttpClient("openai-tts");
            builder.Services.AddSingleton<ITtsService, OpenAiTtsService>();
            Log.Information("TTS: OpenAI enabled");
        }
        else if (!string.IsNullOrEmpty(coquiUrl))
        {
            builder.Services.AddHttpClient("coqui-tts");
            builder.Services.AddSingleton<ITtsService, CoquiTtsService>();
            Log.Information("TTS: Coqui self-hosted at {Url}", coquiUrl);
        }
        else
        {
            builder.Services.AddSingleton<ITtsService, MockTtsService>();
            Log.Information("TTS: no provider configured — using mock (set Tts:ApiKey or Coqui:BaseUrl)");
        }
        // Training video: Playwright live screen recording + synchronized TTS narration
        builder.Services.AddSingleton<ITrainingVideoGeneratorService, PlaywrightTrainingVideoGeneratorService>();
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

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins(
                    "http://localhost:4200",
                    "http://localhost:80")
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
        // Default server handles "default" queue only — NOT "video"
        // Keeping video separate prevents concurrent Chromium+recording sessions from OOM-ing the container
        options.Queues = ["default"];
        options.WorkerCount = Math.Max(Environment.ProcessorCount, 4);
    });
    builder.Services.AddHangfireServer(options =>
    {
        // Single-worker video server — exactly one Playwright recording session at a time
        options.ServerName = "video-worker";
        options.Queues = ["video"];
        options.WorkerCount = 1;
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

    // Health checks
    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty,
            name: "postgresql")
        .AddHangfire(options => { options.MinimumAvailableServers = 1; }, name: "hangfire")
        .AddCheck<MinioHealthCheck>("minio")
        .AddCheck<SignalRHealthCheck>("signalr");

    // Rate limiting — exclude SignalR hubs (they retry on auth failure and can starve real API calls)
    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
        {
            var path = ctx.Request.Path.Value ?? string.Empty;
            if (path.StartsWith("/hubs/", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/health", StringComparison.OrdinalIgnoreCase))
                return RateLimitPartition.GetNoLimiter("infra");

            return RateLimitPartition.GetFixedWindowLimiter(
                ctx.User?.Identity?.Name ?? ctx.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 500,
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
            var recreateDb = builder.Configuration.GetValue<bool>("RECREATE_DB");

            if (recreateDb)
            {
                Log.Information("RECREATE_DB=true — dropping and recreating database...");
                await db.Database.EnsureDeletedAsync();
                await db.Database.EnsureCreatedAsync();
                Log.Information("Database recreated successfully");
            }
            else
            {
                await db.Database.MigrateAsync();
            }

            // Seed essential data idempotently (roles, users, track types, reference data, etc.)
            await SeedData.SeedAsync(scope.ServiceProvider);

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

    // MinIO bucket initialization (skip when using mock storage)
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
            Log.Information("MinIO buckets verified");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "MinIO bucket initialization failed — file storage unavailable until MinIO is reachable");
        }
    }

    // Middleware pipeline
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
    }

    app.UseSession();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
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
