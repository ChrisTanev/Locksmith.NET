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
        string connectionString,
        BlobDuration blobDuration,
        string blobStorageAccountName,
        string containerName = "locks",
        string blobPrefix = "lock-")
    {
        if ((!blobDuration.infinite && blobDuration.Duration > TimeSpan.FromSeconds(60)) ||
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

            if (blobDuration is { infinite: true, Duration: null })
                Environment.SetEnvironmentVariable(EnvironmentalNames.BlobStorageAcquireDuration,
                    TimeSpan.MinValue.ToString());
            else
                Environment.SetEnvironmentVariable(EnvironmentalNames.BlobStorageAcquireDuration,
                    blobDuration.Duration.ToString());

            return environmentalSettingsProvider;
        });

        builder.Services.AddSingleton<IConcreteLockService, BlobStorageLockService>();
    }
}