namespace Locksmith.NET.Azure.Models;

public record BlobDuration(TimeSpan? Duration, bool Infinite = false);