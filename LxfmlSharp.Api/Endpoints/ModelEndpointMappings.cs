using LxfmlSharp.Application.Contracts;
using LxfmlSharp.Application.DTOs;

namespace LxfmlSharp.Api.Endpoints;

internal static class ModelEndpointMappings
{
    public static RouteGroupBuilder MapModelEndpoints(this RouteGroupBuilder group)
    {
        group
            .MapPost(
                "/",
                async (ModelDto model, IModelStorage storage, ILogger<Program> logger) =>
                {
                    try
                    {
                        var id = Guid.NewGuid().ToString();
                        await storage.SaveModelAsync(id, model);
                        logger.LogInformation(
                            "Created model {ModelId} with name {Name}",
                            id,
                            model.Name
                        );
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
            .WithDescription(
                "Saves a model with its parts to storage and returns the generated model ID"
            )
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        group
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

        group
            .MapPut(
                "/{modelId}",
                async (
                    string modelId,
                    ModelDto model,
                    IModelStorage storage,
                    ILogger<Program> logger,
                    CancellationToken ct
                ) =>
                {
                    try
                    {
                        await storage.SaveModelAsync(modelId, model, ct);
                        logger.LogInformation(
                            "Upserted model {ModelId} with name {Name}",
                            modelId,
                            model.Name
                        );
                        return Results.NoContent();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to upsert model {ModelId}", modelId);
                        return Results.Problem(
                            detail: "Failed to upsert model",
                            statusCode: StatusCodes.Status500InternalServerError
                        );
                    }
                }
            )
            .WithName("UpsertModel")
            .WithSummary("Create or overwrite a model by id")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status500InternalServerError);

        group
            .MapDelete(
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
                        var removed = await storage.DeleteModelAsync(modelId, ct);
                        if (!removed)
                        {
                            return Results.NotFound();
                        }

                        logger.LogInformation("Deleted model {ModelId}", modelId);
                        return Results.NoContent();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to delete model {ModelId}", modelId);
                        return Results.Problem(
                            detail: "Failed to delete model",
                            statusCode: StatusCodes.Status500InternalServerError
                        );
                    }
                }
            )
            .WithName("DeleteModel")
            .WithSummary("Delete a model by id")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        return group;
    }
}
