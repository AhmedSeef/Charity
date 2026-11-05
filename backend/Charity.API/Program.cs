using System.Text;
using System.Security.Claims;
using Charity.API.Middleware;
using Charity.API.Services;
using Charity.Application.Services.Implementations;
using Charity.Application.Services.Interfaces;
using Charity.Domain.Entities;
using Charity.Domain.Interfaces;
using Charity.Infrastructure.Data.Context;
using Charity.Infrastructure.Data.Repositories;
using Charity.Infrastructure.Data.Seed;
using Charity.Infrastructure.Data.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Events;

// This path remains beside the executable, even when launched from a shortcut or another directory.
var logsDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
Directory.CreateDirectory(logsDirectory);
Serilog.Debugging.SelfLog.Enable(message => Console.Error.WriteLine("Serilog: " + message));
Log.Logger = ApplicationLogging.Configure(new LoggerConfiguration(), logsDirectory).CreateBootstrapLogger();

try
{
Log.Information("Starting Charity API. Daily log directory: {LogsDirectory}", logsDirectory);
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSerilog((services, configuration) => ApplicationLogging.Configure(configuration, logsDirectory)
    .ReadFrom.Services(services)
    .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName));

// 1. Database Connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=.;Database=CharityDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;";

builder.Services.AddDbContext<CharityDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Identity Configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = false;
})
.AddEntityFrameworkStores<CharityDbContext>()
.AddDefaultTokenProviders();

// 3. JWT Authentication Configuration
var jwtKey = builder.Configuration["Jwt:Key"] ?? "CharitySuperSecretKeyForJwtAuthentication2026!#$OnionArchitecture";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "CharityApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "CharityFrontend";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            var db = context.HttpContext.RequestServices.GetRequiredService<CharityDbContext>();
            var user = string.IsNullOrEmpty(userId) ? null : await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || context.Principal?.FindFirstValue("security_stamp") != (user.SecurityStamp ?? string.Empty))
                context.Fail("User session is no longer valid.");
        }
    };
});

builder.Services.AddAuthorization();

// 4. Dependency Injection - Infrastructure & Application
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Onion Repository & UnitOfWork (Entity-Only operations)
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Application Services (Logic & DTO mappers)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBeneficiaryService, BeneficiaryService>();
builder.Services.AddScoped<IDistributionService, DistributionService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IDonationService, DonationService>();
builder.Services.AddScoped<IStaffService, StaffService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ICharityService, CharityService>();
builder.Services.AddScoped<ICharityProvisioningService, CharityProvisioningService>();
builder.Services.AddScoped<ISuspensionReasonService, SuspensionReasonService>();
builder.Services.AddScoped<ISeparationReasonService, SeparationReasonService>();
builder.Services.AddScoped<IExpenseCategoryService, ExpenseCategoryService>();
builder.Services.AddScoped<IGeographicLookupService, GeographicLookupService>();
builder.Services.AddScoped<IBiAnalyticsService, BiAnalyticsService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

// 5. CORS for Angular Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:4200",
            "https://localhost:4200",
            "https://ahmedsamirpro.github.io"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// 6. Controllers & JSON Options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// 7. Swagger Documentation with JWT Bearer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "منظومة إدارة الجمعيات الخيرية المشتركة - API",
        Version = "v1",
        Description = "ASP.NET Core 10 Web API built with Onion Architecture, EF Core, ASP.NET Core Identity & Multi-tenant Federation."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "أدخل رمز الدخول (JWT Bearer Token): Bearer {token}"
    });

    c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", doc, null),
            new List<string>()
        }
    });
});

var app = builder.Build();

// 8. Auto-apply migrations & seed demo data on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = services.GetRequiredService<CharityDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        logger.LogInformation("Ensuring database is created and migrating...");
        await db.Database.MigrateAsync();

        // Security: Always ensure superadmin is secured and weak default credentials are dead
        var adminUserConf = Environment.GetEnvironmentVariable("ADMIN_USERNAME") 
            ?? app.Configuration["ADMIN_USERNAME"] 
            ?? "admin";
        var adminPassConf = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") 
            ?? app.Configuration["ADMIN_PASSWORD"];

        await DbInitializer.EnsureSuperAdminSecuredAsync(userManager, roleManager, adminUserConf, adminPassConf, logger);

        if (app.Configuration.GetValue("DataInitialization:SeedOnStartup", true))
        {
            logger.LogInformation("Seeding initial system lookups and roles...");
            await DbInitializer.SeedAsync(db, userManager, roleManager, adminUserConf, adminPassConf, logger);
            logger.LogInformation("System lookups seeded successfully.");
        }
        else
        {
            logger.LogInformation("Startup reference-data seeding is disabled for this database.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        if (app.Configuration.GetValue("DataInitialization:FailOnStartupError", false))
            throw;
    }
}

// 9. Middleware Pipeline
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    using (logger.BeginScope(new Dictionary<string, object?>
    {
        ["RequestId"] = context.TraceIdentifier,
        ["TraceId"] = System.Diagnostics.Activity.Current?.TraceId.ToString(),
        ["RequestMethod"] = context.Request.Method,
        ["RequestPath"] = context.Request.Path.Value
    }))
    {
        await next(context);
    }
});
app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (context, elapsed, exception) => exception != null || context.Response.StatusCode >= 500
        ? LogEventLevel.Error
        : context.Response.StatusCode >= 400 ? LogEventLevel.Warning : LogEventLevel.Information;
    options.EnrichDiagnosticContext = (diagnostics, context) =>
    {
        diagnostics.Set("RequestId", context.TraceIdentifier);
        diagnostics.Set("TraceId", System.Diagnostics.Activity.Current?.TraceId.ToString());
    };
});
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Charity Federation API v1");
        c.RoutePrefix = "swagger"; // Swagger at /swagger
    });
}

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health/ready", async (CharityDbContext db, CancellationToken cancellationToken) =>
    await db.Database.CanConnectAsync(cancellationToken)
        ? Results.Ok(new { status = "ready" })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable));
app.MapFallbackToFile("index.html");

app.Run();
}
catch (Exception ex) when (ex is not Microsoft.Extensions.Hosting.HostAbortedException)
{
    Log.Fatal(ex, "Charity API terminated unexpectedly during startup or execution.");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

// Expose the API entry point to the integration test host.
public partial class Program { }
