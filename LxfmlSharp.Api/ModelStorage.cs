using System.Collections.Concurrent;

using LxfmlSharp.Api.Contracts;

namespace LxfmlSharp.Api;

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
