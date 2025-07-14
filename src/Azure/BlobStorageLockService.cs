using Azure.Core;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Locksmith.NET.Azure.Configurations;
using Locksmith.NET.Services;

namespace Locksmith.NET.Azure;

public class BlobStorageLockService(
    TokenCredential tokenCredential,
    BlobServiceClient blobStorage,
    IEnvironmentalSettingsProvider environmentalSettingsProvider)
    : IConcreteLockService
{
    public async Task<bool> AcquireLockAsync(
        string key,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        BlobServiceClient client = new(
            new Uri(
                $"https://{Environment.GetEnvironmentVariable(EnvironmentalNames.BlobStorageAcoountName)}.blob.core.windows.net"),
            tokenCredential);

        var containerClient =
            client.GetBlobContainerClient(
                Environment.GetEnvironmentVariable(EnvironmentalNames.BlobStorageContainerName));
        var blobClient = containerClient.GetBlobClient(key);
        var blobLeaseClient = blobClient.GetBlobLeaseClient();

        var duration =
            TimeSpan.Parse(
                environmentalSettingsProvider.GetEnvironmentalSetting(EnvironmentalNames.BlobStorageAcquireDuration));
        var blobLease =
            await blobLeaseClient.AcquireAsync(duration, cancellationToken: cancellationToken);

        return blobLease.HasValue;
    }

    public Task<bool> ReleaseLockAsync(string key, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}