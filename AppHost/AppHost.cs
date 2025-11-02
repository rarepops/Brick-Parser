var builder = DistributedApplication.CreateBuilder(args);

var localstack = builder
    .AddContainer("localstack", "localstack/localstack", "latest")
    .WithHttpEndpoint(port: 4566, targetPort: 4566, name: "edge")
    .WithEnvironment("SERVICES", "s3,dynamodb")
    .WithEnvironment("DEBUG", "1")
    .WithEnvironment("EAGER_SERVICE_LOADING", "1")
    .WithEnvironment("AWS_DEFAULT_REGION", "us-east-1")
    .WithEnvironment("LOCALSTACK_HOST", "localhost.localstack.cloud:4566");

var api = builder
    .AddProject<Projects.LxfmlSharp_Api>("api")
    .WithEnvironment("LocalStack__UseLocalStack", "true")
    .WithEnvironment("LocalStack__Config__LocalStackHost", "localhost")
    .WithEnvironment("LocalStack__Config__EdgePort", "4566")
    .WithEnvironment("LocalStack__Session__RegionName", "us-east-1");

builder.Build().Run();
