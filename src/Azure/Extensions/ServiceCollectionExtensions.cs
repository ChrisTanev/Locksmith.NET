// Copyright ChrisTanev. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Azure.Core;
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
        string blobContainerName = "locks")
    {
        if ((!blobDuration.Infinite && blobDuration.Duration > TimeSpan.FromSeconds(60)) ||
            blobDuration.Duration < TimeSpan.FromSeconds(15))
        {
            throw new ArgumentException($"{nameof(BlobDuration.Duration)} must be between 15 seconds and 60 seconds.");
        }

        EnvironmentalSettingsProvider environmentalSettingsProvider = new();

        SetupEnvironmentalVariables(
            environmentalSettingsProvider,
            connectionString,
            blobStorageAccountName,
            blobContainerName,
            blobName,
            blobDuration);

        serviceCollection.AddSingleton<IEnvironmentalSettingsProvider>(environmentalSettingsProvider);

        serviceCollection.AddSingleton<IBlobLeaseClientFactory, BlobLeaseClientFactory>();
        serviceCollection.AddSingleton<IBlobClientFactory, BlobClientFactory>();

        serviceCollection.AddSingleton<IConcreteLockService, BlobStorageLockService>();
        serviceCollection.AddSingleton(tokenCredential);
    }

    private static void SetupEnvironmentalVariables(
        IEnvironmentalSettingsProvider environmentalSettingsProvider,
        string connectionString,
        string blobStorageAccountName,
        string blobContainerName,
        string blobName,
        BlobDuration blobDuration)
    {
        environmentalSettingsProvider.SetEnvironmentalSetting(
            EnvironmentalNames.BlobConnectionString,
            connectionString);

        environmentalSettingsProvider.SetEnvironmentalSetting(
            EnvironmentalNames.BlobAccountName,
            blobStorageAccountName);

        environmentalSettingsProvider.SetEnvironmentalSetting(EnvironmentalNames.BlobContainerName, blobContainerName);

        environmentalSettingsProvider.SetEnvironmentalSetting(EnvironmentalNames.BlobName, blobName);

        string? timespan = blobDuration is { Infinite: true, Duration: null }
            ? TimeSpan.MinValue.ToString()
            : blobDuration.Duration.ToString();
        environmentalSettingsProvider.SetEnvironmentalSetting(EnvironmentalNames.BlobAcquireDuration, timespan);
    }
}
