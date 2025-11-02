using LocalStack.Client.Extensions;
using LxfmlSharp.Api;

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

// AuthZ can stay; no policies needed yet
builder.Services.AddAuthorization();

// Built-in OpenAPI
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IModelStorage, AWSModelStorage>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// API group
var models = app.MapGroup("/api/models");

models
    .MapPost(
        "/",
        async (ModelDto model, IModelStorage storage, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return Results.BadRequest(new { error = "Name is required" });
            }

            var modelId = Guid.NewGuid().ToString("n");
            await storage.SaveModelAsync(modelId, model, ct);
            return Results.Created($"/api/models/{modelId}", new { modelId });
        }
    )
    .WithSummary("Upload a parsed LXFML model")
    .Produces(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest);

models
    .MapGet(
        "/{modelId}",
        async (string modelId, IModelStorage storage, CancellationToken ct) =>
        {
            var model = await storage.GetModelAsync(modelId, ct);
            return model is null ? Results.NotFound() : Results.Ok(model);
        }
    )
    .WithSummary("Get a model by id")
    .Produces<ModelDto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

app.Run();
