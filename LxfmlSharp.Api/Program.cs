using LxfmlSharp.Api;

var builder = WebApplication.CreateBuilder(args);

// AuthZ can stay; no policies needed yet
builder.Services.AddAuthorization();

// Built-in OpenAPI
builder.Services.AddOpenApi();

// Register storage (mock for now)
builder.Services.AddSingleton<IModelStorage, InMemoryModelStorage>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // exposes /openapi/v1.json
}

app.UseHttpsRedirection();
app.UseAuthorization();

// API group
var models = app.MapGroup("/api/models");

// POST /api/models : accepts parsed JSON, returns new id
models.MapPost("/", async (ModelDto model, IModelStorage storage, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(model.Name))
    {
        return Results.BadRequest(new { error = "Name is required" });
    }

    var modelId = Guid.NewGuid().ToString("n");
    await storage.SaveModelAsync(modelId, model, ct);
    return Results.Created($"/api/models/{modelId}", new { modelId });
})
.WithSummary("Upload a parsed LXFML model")
.Produces(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest);

// GET /api/models/{id} : returns the model or 404
models.MapGet("/{modelId}", async (string modelId, IModelStorage storage, CancellationToken ct) =>
{
    var model = await storage.GetModelAsync(modelId, ct);
    return model is null ? Results.NotFound() : Results.Ok(model);
})
.WithSummary("Get a model by id")
.Produces<ModelDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

app.Run();
