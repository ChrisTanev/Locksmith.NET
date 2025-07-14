using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Locksmith.NET.Azure.Configurations;
using Locksmith.NET.Services;
using Microsoft.Extensions.Logging;

namespace Locksmith.NET.Azure;

public class BlobStorageLockService(
    IEnvironmentalSettingsProvider environmentalSettingsProvider,
    BlobClient blobClient,
    ILogger<BlobStorageLockService> logger)
    : IConcreteLockService
{
    private BlobLeaseClient? LeaseClient { get; set; }

    public async Task<bool> AcquireLockAsync(
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        LeaseClient = blobClient.GetBlobLeaseClient();

        var duration =
            TimeSpan.Parse(
                environmentalSettingsProvider.GetEnvironmentalSetting(EnvironmentalNames.BlobStorageAcquireDuration));
        try
        {
            var blobLease =
                await LeaseClient.AcquireAsync(duration, cancellationToken: cancellationToken);

            return blobLease.HasValue;
        }
        catch (RequestFailedException ex)
        {
            logger.LogError(ex, "Failed to acquire the lock.");
            await LeaseClient.ReleaseAsync(cancellationToken: cancellationToken);
        }

        return false;
    }

    public async Task<bool> ReleaseLockAsync(CancellationToken cancellationToken = default)
    {
        if (LeaseClient is null)
            throw new InvalidOperationException("LeaseClient is not initialized. Call AcquireLockAsync first.");

        var response = await LeaseClient.ReleaseAsync(cancellationToken: cancellationToken);
        if (response.HasValue) return true;

        throw new InvalidOperationException("Failed to release the lock.");
    }
}