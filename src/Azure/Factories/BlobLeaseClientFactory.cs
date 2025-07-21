using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;

namespace Locksmith.NET.Azure.Factories;

public class BlobLeaseClientFactory(BlobClient blobClient) : IBlobLeaseClientFactory
{
    public BlobLeaseClient Get()
    {
        return blobClient.GetBlobLeaseClient();
    }
}