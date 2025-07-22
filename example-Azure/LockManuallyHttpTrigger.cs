// Copyright ChrisTanev. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Locksmith.NET.Example.AzureFunction;

public class LockManuallyHttpTrigger(ILogger<LockManuallyHttpTrigger> logger)
{
    [Function(nameof(RunManuallyHttpTrigger))]
    public async Task<HttpResponseData> RunManuallyHttpTrigger(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")]
        HttpRequestData req,
        FunctionContext executionContext)
    {
        logger.LogInformation("C# HTTP trigger function processed");

        return await Task.FromResult(req.CreateResponse(HttpStatusCode.OK));
    }
}
