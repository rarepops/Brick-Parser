using LxfmlSharp.Api.Tests.Infrastructure;
using LxfmlSharp.Application.DTOs;

namespace LxfmlSharp.Api.Tests.Endpoints;

[ParallelGroup("GetModelEndpoint")]
public class GetModelEndpointTests : ApiTestBase
{
    [Test]
    public async Task GetModel_WhenModelExists_ReturnsModel()
    {
        // Arrange
        await using var host = await CreateHostAsync();
        var model = new ModelDto
        {
            Name = "Existing Model",
            PartCount = 2,
            Parts = new()
            {
                new PartDto
                {
                    Uuid = "uuid1",
                    DesignId = 1,
                    TransformMatrix = new float[16],
                },
                new PartDto
                {
                    Uuid = "uuid2",
                    DesignId = 2,
                    TransformMatrix = new float[16],
                },
            },
        };
        await host.Storage.SaveModelAsync("existing-id", model);

        // Act
        var response = await host.Client.GetAsync("/api/v1/models/existing-id");

        // Assert
        var returned = await response.Content.ReadFromJsonAsync<ModelDto>();
        await Verify(new { response.StatusCode, Model = returned });
    }

    [Test]
    public async Task GetModel_WhenMissing_ReturnsNotFound()
    {
        // Arrange
        await using var host = await CreateHostAsync();

        // Act
        var response = await host.Client.GetAsync("/api/v1/models/missing-id");

        // Assert
        await Verify(response.StatusCode);
    }

    [Test]
    public async Task GetModel_WhenStorageThrows_ReturnsInternalServerError()
    {
        // Arrange
        await using var host = await CreateHostAsync();
        host.Storage.ThrowOnGet = true;

        // Act
        var response = await host.Client.GetAsync("/api/v1/models/any-id");

        // Assert
        await Verify(response.StatusCode);
    }
}
