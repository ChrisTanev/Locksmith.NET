// Copyright ChrisTanev. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Locksmith.NET.Services;

public class DistributedLockService(ILockService lockService) : IDistributedLockService
{
    public async Task<bool> AcquireLockAsync(string fileName, TimeSpan? expiration = null, CancellationToken cancellationToken = default) => await lockService.AcquireLockAsync(fileName, expiration, cancellationToken);

    public async Task<bool> ReleaseLockAsync(CancellationToken cancellationToken = default) => await lockService.ReleaseLockAsync(cancellationToken);
}
