using Azure.Core;
using Azure.Storage.Blobs;
using Locksmith.NET.Azure.Configurations;
using Locksmith.NET.Azure.Models;
using Locksmith.NET.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Locksmith.NET.Azure.Extensions;

public static class HostApplicationBuilderExtensions
{
    public static void RegisterBlobStorageLockService(
        this HostApplicationBuilder builder,
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
            throw new ArgumentException($"{nameof(BlobDuration.Duration)} must be between 15 seconds and 60 seconds.");

        builder.Services.AddTransient<IEnvironmentalSettingsProvider, EnvironmentalSettingsProvider>(provider =>
        {
            var environmentalSettingsProvider = new EnvironmentalSettingsProvider();
            environmentalSettingsProvider.SetEnvoronmentalSetting(EnvironmentalNames.BlobStorageConnectionString,
                connectionString);
            environmentalSettingsProvider.SetEnvoronmentalSetting(EnvironmentalNames.BlobStorageAcoountName,
                blobStorageAccountName);
            environmentalSettingsProvider.SetEnvoronmentalSetting(EnvironmentalNames.BlobStorageContainerName,
                containerName);
            environmentalSettingsProvider.SetEnvoronmentalSetting(EnvironmentalNames.BlobStorageAcquireDuration,
                containerName);

            if (blobDuration is { Infinite: true, Duration: null })
                environmentalSettingsProvider.SetEnvoronmentalSetting(EnvironmentalNames.BlobStorageAcquireDuration,
                    TimeSpan.MinValue.ToString());
            else
                environmentalSettingsProvider.SetEnvoronmentalSetting(EnvironmentalNames.BlobStorageAcquireDuration,
                    blobDuration.Duration.ToString());

            // Register BlobClient
            BlobServiceClient client = new(
                new Uri(
                    $"https://{environmentalSettingsProvider.GetEnvironmentalSetting(EnvironmentalNames.BlobStorageAcoountName)}.blob.core.windows.net"),
                tokenCredential);

            var containerClient =
                client.GetBlobContainerClient(
                    environmentalSettingsProvider.GetEnvironmentalSetting(EnvironmentalNames.BlobStorageContainerName));
            var blobClient = containerClient.GetBlobClient(blobName);
            builder.Services.AddSingleton(blobClient);

            return environmentalSettingsProvider;
        });

        builder.Services.AddSingleton<IConcreteLockService, BlobStorageLockService>();
        builder.Services.AddSingleton(tokenCredential);
    }
}