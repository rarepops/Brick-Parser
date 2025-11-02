using System.Diagnostics;
using System.Net;
using System.Text.Json;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using Amazon.S3.Model;

using LxfmlSharp.Application.Contracts;
using LxfmlSharp.Application.DTOs;

namespace LxfmlSharp.Api.Infrastructure;

public sealed class AWSModelStorage : IModelStorage
{
    private static readonly ActivitySource _activitySource = new("LxfmlSharp.Storage");
    private readonly IAmazonS3 _s3;
    private readonly IAmazonDynamoDB _dynamo;
    private readonly ILogger<AWSModelStorage> _logger;
    private readonly string _bucketName;
    private readonly string _tableName;

    public AWSModelStorage(
        IAmazonS3 s3,
        IAmazonDynamoDB dynamo,
        ILogger<AWSModelStorage> logger,
        IConfiguration configuration
    )
    {
        _s3 = s3;
        _dynamo = dynamo;
        _logger = logger;
        _bucketName = configuration["AWS:BucketName"] ?? "lxfml-models";
        _tableName = configuration["AWS:TableName"] ?? "Models";
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
                BucketName = _bucketName,
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
                TableName = _tableName,
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
                new GetObjectRequest { BucketName = _bucketName, Key = $"{modelId}.json" },
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

    public async Task<bool> DeleteModelAsync(string modelId, CancellationToken ct = default)
    {
        using var activity = _activitySource.StartActivity("DeleteModel", ActivityKind.Internal);
        activity?.SetTag("modelId", modelId);

        _logger.LogInformation("Deleting model {ModelId} from DynamoDB and S3", modelId);

        var deleteResponse = await _dynamo.DeleteItemAsync(
            new DeleteItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    ["ModelId"] = new AttributeValue { S = modelId },
                },
                ReturnValues = ReturnValue.ALL_OLD,
            },
            ct
        );

        var existed = deleteResponse.Attributes?.Count > 0;

        try
        {
            await _s3.DeleteObjectAsync(
                new DeleteObjectRequest { BucketName = _bucketName, Key = $"{modelId}.json" },
                ct
            );
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogDebug("Model {ModelId} object was not present in S3", modelId);
        }

        _logger.LogInformation(
            existed ? "Deleted model {ModelId}" : "Model {ModelId} did not exist",
            modelId
        );

        return existed;
    }
}
