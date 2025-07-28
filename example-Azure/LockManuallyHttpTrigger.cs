// Copyright ChrisTanev. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net;
using Azure;
using Locksmith.NET.Azure.Configurations;
using Locksmith.NET.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Locksmith.NET.Example.AzureFunction;

public class LockManuallyHttpTrigger(
    ILogger<LockManuallyHttpTrigger> logger,
    ILockService lockService,
    IEnvironmentalSettingsProvider environmentalSettingsProvider)
{
    [Function(nameof(RunManuallyHttpTrigger))]
    public async Task<HttpResponseData> RunManuallyHttpTrigger(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "RunManuallyHttpTrigger/{blobName}")] HttpRequestData req,
        string blobName,
        FunctionContext executionContext)
    {
        try
        {
            string timespanDuration = environmentalSettingsProvider.GetEnvironmentalSetting(EnvironmentalNames.BlobAcquireDuration);

            bool isLocked = await lockService.AcquireLockAsync(blobName, TimeSpan.Parse(timespanDuration), executionContext.CancellationToken);

            logger.LogInformation(
                "{RunManuallyHttpTrigger} function with Invocation Id= {Id} is locked= {IsLocked}",
                nameof(RunManuallyHttpTrigger),
                executionContext.InvocationId,
                isLocked);

            // Simulating some work
            await Task.Delay(TimeSpan.FromSeconds(1), executionContext.CancellationToken);

            HttpResponseData httpResponseData = await Task.FromResult(req.CreateResponse(HttpStatusCode.OK));

            bool isUnlocked = await lockService.ReleaseLockAsync(executionContext.CancellationToken);

            logger.LogInformation(
                "{RunManuallyHttpTrigger} function with Invocation Id= {Id} is unlocked= {isUnlocked}",
                nameof(RunManuallyHttpTrigger),
                executionContext.InvocationId,
                isUnlocked);

            return httpResponseData;
        }
        catch (RequestFailedException e)
        {
            logger.LogError(e, "Invocation with id {Id} failed to acquire the lock.", executionContext.InvocationId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "{Func} threw exception", nameof(RunManuallyHttpTrigger));
        }

        return req.CreateResponse(HttpStatusCode.InternalServerError);
    }
}
