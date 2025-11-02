namespace LxfmlSharp.Api.Contracts;

public interface IModelStorage
{
    Task SaveModelAsync(string modelId, ModelDto model, CancellationToken ct = default);
    Task<ModelDto?> GetModelAsync(string modelId, CancellationToken ct = default);
}
