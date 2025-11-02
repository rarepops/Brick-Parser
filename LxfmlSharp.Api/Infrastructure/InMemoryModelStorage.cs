using System.Collections.Concurrent;
using LxfmlSharp.Application.Contracts;
using LxfmlSharp.Application.DTOs;

namespace LxfmlSharp.Api.Infrastructure;

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

    public Task<bool> DeleteModelAsync(string modelId, CancellationToken ct = default)
    {
        var removed = Store.TryRemove(modelId, out _);
        return Task.FromResult(removed);
    }
}
