using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SearchService.Api.Middleware;
using SearchService.Application.Interfaces;
using SearchService.Application.Mapping;
using SearchService.Application.Services;
using SearchService.Domain.Constants;
using SearchService.Domain.Models;
using SearchService.Infrastructure.Elasticsearch;
using SearchService.Infrastructure.Kafka;

var builder = WebApplication.CreateBuilder(args);

// ──────────────────────────────────────────────────────────────
// Kestrel
// ──────────────────────────────────────────────────────────────
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5003);
});

// ──────────────────────────────────────────────────────────────
// CORS
// ──────────────────────────────────────────────────────────────
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("ConfiguredOrigins", policy =>
    {
        if (allowedOrigins.Length == 0 || allowedOrigins.Contains("*"))
        {
            // Development only — a wildcard origin must not be used in production
            if (!builder.Environment.IsProduction())
                policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            else
                throw new InvalidOperationException(
                    "Cors:AllowedOrigins must be explicitly set in production. Wildcard (*) is not permitted.");
        }
        else
        {
            policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader();
        }
    });
});

// ──────────────────────────────────────────────────────────────
// Authentication & Authorization (JWT Bearer)
// ──────────────────────────────────────────────────────────────
var authConfig = builder.Configuration.GetSection("Authentication");
var authority = authConfig["Authority"];

if (string.IsNullOrWhiteSpace(authority) && builder.Environment.IsProduction())
    throw new InvalidOperationException(
        "Authentication:Authority must be configured in production.");

if (!string.IsNullOrWhiteSpace(authority))
{
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = authority;
            options.Audience = authConfig["Audience"] ?? "search-service";
            options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            };
        });
}
else
{
    // No authority configured — allow everything (development only, enforced above in prod)
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options => options.RequireHttpsMetadata = false);
}

builder.Services.AddAuthorization();

// ──────────────────────────────────────────────────────────────
// Rate Limiting
// ──────────────────────────────────────────────────────────────
var rlConfig = builder.Configuration.GetSection("RateLimiting");

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("search", limiter =>
    {
        limiter.Window = TimeSpan.FromSeconds(rlConfig.GetValue<int>("WindowSeconds", 60));
        limiter.PermitLimit = rlConfig.GetValue<int>("RequestLimit", 100);
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 0;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// ──────────────────────────────────────────────────────────────
// Controllers + Dapr
// ──────────────────────────────────────────────────────────────
builder.Services.AddControllers().AddDapr();

// ──────────────────────────────────────────────────────────────
// Swagger / OpenAPI
// ──────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Search Service API",
        Version = "v1",
        Description = "Provides search functionality for CleReview"
    });

    // Swagger auth support — lets you test protected endpoints from the UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter your JWT access token."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            []
        }
    });
});

// ──────────────────────────────────────────────────────────────
// Elasticsearch
// ──────────────────────────────────────────────────────────────
var esConfig = builder.Configuration.GetSection("Elasticsearch");
var cloudId = esConfig["CloudId"]
    ?? throw new InvalidOperationException("Elasticsearch:CloudId is required. Set it via environment variable or secrets.");
var apiKey = esConfig["ApiKey"]
    ?? throw new InvalidOperationException("Elasticsearch:ApiKey is required. Set it via environment variable or secrets.");

builder.Services.AddSingleton<IElasticsearchClientFactory>(_ =>
    new ElasticsearchClientFactory(cloudId, apiKey));

builder.Services.AddSingleton(provider =>
    provider.GetRequiredService<IElasticsearchClientFactory>().CreateClient());

// Register ElasticsearchService once and expose it under both interfaces
builder.Services.AddScoped<ElasticsearchService>();
builder.Services.AddScoped<IElasticsearchService>(sp => sp.GetRequiredService<ElasticsearchService>());
builder.Services.AddScoped<IElasticsearchHealthService>(sp => sp.GetRequiredService<ElasticsearchService>());

// ──────────────────────────────────────────────────────────────
// Application Services
// ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IBusinessIndexMapper, BusinessIndexMapper>();
builder.Services.AddScoped<IBusinessSearchService, BusinessSearchService>();

// ──────────────────────────────────────────────────────────────
// Dapr Client + Kafka
// ──────────────────────────────────────────────────────────────
builder.Services.AddDaprClient();
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();

// ──────────────────────────────────────────────────────────────
// Health Checks
// ──────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks();

// ──────────────────────────────────────────────────────────────
// Build
// ──────────────────────────────────────────────────────────────
var app = builder.Build();

// ──────────────────────────────────────────────────────────────
// Middleware pipeline (order matters)
// ──────────────────────────────────────────────────────────────
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Search Service API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseCors("ConfiguredOrigins");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/healthz");
app.MapSubscribeHandler();   // Dapr pub/sub subscription handler
app.MapControllers();

// ──────────────────────────────────────────────────────────────
// Elasticsearch index bootstrap
// ──────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var health = scope.ServiceProvider.GetRequiredService<IElasticsearchHealthService>();
    var es = scope.ServiceProvider.GetRequiredService<IElasticsearchService>();

    var connected = await health.IsHealthyAsync();
    if (connected)
    {
        app.Logger.LogInformation("Elasticsearch connected. Ensuring index exists…");
        await es.CreateIndexAsync<BusinessIndexDto>(IndexNames.Businesses);
    }
    else
    {
        app.Logger.LogWarning("Elasticsearch is unreachable. Index bootstrap skipped — check configuration.");
    }
}

app.Run();
