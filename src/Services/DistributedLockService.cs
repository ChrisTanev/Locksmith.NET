// Copyright ChrisTanev. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Locksmith.NET.Services;

public class DistributedLockService(IConcreteLockService concreteLockService) : IDistributedLockService
{
    public async Task<bool> AcquireLockAsync(TimeSpan? expiration = null, CancellationToken cancellationToken = default) => await concreteLockService.AcquireLockAsync(expiration, cancellationToken);

    public async Task<bool> ReleaseLockAsync(CancellationToken cancellationToken = default) => await concreteLockService.ReleaseLockAsync(cancellationToken);
}
