using LxfmlSharp.Api.Endpoints;
using LxfmlSharp.Application.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LxfmlSharp.Api.Tests.Infrastructure;

public abstract class ApiTestBase
{
    protected static async Task<TestHost> CreateHostAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<FakeModelStorage>();
        builder.Services.AddSingleton<IModelStorage>(sp =>
            sp.GetRequiredService<FakeModelStorage>()
        );
        builder.Services.AddLogging(logging => logging.SetMinimumLevel(LogLevel.Warning));

        var app = builder.Build();
        app.MapGroup("/api/v1/models").MapModelEndpoints();

        await app.StartAsync();
        var client = app.GetTestClient();
        client.BaseAddress = new Uri("http://localhost");

        return new TestHost(app, client, app.Services.GetRequiredService<FakeModelStorage>());
    }

    protected sealed class TestHost : IAsyncDisposable
    {
        private readonly WebApplication _app;

        internal TestHost(WebApplication app, HttpClient client, FakeModelStorage storage)
        {
            _app = app;
            Client = client;
            Storage = storage;
        }

        internal HttpClient Client { get; }
        internal FakeModelStorage Storage { get; }

        public async ValueTask DisposeAsync()
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
    }
}
