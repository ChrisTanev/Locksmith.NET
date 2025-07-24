// Copyright ChrisTanev. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Azure;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Locksmith.NET.Azure.Configurations;
using Locksmith.NET.Azure.Factories;
using Locksmith.NET.Services;
using Microsoft.Extensions.Logging;

namespace Locksmith.NET.Azure;

// TODO add global usings
// TODO add retry logic with Polly
// TODO add directory props
public class BlobStorageLockService(
    IEnvironmentalSettingsProvider environmentalSettingsProvider,
    IBlobLeaseClientFactory blobLeaseClientFactory,
    ILogger<BlobStorageLockService> logger)
    : IConcreteLockService
{
    private BlobLeaseClient? LeaseClient { get; set; }

    public async Task<bool> AcquireLockAsync(
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            LeaseClient = blobLeaseClientFactory.Get();

            TimeSpan duration = TimeSpan.Parse(environmentalSettingsProvider.GetEnvironmentalSetting(EnvironmentalNames.BlobAcquireDuration));

            // TODO  add poly retry logic
            Response<BlobLease>? blobLease = await LeaseClient.AcquireAsync(duration, cancellationToken: cancellationToken);

            return blobLease.HasValue;
        }
        catch (RequestFailedException ex)
        {
            logger.LogError(ex, "Failed to acquire the lock.");
            if (LeaseClient != null)
            {
                await LeaseClient.ReleaseAsync(cancellationToken: cancellationToken);
            }
        }

        return false;
    }

    public async Task<bool> ReleaseLockAsync(CancellationToken cancellationToken = default)
    {
        if (LeaseClient is null)
        {
            throw new InvalidOperationException("LeaseClient is not initialized. Call AcquireLockAsync first.");
        }

        Response<ReleasedObjectInfo>? response = await LeaseClient.ReleaseAsync(cancellationToken: cancellationToken);
        if (response.HasValue)
        {
            return true;
        }

        throw new InvalidOperationException("Failed to release the lock.");
    }
}
