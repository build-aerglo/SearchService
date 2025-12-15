using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using SearchService.Application.Interfaces;
using SearchService.Application.Services;
using SearchService.Domain.Constants;
using SearchService.Domain.Models;
using SearchService.Infrastructure.Elasticsearch;
using SearchService.Infrastructure.Kafka;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5003);
});

// Controllers + Dapr
builder.Services.AddControllers().AddDapr();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Search Service API",
        Version = "v1",
        Description = "Provides search functionality for CleReview"
    });
});

// Elasticsearch
var esConfig = builder.Configuration.GetSection("Elasticsearch");
var cloudEndpoint = esConfig.GetValue<string>("CloudId");
var apiKey = esConfig.GetValue<string>("ApiKey");

builder.Services.AddSingleton<IElasticsearchClientFactory>(_ =>
    new ElasticsearchClientFactory(cloudEndpoint, apiKey));

builder.Services.AddSingleton(provider =>
{
    var factory = provider.GetRequiredService<IElasticsearchClientFactory>();
    return factory.CreateClient();
});

builder.Services.AddScoped<IElasticsearchService, ElasticsearchService>();
builder.Services.AddScoped<IBusinessSearchService, BusinessSearchService>();

// Dapr Client
builder.Services.AddDaprClient();

// Kafka producer (optional)
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();

// cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .SetIsOriginAllowed(_ => true)  // allow temporary until production
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Search Service API v1");
    options.RoutePrefix = "";
});


// Required for Dapr pub/sub handling
app.MapSubscribeHandler();

// Controllers
app.MapControllers();

// Elasticsearch Index initialization
using (var scope = app.Services.CreateScope())
{
    var es = scope.ServiceProvider.GetRequiredService<IElasticsearchService>();
    var ok = await es.TestConnectionAsync();

    if (ok)
    {
        Console.WriteLine(" Elasticsearch connected. Creating index…");
        await es.CreateIndexAsync<BusinessIndexDto>(IndexNames.Businesses);
    }
    else
    {
        Console.WriteLine(" Elasticsearch is unreachable. Check config.");
    }
}
app.UseCors("FrontendPolicy");
app.Run();
