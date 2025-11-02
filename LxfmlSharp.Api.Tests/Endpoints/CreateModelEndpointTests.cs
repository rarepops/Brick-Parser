using System.Text.Json;
using LxfmlSharp.Api.Tests.Infrastructure;
using LxfmlSharp.Application.DTOs;

namespace LxfmlSharp.Api.Tests.Endpoints;

[ParallelGroup("CreateModelEndpoint")]
public class CreateModelEndpointTests : ApiTestBase
{
    [Test]
    public async Task CreateModel_ReturnsCreatedAndPersistsModel()
    {
        // Arrange
        await using var host = await CreateHostAsync();
        var payload = new ModelDto
        {
            Name = "Test Model",
            PartCount = 1,
            Parts = new()
            {
                new PartDto
                {
                    Uuid = "test-uuid",
                    DesignId = 123,
                    TransformMatrix = [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1],
                },
            },
        };

        // Act
        var response = await host.Client.PostAsJsonAsync("/api/v1/models", payload);

        // Assert
        using var document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync()
        );
        var modelId = document.RootElement.GetProperty("modelId").GetString();
        var stored = await host.Storage.GetModelAsync(modelId!);

        await Verify(
                new
                {
                    response.StatusCode,
                    HasLocation = response.Headers.Location != null,
                    ModelId = modelId,
                    StoredName = stored?.Name,
                }
            )
            .ScrubMember("ModelId");
    }

    [Test]
    public async Task CreateModel_WithEmptyParts_ReturnsCreated()
    {
        // Arrange
        await using var host = await CreateHostAsync();
        var payload = new ModelDto
        {
            Name = "Empty Model",
            PartCount = 0,
            Parts = new(),
        };

        // Act
        var response = await host.Client.PostAsJsonAsync("/api/v1/models", payload);

        // Assert
        await Verify(response.StatusCode);
    }

    [Test]
    public async Task CreateModel_WithManyParts_ReturnsCreated()
    {
        // Arrange
        await using var host = await CreateHostAsync();
        var parts = Enumerable
            .Range(1, 100)
            .Select(i => new PartDto
            {
                Uuid = $"uuid-{i}",
                DesignId = i,
                TransformMatrix = new float[16],
            })
            .ToList();
        var payload = new ModelDto
        {
            Name = "Large Model",
            PartCount = 100,
            Parts = parts,
        };

        // Act
        var response = await host.Client.PostAsJsonAsync("/api/v1/models", payload);

        // Assert
        using var document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync()
        );
        var modelId = document.RootElement.GetProperty("modelId").GetString();
        var stored = await host.Storage.GetModelAsync(modelId!);

        await Verify(new { response.StatusCode, PartCount = stored?.Parts.Count });
    }

    [Test]
    public async Task CreateModel_WhenStorageThrows_ReturnsInternalServerError()
    {
        // Arrange
        await using var host = await CreateHostAsync();
        host.Storage.ThrowOnSave = true;
        var payload = new ModelDto
        {
            Name = "Failure Model",
            PartCount = 0,
            Parts = new(),
        };

        // Act
        var response = await host.Client.PostAsJsonAsync("/api/v1/models", payload);

        // Assert
        await Verify(
            new
            {
                response.StatusCode,
                ContentType = response.Content.Headers.ContentType?.MediaType,
            }
        );
    }
}
