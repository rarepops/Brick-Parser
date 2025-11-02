using LocalStack.Client.Extensions;
using LxfmlSharp.Api;
using LxfmlSharp.Api.Endpoints;
using LxfmlSharp.Api.Infrastructure;
using LxfmlSharp.Application.Contracts;
using LxfmlSharp.Application.DTOs;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add LocalStack configuration from Aspire-injected env vars
builder.Services.AddLocalStack(builder.Configuration);

// Configure S3 client options for LocalStack compatibility
builder.Services.Configure<Amazon.S3.AmazonS3Config>(config =>
{
    config.ForcePathStyle = true;
    config.UseHttp = false;
});

// Register AWS SDK clients - auto-target LocalStack when enabled
builder.Services.AddAwsService<Amazon.S3.IAmazonS3>();
builder.Services.AddAwsService<Amazon.DynamoDBv2.IAmazonDynamoDB>();

builder.Services.AddSingleton<IModelStorage, AWSModelStorage>();
builder.Services.AddHostedService<StorageInitializer>();

builder.Services.AddAuthorization();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks().AddCheck<LocalStackHealthCheck>("localstack");

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();

var models = app.MapGroup("/api/v1/models").WithTags("Models");
models.MapModelEndpoints();

app.MapHealthChecks("/health");

// Redirect root to Scalar UI
app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();

app.Run();
