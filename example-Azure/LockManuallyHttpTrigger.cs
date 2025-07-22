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

        // Here you would implement the logic to lock manually using the provided key.
        // This is just a placeholder for demonstration purposes.

        return await Task.FromResult(req.CreateResponse(HttpStatusCode.OK));
    }
}