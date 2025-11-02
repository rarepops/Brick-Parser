using Amazon.S3;

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LxfmlSharp.Api;

public sealed class LocalStackHealthCheck : IHealthCheck
{
    private readonly IAmazonS3 _s3;
    private readonly ILogger<LocalStackHealthCheck> _logger;

    public LocalStackHealthCheck(IAmazonS3 s3, ILogger<LocalStackHealthCheck> logger)
    {
        _s3 = s3;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await _s3.ListBucketsAsync(cancellationToken);
            return HealthCheckResult.Healthy("LocalStack is ready");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "LocalStack not ready yet");
            return HealthCheckResult.Unhealthy("LocalStack is not ready", ex);
        }
    }
}
