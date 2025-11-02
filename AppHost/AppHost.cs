var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.LxfmlSharp_Api>("lxfmlsharp-api");

builder.Build().Run();
