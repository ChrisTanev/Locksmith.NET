// Copyright ChrisTanev. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;

namespace Locksmith.NET.Azure.Factories;

public class BlobLeaseClientFactory(BlobClient blobClient) : IBlobLeaseClientFactory
{
    public BlobLeaseClient Get() => blobClient.GetBlobLeaseClient();
}
