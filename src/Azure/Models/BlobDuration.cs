namespace Locksmith.NET.Azure.Models;

public record BlobDuration(TimeSpan? Duration, bool infinite = false);