namespace Locksmith.NET.Services;

public class DistributedDistributedLockService(IConcreteLockService concreteLockService) : IDistributedLockService
{
    public async Task<bool> AcquireLockAsync(TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        return await concreteLockService.AcquireLockAsync(expiration, cancellationToken);
    }

    public async Task<bool> ReleaseLockAsync(CancellationToken cancellationToken = default)
    {
        return await concreteLockService.ReleaseLockAsync(cancellationToken);
    }
}