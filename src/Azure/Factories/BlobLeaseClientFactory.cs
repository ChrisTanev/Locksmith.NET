// Copyright ChrisTanev. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Azure.Storage.Blobs.Specialized;

namespace Locksmith.NET.Azure.Factories;

public class BlobLeaseClientFactory(IBlobClientFactory blobClientFactory) : IBlobLeaseClientFactory
{
    public BlobLeaseClient Get() => blobClientFactory.Get().GetBlobLeaseClient();
}
