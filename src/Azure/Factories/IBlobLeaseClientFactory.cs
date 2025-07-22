// Copyright ChrisTanev. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Azure.Storage.Blobs.Specialized;

namespace Locksmith.NET.Azure.Factories;

/// <summary>
///     Interface for creating instances of <see cref="BlobLeaseClient" />.
/// </summary>
public interface IBlobLeaseClientFactory
{
    BlobLeaseClient Get();
}
