// Copyright ChrisTanev. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Locksmith.NET.Azure.Configurations;

namespace Locksmith.NET.Azure.Factories;

public class BlobClientFactory(IEnvironmentalSettingsProvider environmentalSettingsProvider) : IBlobClientFactory
{
    private readonly ConcurrentDictionary<string, Lazy<Task<BlobClient>>> _clients = new();

    public async Task<BlobClient> GetBlobClientAsync(string blobName)
    {
        Lazy<Task<BlobClient>> lazy = _clients.GetOrAdd(blobName, name =>
            new Lazy<Task<BlobClient>>(() => CreateBlobClientAsync(name)));

        return await lazy.Value;
    }

    public async Task<BlobLeaseClient> GetBlobLeaseClientAsync(string blobName)
    {
        BlobClient blobClient = await GetBlobClientAsync(blobName);
        return blobClient.GetBlobLeaseClient();
    }

    private async Task<BlobClient> CreateBlobClientAsync(string blobName)
    {
        string connectionString = environmentalSettingsProvider.GetEnvironmentalSetting(EnvironmentalNames.BlobConnectionString);
        string containerName = environmentalSettingsProvider.GetEnvironmentalSetting(EnvironmentalNames.BlobContainerName);

        BlobServiceClient serviceClient = new(connectionString);
        BlobContainerClient? containerClient = serviceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();

        BlobClient blobClient = containerClient.GetBlobClient(blobName);

        if (await blobClient.ExistsAsync())
        {
            return await Task.FromResult(blobClient);
        }

        using MemoryStream emptyStream = new([]);
        await blobClient.UploadAsync(emptyStream);

        return blobClient;
    }
}
