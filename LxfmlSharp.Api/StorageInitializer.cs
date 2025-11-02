using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using Amazon.S3.Model;

namespace LxfmlSharp.Api;

public sealed class StorageInitializer : IHostedService
{
    private readonly IAmazonS3 _s3;
    private readonly IAmazonDynamoDB _dynamo;
    private readonly ILogger<StorageInitializer> _logger;
    private readonly IConfiguration _configuration;

    public StorageInitializer(
        IAmazonS3 s3,
        IAmazonDynamoDB dynamo,
        ILogger<StorageInitializer> logger,
        IConfiguration configuration)
    {
        _s3 = s3;
        _dynamo = dynamo;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var bucketName = _configuration["AWS:BucketName"] ?? "lxfml-models";
        var tableName = _configuration["AWS:TableName"] ?? "Models";
        
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

                var buckets = await _s3.ListBucketsAsync(cancellationToken);
                if (buckets == null)
                {
                    throw new InvalidOperationException("Failed to list S3 buckets");
                }

                _logger.LogInformation(
                    "LocalStack S3 is ready - found {Count} buckets",
                    buckets.Buckets?.Count ?? 0
                );

                if (buckets?.Buckets?.Any(b => b.BucketName == bucketName) != true)
                {
                    await _s3.PutBucketAsync(bucketName, cancellationToken);
                    _logger.LogInformation("Created bucket: {Bucket}", bucketName);
                }

                var tables = await _dynamo.ListTablesAsync(cancellationToken);
                if (!tables.TableNames.Contains(tableName))
                {
                    await _dynamo.CreateTableAsync(
                        new CreateTableRequest
                        {
                            TableName = tableName,
                            KeySchema = [new KeySchemaElement("ModelId", KeyType.HASH)],
                            AttributeDefinitions =
                            [
                                new AttributeDefinition("ModelId", ScalarAttributeType.S),
                            ],
                            BillingMode = BillingMode.PAY_PER_REQUEST,
                        },
                        cancellationToken
                    );
                    _logger.LogInformation("Created table: {Table}", tableName);
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
                await Task.Delay(retryDelay, cancellationToken);
                retryDelay = TimeSpan.FromSeconds(retryDelay.TotalSeconds * 1.5);
            }
        }

        throw new InvalidOperationException(
            $"Failed to initialize LocalStack after {maxRetries} attempts"
        );
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}