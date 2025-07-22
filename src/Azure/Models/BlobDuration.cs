// Copyright ChrisTanev. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Locksmith.NET.Azure.Models;

public record BlobDuration(TimeSpan? Duration, bool Infinite = false);
