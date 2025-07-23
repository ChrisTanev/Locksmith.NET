// Copyright ChrisTanev. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Azure.Identity;
using Locksmith.NET.Azure.Extensions;
using Locksmith.NET.Azure.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
       .AddApplicationInsightsTelemetryWorkerService()
       .ConfigureFunctionsApplicationInsights();

builder.Services.AddLogging();

builder.Services.RegisterBlobStorageLockService(
    tokenCredential: new DefaultAzureCredential(),
    connectionString: "UseDevelopmentStorage=true",
    blobDuration: new BlobDuration(TimeSpan.FromSeconds(30)),
    blobStorageAccountName: "myaccount",
    blobName: "lock-blob");

builder.Build().Run();
