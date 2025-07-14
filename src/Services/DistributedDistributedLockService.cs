namespace Locksmith.NET.Services;

public class DistributedDistributedLockService(IConcreteLockService concreteLockService) : IDistributedLockService
{
    public async Task<bool> AcquireLockAsync(string key, TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        return await concreteLockService.AcquireLockAsync(key, expiration, cancellationToken);
    }

    public async Task<bool> ReleaseLockAsync(string key, CancellationToken cancellationToken = default)
    {
        return await concreteLockService.ReleaseLockAsync(key, cancellationToken);
    }
}