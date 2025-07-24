// Copyright ChrisTanev. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Azure.Storage.Blobs;
using Locksmith.NET.Azure.Configurations;

namespace Locksmith.NET.Azure.Factories;

public class BlobClientFactory(IEnvironmentalSettingsProvider environmentalSettingsProvider) : IBlobClientFactory
{
    private readonly Lazy<BlobClient> _lazyBlobClient = new(() =>
    {
        string connectionString = environmentalSettingsProvider
            .GetEnvironmentalSetting(EnvironmentalNames.BlobConnectionString);

        string containerName = environmentalSettingsProvider
            .GetEnvironmentalSetting(EnvironmentalNames.BlobContainerName);

        string blobName = environmentalSettingsProvider
            .GetEnvironmentalSetting(EnvironmentalNames.BlobName);

        var serviceClient = new BlobServiceClient(connectionString);
        var containerClient = serviceClient.GetBlobContainerClient(containerName);
        containerClient.CreateIfNotExists(); // container

        var blobClient = containerClient.GetBlobClient(blobName);

        if (blobClient.Exists())
        {
            return blobClient;
        }

        using var emptyStream = new MemoryStream([]);
        blobClient.Upload(emptyStream);

        return blobClient;
    });

    public BlobClient Get() => _lazyBlobClient.Value;
}
