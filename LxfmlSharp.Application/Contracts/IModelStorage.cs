using LxfmlSharp.Application.DTOs;

namespace LxfmlSharp.Application.Contracts;

public interface IModelStorage
{
    Task SaveModelAsync(string modelId, ModelDto model, CancellationToken ct = default);
    Task<ModelDto?> GetModelAsync(string modelId, CancellationToken ct = default);
    Task<bool> DeleteModelAsync(string modelId, CancellationToken ct = default);
}
