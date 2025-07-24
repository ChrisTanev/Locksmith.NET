// Copyright ChrisTanev. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Azure.Storage.Blobs;

namespace Locksmith.NET.Azure.Factories;

public interface IBlobClientFactory
{
    BlobClient Get();
}
