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
using Microsoft.Extensions.Options;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;
using QBEngineer.Data.Repositories;
using QBEngineer.Integrations;
using System.Threading.RateLimiting;
using Hangfire;
using Hangfire.PostgreSql;
using QBEngineer.Api.Jobs;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using QBEngineer.Api.HealthChecks;
using Scalar.AspNetCore;
using Serilog;

QuestPDF.Settings.License = LicenseType.Community;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((context, services, config) => config
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .WriteTo.Console()
        .Enrich.FromLogContext());

    // EF Core + PostgreSQL
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
    builder.Services.AddHttpContextAccessor();

    // Integration services (mock or real based on config)
    var useMocks = builder.Configuration.GetValue<bool>("MockIntegrations");
    builder.Services.Configure<MinioOptions>(builder.Configuration.GetSection(MinioOptions.SectionName));
    builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.SectionName));

    if (useMocks)
    {
        builder.Services.AddSingleton<IStorageService, MockStorageService>();
        builder.Services.AddSingleton<IAccountingService, MockAccountingService>();
        builder.Services.AddSingleton<IShippingService, MockShippingService>();
        builder.Services.AddSingleton<IAiService, MockAiService>();
        builder.Services.AddSingleton<IEmailService, MockEmailService>();
        Log.Information("MockIntegrations=true — using in-memory storage and mock services");
    }
    else
    {
        builder.Services.AddSingleton<IStorageService, MinioStorageService>();
        builder.Services.AddSingleton<IEmailService, SmtpEmailService>();
        // Real accounting/shipping/AI services registered here when implemented
        builder.Services.AddSingleton<IAccountingService, MockAccountingService>();
        builder.Services.AddSingleton<IShippingService, MockShippingService>();
        builder.Services.AddSingleton<IAiService, MockAiService>();
    }

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
    builder.Services.AddHangfireServer();
    builder.Services.AddScoped<RecurringOrderJob>();
    builder.Services.AddScoped<OverdueInvoiceJob>();
    builder.Services.AddScoped<ScheduledTaskJob>();
    builder.Services.AddScoped<DailyDigestJob>();
    builder.Services.AddTransient<DatabaseBackupJob>();

    // Health checks
    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty,
            name: "postgresql")
        .AddHangfire(options => { options.MinimumAvailableServers = 1; }, name: "hangfire")
        .AddCheck<MinioHealthCheck>("minio")
        .AddCheck<SignalRHealthCheck>("signalr");

    // Rate limiting
    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
            RateLimitPartition.GetFixedWindowLimiter(
                ctx.User?.Identity?.Name ?? ctx.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                }));
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });

    var app = builder.Build();

    // Database initialization
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var recreateDb = builder.Configuration.GetValue<bool>("RECREATE_DB");

        if (recreateDb)
        {
            Log.Information("RECREATE_DB=true — dropping and recreating database...");
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
            Log.Information("Database recreated successfully");

            // Seed default data
            await SeedData.SeedAsync(scope.ServiceProvider);
        }
        else
        {
            await db.Database.MigrateAsync();
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
    RecurringJob.AddOrUpdate<DatabaseBackupJob>(
        "database-backup",
        job => job.RunBackupAsync(),
        Cron.Daily(3)); // 3 AM UTC daily

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
