namespace Locksmith.NET.Services;

public interface IDistributedLockService
{
    public Task<bool> AcquireLockAsync(TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);

    public Task<bool> ReleaseLockAsync(CancellationToken cancellationToken = default);
}