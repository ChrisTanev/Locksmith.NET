// Copyright ChrisTanev. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Locksmith.NET.Azure.Configurations;

public static class EnvironmentalNames
{
    public static string BlobAccountName { get; set; } = nameof(BlobAccountName);

    public static string BlobConnectionString { get; set; } = nameof(BlobConnectionString);

    public static string BlobContainerName { get; set; } = nameof(BlobContainerName);

    public static string BlobAcquireDuration { get; set; } = nameof(BlobAcquireDuration);
}
