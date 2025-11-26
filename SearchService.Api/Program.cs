using Elastic.Clients.Elasticsearch;
using Dapr.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SearchService.Application.DTOs;
using SearchService.Application.Interfaces;
using SearchService.Application.Services;
using SearchService.Infrastructure.Constants;
using SearchService.Infrastructure.Elasticsearch;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure Elasticsearch
var elasticsearchSettings = builder.Configuration.GetSection("Elasticsearch");
var connectionString = elasticsearchSettings["ConnectionString"] ?? "https://my-elasticsearch-project-ec5b4b.es.us-central1.gcp.elastic.cloud:443";
var apiKey = elasticsearchSettings["ApiKey"] ?? "UzR1WnY1b0JHWEl6aUZaeTJGY0I6TXZsd3poUUl3Q25MMjFCcWNmenBwZw==";

builder.Services.AddSingleton<IElasticsearchClientFactory>(_ => 
    new ElasticsearchClientFactory(connectionString, apiKey));
Console.WriteLine("Elastic Search Started...");

// builder.Services.AddSingleton<IElasticsearchClientFactory>(_ => new ElasticsearchClientFactory(connectionString));
builder.Services.AddScoped<IElasticsearchService, ElasticsearchService>();
builder.Services.AddScoped<IBusinessSearchService, BusinessSearchService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Dapr
builder.Services.AddDaprClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "User Service API v1");
    options.RoutePrefix = ""; // load Swagger at root
});

app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.UseCloudEvents();
app.MapSubscribeHandler();

// Initialize Elasticsearch index on startup
// Initialize Elasticsearch indices on startup
using (var scope = app.Services.CreateScope())
{
    var elasticsearchService = scope.ServiceProvider.GetRequiredService<IElasticsearchService>();
    
    // Test connection first
    Console.WriteLine("Testing Elasticsearch connection...");
    var connectionSuccess = await elasticsearchService.TestConnectionAsync();
    
    if (!connectionSuccess)
    {
        Console.WriteLine("Cannot proceed with index creation - Elasticsearch connection failed");
        return;
    }
    
    try
    {
        // Create business index
        var res = await elasticsearchService.CreateIndexAsync<BusinessIndexDto>(IndexNames.Businesses);
        Console.WriteLine(res? "Business index created successfully" : "Error creating business index");
        
        // Add more indices here as needed
        // await elasticsearchService.CreateIndexAsync<ReviewIndexDto>(IndexNames.Reviews);
        // await elasticsearchService.CreateIndexAsync<NotificationIndexDto>(IndexNames.Notifications);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to create Elasticsearch indices: {ex.Message}");
    }
}

app.Run();