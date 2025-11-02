using System.Diagnostics;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using Amazon.S3.Model;

namespace LxfmlSharp.Api;

public sealed class AWSModelStorage : IModelStorage
{
    private static readonly ActivitySource _activitySource = new("LxfmlSharp.Storage");
    private readonly IAmazonS3 _s3;
    private readonly IAmazonDynamoDB _dynamo;
    private readonly ILogger<AWSModelStorage> _logger;

    private const string BucketName = "lxfml-models";
    private const string TableName = "Models";

    public AWSModelStorage(IAmazonS3 s3, IAmazonDynamoDB dynamo, ILogger<AWSModelStorage> logger)
    {
        _s3 = s3;
        _dynamo = dynamo;
        _logger = logger;

        // Ensure resources exist on startup (dev only)
        InitializeAsync().GetAwaiter().GetResult();
    }

    private async Task InitializeAsync()
    {
        var maxRetries = 5;
        var retryDelay = TimeSpan.FromSeconds(3);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                _logger.LogInformation(
                    "Initializing LocalStack (attempt {Attempt}/{Max})...",
                    attempt,
                    maxRetries
                );

                // Test connectivity first
                var buckets = await _s3.ListBucketsAsync();

                if (buckets == null)
                {
                    throw new InvalidOperationException("Failed to list S3 buckets");
                }

                var bucketCount = buckets?.Buckets?.Count ?? 0;

                _logger.LogInformation(
                    "LocalStack S3 is ready - found {Count} buckets",
                    bucketCount
                );

                // Create bucket if needed
                if (buckets?.Buckets?.Any(b => b.BucketName == BucketName) != true)
                {
                    await _s3.PutBucketAsync(BucketName);
                    _logger.LogInformation("Created bucket: {Bucket}", BucketName);
                }

                // Create DynamoDB table
                var tables = await _dynamo.ListTablesAsync();
                if (!tables.TableNames.Contains(TableName))
                {
                    await _dynamo.CreateTableAsync(
                        new CreateTableRequest
                        {
                            TableName = TableName,
                            KeySchema = [new KeySchemaElement("ModelId", KeyType.HASH)],
                            AttributeDefinitions =
                            [
                                new AttributeDefinition("ModelId", ScalarAttributeType.S),
                            ],
                            BillingMode = BillingMode.PAY_PER_REQUEST,
                        }
                    );
                    _logger.LogInformation("Created table: {Table}", TableName);
                }

                _logger.LogInformation("LocalStack resources initialized");
                return;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                _logger.LogWarning(
                    ex,
                    "LocalStack not ready (attempt {Attempt}/{Max}), retrying in {Delay}s...",
                    attempt,
                    maxRetries,
                    retryDelay.TotalSeconds
                );
                await Task.Delay(retryDelay);
                retryDelay = TimeSpan.FromSeconds(retryDelay.TotalSeconds * 1.5); // Exponential backoff
            }
        }

        throw new InvalidOperationException(
            $"Failed to initialize LocalStack after {maxRetries} attempts"
        );
    }

    public async Task SaveModelAsync(string modelId, ModelDto model, CancellationToken ct = default)
    {
        using var activity = _activitySource.StartActivity("SaveModel", ActivityKind.Internal);
        activity?.SetTag("modelId", modelId);
        activity?.SetTag("modelName", model.Name);
        activity?.SetTag("partCount", model.PartCount);

        var json = JsonSerializer.Serialize(model);

        _logger.LogInformation(
            "Saving model {ModelId} ({PartCount} parts) to S3",
            modelId,
            model.PartCount
        );
        await _s3.PutObjectAsync(
            new PutObjectRequest
            {
                BucketName = BucketName,
                Key = $"{modelId}.json",
                ContentBody = json,
                ContentType = "application/json",
            },
            ct
        );

        _logger.LogInformation("Saving metadata to DynamoDB");
        await _dynamo.PutItemAsync(
            new PutItemRequest
            {
                TableName = TableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    ["ModelId"] = new AttributeValue { S = modelId },
                    ["Name"] = new AttributeValue { S = model.Name },
                    ["PartCount"] = new AttributeValue { N = model.PartCount.ToString() },
                    ["CreatedAt"] = new AttributeValue { S = DateTime.UtcNow.ToString("O") },
                },
            },
            ct
        );

        _logger.LogInformation("Persisted model {modelId}", modelId);
    }

    public async Task<ModelDto?> GetModelAsync(string modelId, CancellationToken ct = default)
    {
        using var activity = _activitySource.StartActivity("GetModel", ActivityKind.Internal);
        activity?.SetTag("modelId", modelId);

        try
        {
            _logger.LogInformation("Retrieving model {ModelId} from S3", modelId);
            var response = await _s3.GetObjectAsync(
                new GetObjectRequest { BucketName = BucketName, Key = $"{modelId}.json" },
                ct
            );

            using var reader = new StreamReader(response.ResponseStream);
            var json = await reader.ReadToEndAsync(ct);
            var model = JsonSerializer.Deserialize<ModelDto>(json);

            activity?.SetTag("partCount", model?.PartCount ?? 0);
            _logger.LogInformation("Retrieved model {ModelId}", modelId);
            return model;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Model {ModelId} not found in S3", modelId);
            activity?.AddEvent(new ActivityEvent("ModelNotFound"));
            return null;
        }
    }
}
