# LxfmlSharp

A .NET 10 solution for parsing LEGO models (LXFML format) and storing them via a RESTful API backed by AWS.

## Quick Start

> Need to have Docker installed and running!

```bash
dotnet run --project LxfmlSharp.AppHost
```

This spins up **LocalStack** (local AWS: S3 + DynamoDB) and the API on `http://localhost:5130`.

> **Note:** If you encounter port binding errors, see [TROUBLESHOOTING.md](TROUBLESHOOTING.md#port-binding-errors-in-aspire)

## Testing

### In Your IDE

Open `LxfmlSharp.Api/LxfmlSharp.Api.http` and send requests directly from the editor:

```http
POST http://localhost:5130/api/v1/models
Content-Type: application/json

{
  "name": "My Model",
  "partCount": 1,
  "parts": [
    {
      "uuid": "p-1",
      "designId": 3001,
      "transformMatrix": [1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0]
    }
  ]
}
```

Or use the **interactive API docs**: `http://localhost:5130/scalar/v1`

### CLI

```bash
dotnet run --project LxfmlSharp.Cli -- model.lxfml
```

### Unit Tests

```bash
dotnet test
```

## Architecture

- **Core** — Streaming XML parser with validation
- **Api** — ASP.NET Core endpoints + AWS integration (via LocalStack)
- **Cli** — Command-line tool
- **AppHost** — Docker orchestration (LocalStack + API)

## Features

- ✓ Full CRUD API
- ✓ Streaming parser with line-number error reporting
- ✓ Local AWS emulation (S3 + DynamoDB)
- ✓ Comprehensive tests
- ✓ OpenTelemetry observability
- ✓ Health checks with exponential backoff

## Unity Viewer

A Unity-based 3D viewer for visualizing LXFML models loaded from the API.

### Setup

1. Open the `LxfmlViewer` folder in Unity (Unity 6+)
2. Ensure the API is running: `dotnet run --project LxfmlSharp.AppHost`
3. Open the `MainScene` and press Play
4. Enter a model ID and click "Load Model"

### Features

- ✓ Real-time model loading from API
- ✓ Automatic transformation matrix parsing
- ✓ Exquisite multi-axis rotation animation
- ✓ Fancy part coloring for visualization
- ✓ Model replacement on reload