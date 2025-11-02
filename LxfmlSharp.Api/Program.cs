using LocalStack.Client.Extensions;
using LxfmlSharp.Api;
using LxfmlSharp.Api.Contracts;
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

// AuthZ can stay; no policies needed yet
builder.Services.AddAuthorization();

// Built-in OpenAPI (.NET 9)
builder.Services.AddOpenApi();

// Add ProblemDetails for exception handling
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

models
    .MapPost(
        "/",
        async (ModelDto model, IModelStorage storage, ILogger<Program> logger) =>
        {
            try
            {
                var id = Guid.NewGuid().ToString();
                await storage.SaveModelAsync(id, model);
                logger.LogInformation("Created model {ModelId} with name {Name}", id, model.Name);
                return Results.Created($"/api/v1/models/{id}", new { modelId = id });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to save model {ModelName}", model.Name);
                return Results.Problem(
                    detail: "Failed to save model",
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        }
    )
    .WithName("CreateModel")
    .WithSummary("Upload a parsed LXFML model")
    .WithDescription("Saves a model with its parts to storage and returns the generated model ID")
    .Produces(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status500InternalServerError);

models
    .MapGet(
        "/{modelId}",
        async (
            string modelId,
            IModelStorage storage,
            ILogger<Program> logger,
            CancellationToken ct
        ) =>
        {
            try
            {
                var model = await storage.GetModelAsync(modelId, ct);
                return model is null ? Results.NotFound() : Results.Ok(model);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to retrieve model {ModelId}", modelId);
                return Results.Problem(
                    detail: "Failed to retrieve model",
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        }
    )
    .WithName("GetModel")
    .WithSummary("Get a model by id")
    .Produces<ModelDto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status500InternalServerError);

app.MapHealthChecks("/health");

// Redirect root to Scalar UI
app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();

app.Run();
