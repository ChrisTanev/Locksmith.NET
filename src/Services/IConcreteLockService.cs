namespace Locksmith.NET.Services;

public interface IConcreteLockService
{
    public Task<bool> AcquireLockAsync(string key, TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);

    public Task<bool> ReleaseLockAsync(string key, CancellationToken cancellationToken = default);
}