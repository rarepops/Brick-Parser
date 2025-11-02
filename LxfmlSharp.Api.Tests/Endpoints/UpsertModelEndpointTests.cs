using LxfmlSharp.Api.Tests.Infrastructure;
using LxfmlSharp.Application.DTOs;

namespace LxfmlSharp.Api.Tests.Endpoints;

[ParallelGroup("UpsertModelEndpoint")]
public class UpsertModelEndpointTests : ApiTestBase
{
    [Test]
    public async Task UpsertModel_SavesAndReturnsNoContent()
    {
        // Arrange
        await using var host = await CreateHostAsync();
        var payload = new ModelDto
        {
            Name = "Upserted Model",
            PartCount = 0,
            Parts = new(),
        };

        // Act
        var response = await host.Client.PutAsJsonAsync("/api/v1/models/upsert-id", payload);

        // Assert
        var stored = await host.Storage.GetModelAsync("upsert-id");
        await Verify(
            new
            {
                response.StatusCode,
                InStorage = host.Storage.Contains("upsert-id"),
                StoredModel = stored,
            }
        );
    }

    [Test]
    public async Task UpsertModel_OverwritesExistingModel()
    {
        // Arrange
        await using var host = await CreateHostAsync();
        var original = new ModelDto
        {
            Name = "Original",
            PartCount = 1,
            Parts = new()
            {
                new PartDto
                {
                    Uuid = "old",
                    DesignId = 1,
                    TransformMatrix = new float[16],
                },
            },
        };
        await host.Storage.SaveModelAsync("overwrite-id", original);
        var updated = new ModelDto
        {
            Name = "Updated",
            PartCount = 2,
            Parts = new()
            {
                new PartDto
                {
                    Uuid = "new",
                    DesignId = 2,
                    TransformMatrix = new float[16],
                },
            },
        };

        // Act
        await host.Client.PutAsJsonAsync("/api/v1/models/overwrite-id", updated);

        // Assert
        var stored = await host.Storage.GetModelAsync("overwrite-id");
        await Verify(stored);
    }

    [Test]
    public async Task UpsertModel_WhenStorageThrows_ReturnsInternalServerError()
    {
        // Arrange
        await using var host = await CreateHostAsync();
        host.Storage.ThrowOnSave = true;
        var payload = new ModelDto
        {
            Name = "Will Fail",
            PartCount = 0,
            Parts = new(),
        };

        // Act
        var response = await host.Client.PutAsJsonAsync("/api/v1/models/fail-id", payload);

        // Assert
        await Verify(response.StatusCode);
    }
}
