// Copyright ChrisTanev. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Locksmith.NET.Services;

public interface IDistributedLockService
{
    public Task<bool> AcquireLockAsync(string fileName, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    public Task<bool> ReleaseLockAsync(CancellationToken cancellationToken = default);
}
