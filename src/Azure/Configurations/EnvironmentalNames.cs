// Copyright ChrisTanev. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Locksmith.NET.Azure.Configurations;

public static class EnvironmentalNames
{
    public static string BlobStorageAccountName { get; set; } = nameof(BlobStorageAccountName);

    public static string BlobStorageConnectionString { get; set; } = nameof(BlobStorageConnectionString);

    public static string BlobStorageContainerName { get; set; } = nameof(BlobStorageContainerName);

    public static string BlobStorageAcquireDuration { get; set; } = nameof(BlobStorageAcquireDuration);
}
