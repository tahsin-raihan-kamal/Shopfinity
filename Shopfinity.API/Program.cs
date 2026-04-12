using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shopfinity.Application.Common.Interfaces;
using Shopfinity.Application.Features.Auth.Services;
using Shopfinity.Application.Features.Carts.Services;
using Shopfinity.Application.Features.Categories.Services;
using Shopfinity.Application.Features.Orders.Services;
using Shopfinity.Application.Features.Products.Services;
using Shopfinity.Application.Features.Uploads.Services;
using Shopfinity.Infrastructure.Data;
using Shopfinity.Infrastructure.Identity;
using Shopfinity.Infrastructure.Services;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using Serilog;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ───────────────────────────────────────────────────────────────────
builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithProcessId()
        .WriteTo.Console()
        .WriteTo.File("logs/shopfinity-logs.txt", rollingInterval: RollingInterval.Day);
});

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Required for HttpOnly cookie auth
    });
});

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptionsAction: npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null);
        }));

builder.Services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

// ── Identity ──────────────────────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit           = true;
    options.Password.RequiredLength         = 8;
    options.Password.RequireUppercase       = true;
    options.Password.RequireLowercase       = true;
    options.Password.RequireNonAlphanumeric = true;
    options.User.RequireUniqueEmail         = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ── JWT Authentication ────────────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtSettings["Issuer"],
        ValidAudience            = jwtSettings["Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Key"]!)),
        ClockSkew = TimeSpan.Zero
    };

    // ── CRITICAL FIX 1: Read JWT from HttpOnly cookie set by Next.js proxy ────
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = ctx =>
        {
            // Try cookie first (browser requests from Next.js frontend)
            var cookieToken = ctx.Request.Cookies["shopfinity_token"];
            if (!string.IsNullOrEmpty(cookieToken))
            {
                ctx.Token = cookieToken;
            }
            // Fall back to Authorization header (Postman / Swagger / mobile clients)
            return Task.CompletedTask;
        }
    };
});

// ── Rate Limiting ─────────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100, // Global limit of 100 requests per minute
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.AddFixedWindowLimiter("AuthRatePolicy", opt =>
    {
        opt.Window                = TimeSpan.FromMinutes(1);
        opt.PermitLimit           = 5;
        opt.QueueProcessingOrder  = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit            = 0;
    });

    options.AddFixedWindowLimiter("SearchRatePolicy", opt =>
    {
        opt.Window                = TimeSpan.FromSeconds(10);
        opt.PermitLimit           = 20; // Max 20 searches per 10 seconds
        opt.QueueProcessingOrder  = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit            = 2;
    });

    options.AddFixedWindowLimiter("ReviewSubmissionPolicy", opt =>
    {
        opt.Window                = TimeSpan.FromMinutes(5);
        opt.PermitLimit           = 3; // Max 3 reviews per 5 mins
        opt.QueueProcessingOrder  = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit            = 0;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// ── Caching & Compression ─────────────────────────────────────────────────────
builder.Services.AddResponseCaching();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});
builder.Services.AddMemoryCache();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// ── Application Services ──────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService,     AuthService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService,  ProductService>();
builder.Services.AddScoped<ICartService,     CartService>();
builder.Services.AddScoped<IOrderService,    OrderService>();
builder.Services.AddScoped<IImageService,    ImageService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

builder.Services.AddValidatorsFromAssembly(typeof(Shopfinity.Application.Features.Carts.DTOs.AddToCartDto).Assembly);

// ── Health Checks ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

// ── API Controllers + Validation ──────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        // Serialize enums as strings (e.g. OrderStatus)
        o.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Any() == true)
                .SelectMany(e => e.Value!.Errors.Select(err => err.ErrorMessage))
                .ToList();

            return new BadRequestObjectResult(new Shopfinity.API.Responses.ApiErrorResponse
            {
                Success = false,
                Message = "Validation failed.",
                StatusCode = StatusCodes.Status400BadRequest,
                Errors = errors
            });
        };
    });

builder.Services.AddOpenApi();

// ── API Versioning ────────────────────────────────────────────────────────────
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion            = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions            = true;
});

// ── Request Timeouts (ASP.NET 8+) ─────────────────────────────────────────────
builder.Services.AddRequestTimeouts(options =>
{
    options.DefaultPolicy = new Microsoft.AspNetCore.Http.Timeouts.RequestTimeoutPolicy
    {
        Timeout = TimeSpan.FromSeconds(30),
        TimeoutStatusCode = StatusCodes.Status408RequestTimeout
    };

    options.AddPolicy("AuthTimeout", TimeSpan.FromSeconds(10));
});

// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<Shopfinity.API.Middleware.CorrelationIdMiddleware>();
app.UseMiddleware<Shopfinity.API.Middleware.ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// ── CORS must be before auth ─────────────────────────────────────────────────
app.UseCors("FrontendPolicy");

app.UseResponseCompression();
app.UseResponseCaching();

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000"); // 1 year cache
    }
});

app.UseRateLimiter();
app.UseRequestTimeouts(); // Added before auth
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<Shopfinity.API.Middleware.CsrfValidationMiddleware>();

app.MapControllers();

// ── Health Check endpoint ─────────────────────────────────────────────────────
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (ctx, report) =>
    {
        ctx.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status  = report.Status.ToString(),
            checks  = report.Entries.Select(e => new
            {
                name    = e.Key,
                status  = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        });
        await ctx.Response.WriteAsync(result);
    }
});

// ── Database Seeding (Development Only) ───────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await Shopfinity.Infrastructure.Data.DatabaseSeeder.SeedAsync(db, userManager, roleManager);
        Log.Information("✅ Database seeding completed successfully.");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "❌ Database seeding failed. The application will continue running without seeded data. " +
            "Check your PostgreSQL connection string and ensure the database server is running.");
    }
}

app.Run();
