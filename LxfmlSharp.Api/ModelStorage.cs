using System.Collections.Concurrent;

namespace LxfmlSharp.Api;

public record ModelDto(string Name, int PartCount, PartDto[] Parts);
public record PartDto(string Uuid, int DesignId, float[] TransformMatrix);

public interface IModelStorage
{
    Task SaveModelAsync(string modelId, ModelDto model, CancellationToken ct = default);
    Task<ModelDto?> GetModelAsync(string modelId, CancellationToken ct = default);
}

// In-memory storage for now
public sealed class InMemoryModelStorage : IModelStorage
{
    private static readonly ConcurrentDictionary<string, ModelDto> Store = new();

    public Task SaveModelAsync(string modelId, ModelDto model, CancellationToken ct = default)
    {
        Store[modelId] = model;
        return Task.CompletedTask;
    }

    public Task<ModelDto?> GetModelAsync(string modelId, CancellationToken ct = default)
    {
        return Task.FromResult(Store.TryGetValue(modelId, out var m) ? m : null);
    }
}
