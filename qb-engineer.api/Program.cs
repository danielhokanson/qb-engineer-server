using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
using Scalar.AspNetCore;
using Serilog;

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
    builder.Services.AddAuthentication(options =>
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
    builder.Services.AddHttpContextAccessor();

    // Integration services (mock or real based on config)
    var useMocks = builder.Configuration.GetValue<bool>("MockIntegrations");
    builder.Services.Configure<MinioOptions>(builder.Configuration.GetSection(MinioOptions.SectionName));

    if (useMocks)
    {
        builder.Services.AddSingleton<IStorageService, MockStorageService>();
        builder.Services.AddSingleton<IAccountingService, MockAccountingService>();
        builder.Services.AddSingleton<IShippingService, MockShippingService>();
        builder.Services.AddSingleton<IAiService, MockAiService>();
        Log.Information("MockIntegrations=true — using in-memory storage and mock services");
    }
    else
    {
        builder.Services.AddSingleton<IStorageService, MinioStorageService>();
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

    // Health checks
    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty,
            name: "postgresql");

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
    app.MapHealthChecks("/api/v1/health");

    // SignalR Hubs
    app.MapHub<BoardHub>("/hubs/board");
    app.MapHub<NotificationHub>("/hubs/notifications");
    app.MapHub<TimerHub>("/hubs/timer");

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
