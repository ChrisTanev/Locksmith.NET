// Copyright ChrisTanev. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net;
using Locksmith.NET.Azure.Configurations;
using Locksmith.NET.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Locksmith.NET.Example.AzureFunction;

public class LockManuallyHttpTrigger(ILogger<LockManuallyHttpTrigger> logger, IConcreteLockService lockService, IEnvironmentalSettingsProvider environmentalSettingsProvider)
{
    [Function(nameof(RunManuallyHttpTrigger))]
    public async Task<HttpResponseData> RunManuallyHttpTrigger(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")]
        HttpRequestData req,
        FunctionContext executionContext)
    {
        logger.LogInformation("C# HTTP trigger function processed");

        string environmentalSettings = environmentalSettingsProvider.GetEnvironmentalSetting(EnvironmentalNames.BlobStorageAcquireDuration);
        bool isLocked = await lockService.AcquireLockAsync(TimeSpan.Parse(environmentalSettings), executionContext.CancellationToken);
        logger.LogInformation("HTTP trigger function is locked: {IsLocked}", isLocked);

        HttpResponseData httpResponseData = await Task.FromResult(req.CreateResponse(HttpStatusCode.OK));

        bool isUnlocked = await lockService.ReleaseLockAsync(executionContext.CancellationToken);
        logger.LogInformation("HTTP trigger function is unlocked: {IsUnlocked}", isUnlocked);

        return httpResponseData;
    }
}
