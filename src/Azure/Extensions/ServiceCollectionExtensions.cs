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

        var environmentalSettingsProvider = new EnvironmentalSettingsProvider();
        environmentalSettingsProvider.SetEnvironmentalSetting(EnvironmentalNames.BlobStorageConnectionString, connectionString);
        environmentalSettingsProvider.SetEnvironmentalSetting(EnvironmentalNames.BlobStorageAccountName, blobStorageAccountName);
        environmentalSettingsProvider.SetEnvironmentalSetting(EnvironmentalNames.BlobStorageContainerName, containerName);

        if (blobDuration is { Infinite: true, Duration: null, })
        {
            environmentalSettingsProvider.SetEnvironmentalSetting(EnvironmentalNames.BlobStorageAcquireDuration, TimeSpan.MinValue.ToString());
        }
        else
        {
            environmentalSettingsProvider.SetEnvironmentalSetting(EnvironmentalNames.BlobStorageAcquireDuration, blobDuration.Duration.ToString());
        }

        // Register EnvironmentalSettingsProvider as singleton
        serviceCollection.AddSingleton<IEnvironmentalSettingsProvider>(environmentalSettingsProvider);

        // Create BlobServiceClient, container client and BlobClient outside of factory
        // BlobServiceClient client = new(new($"https://{environmentalSettingsProvider.GetEnvironmentalSetting(EnvironmentalNames.AzureWebJobsStorage)}.blob.core.windows.net"), tokenCredential);
        BlobServiceClient client = new("UseDevelopmentStorage=true");
        BlobContainerClient containerClient = client.GetBlobContainerClient(containerName);
        BlobClient blobClient = containerClient.GetBlobClient(blobName);

        serviceCollection.AddSingleton(blobClient);

        // Register your factory here
        serviceCollection.AddSingleton<IBlobLeaseClientFactory, BlobLeaseClientFactory>();

        // Register your lock service
        serviceCollection.AddSingleton<IConcreteLockService, BlobStorageLockService>();

        // Register token credential
        serviceCollection.AddSingleton(tokenCredential);
    }
}
