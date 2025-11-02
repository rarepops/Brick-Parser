using LxfmlSharp.Api.Tests.Infrastructure;
using LxfmlSharp.Application.DTOs;

namespace LxfmlSharp.Api.Tests.Endpoints;

[ParallelGroup("DeleteModelEndpoint")]
public class DeleteModelEndpointTests : ApiTestBase
{
    [Test]
    public async Task DeleteModel_WhenModelExists_RemovesAndReturnsNoContent()
    {
        // Arrange
        await using var host = await CreateHostAsync();
        var model = new ModelDto
        {
            Name = "Delete Me",
            PartCount = 0,
            Parts = new(),
        };
        await host.Storage.SaveModelAsync("delete-id", model);

        // Act
        var response = await host.Client.DeleteAsync("/api/v1/models/delete-id");

        // Assert
        await Verify(
            new { response.StatusCode, StillInStorage = host.Storage.Contains("delete-id") }
        );
    }

    [Test]
    public async Task DeleteModel_WhenMissing_ReturnsNotFound()
    {
        // Arrange
        await using var host = await CreateHostAsync();

        // Act
        var response = await host.Client.DeleteAsync("/api/v1/models/missing-id");

        // Assert
        await Verify(response.StatusCode);
    }

    [Test]
    public async Task DeleteModel_MultipleDeletes_ReturnsNotFoundAfterFirst()
    {
        // Arrange
        await using var host = await CreateHostAsync();
        var model = new ModelDto
        {
            Name = "Double Delete",
            PartCount = 0,
            Parts = new(),
        };
        await host.Storage.SaveModelAsync("double-delete-id", model);

        // Act
        var firstResponse = await host.Client.DeleteAsync("/api/v1/models/double-delete-id");
        var secondResponse = await host.Client.DeleteAsync("/api/v1/models/double-delete-id");

        // Assert
        await Verify(
            new { FirstStatus = firstResponse.StatusCode, SecondStatus = secondResponse.StatusCode }
        );
    }

    [Test]
    public async Task DeleteModel_WhenStorageThrows_ReturnsInternalServerError()
    {
        // Arrange
        await using var host = await CreateHostAsync();
        host.Storage.ThrowOnDelete = true;

        // Act
        var response = await host.Client.DeleteAsync("/api/v1/models/problem-id");

        // Assert
        await Verify(response.StatusCode);
    }
}
