using LxfmlSharp.Application.Contracts;
using LxfmlSharp.Application.DTOs;

namespace LxfmlSharp.Api.Tests.Infrastructure;

public sealed class FakeModelStorage : IModelStorage
{
    private readonly Dictionary<string, ModelDto> _models = new(StringComparer.Ordinal);
    private readonly object _sync = new();

    public bool ThrowOnSave { get; set; }
    public bool ThrowOnGet { get; set; }
    public bool ThrowOnDelete { get; set; }

    public Task SaveModelAsync(
        string modelId,
        ModelDto model,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (ThrowOnSave)
        {
            throw new InvalidOperationException("Save failure");
        }

        lock (_sync)
        {
            _models[modelId] = model;
        }

        return Task.CompletedTask;
    }

    public Task<ModelDto?> GetModelAsync(
        string modelId,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (ThrowOnGet)
        {
            throw new InvalidOperationException("Get failure");
        }

        lock (_sync)
        {
            _models.TryGetValue(modelId, out var model);
            return Task.FromResult(model);
        }
    }

    public Task<bool> DeleteModelAsync(
        string modelId,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (ThrowOnDelete)
        {
            throw new InvalidOperationException("Delete failure");
        }

        lock (_sync)
        {
            return Task.FromResult(_models.Remove(modelId));
        }
    }

    public bool Contains(string modelId)
    {
        lock (_sync)
        {
            return _models.ContainsKey(modelId);
        }
    }
}
