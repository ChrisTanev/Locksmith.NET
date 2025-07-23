// Copyright ChrisTanev. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Azure.Core;
using Azure.Storage.Blobs;
using Locksmith.NET.Azure.Configurations;
using Locksmith.NET.Azure.Factories;
using Locksmith.NET.Azure.Models;
using Locksmith.NET.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Locksmith.NET.Azure.Extensions;

public static class ServiceCollectionExtensions
{
    public static void RegisterBlobStorageLockService(
        this IServiceCollection serviceCollection,
        TokenCredential tokenCredential,
        string connectionString,
        BlobDuration blobDuration,
        string blobStorageAccountName,
        string blobName,
        string containerName = "locks",
        string blobPrefix = "lock-")
    {
        if ((!blobDuration.Infinite && blobDuration.Duration > TimeSpan.FromSeconds(60)) ||
            blobDuration.Duration < TimeSpan.FromSeconds(15))
        {
            throw new ArgumentException($"{nameof(BlobDuration.Duration)} must be between 15 seconds and 60 seconds.");
        }

        serviceCollection.AddSingleton<IEnvironmentalSettingsProvider, EnvironmentalSettingsProvider>(_ =>
        {
            EnvironmentalSettingsProvider environmentalSettingsProvider = new();
            environmentalSettingsProvider.SetEnvironmentalSetting(EnvironmentalNames.BlobStorageConnectionString, connectionString);
            environmentalSettingsProvider.SetEnvironmentalSetting(EnvironmentalNames.BlobStorageAcoountName, blobStorageAccountName);
            environmentalSettingsProvider.SetEnvironmentalSetting(EnvironmentalNames.BlobStorageContainerName, containerName);
            environmentalSettingsProvider.SetEnvironmentalSetting(EnvironmentalNames.BlobStorageAcquireDuration, containerName);

            if (blobDuration is { Infinite: true, Duration: null, })
            {
                environmentalSettingsProvider.SetEnvironmentalSetting(EnvironmentalNames.BlobStorageAcquireDuration, TimeSpan.MinValue.ToString());
            }
            else
            {
                environmentalSettingsProvider.SetEnvironmentalSetting(EnvironmentalNames.BlobStorageAcquireDuration, blobDuration.Duration.ToString());
            }

            // Register BlobClient
            BlobServiceClient client = new(new($"https://{environmentalSettingsProvider.GetEnvironmentalSetting(EnvironmentalNames.BlobStorageAcoountName)}.blob.core.windows.net"), tokenCredential);

            BlobContainerClient? containerClient = client.GetBlobContainerClient(environmentalSettingsProvider.GetEnvironmentalSetting(EnvironmentalNames.BlobStorageContainerName));

            BlobClient? blobClient = containerClient.GetBlobClient(blobName);
            serviceCollection.AddSingleton(blobClient);

            serviceCollection.AddSingleton<IBlobLeaseClientFactory, BlobLeaseClientFactory>();
            return environmentalSettingsProvider;
        });

        serviceCollection.AddSingleton<IConcreteLockService, BlobStorageLockService>();
        serviceCollection.AddSingleton(tokenCredential);
    }
}
