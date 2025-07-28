// Copyright ChrisTanev. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text;
using AutoFixture;
using Azure;
using Locksmith.NET.Azure.Configurations;
using Locksmith.NET.Example.AzureFunction;
using Locksmith.NET.Services;
using Locksmith.NET.UnitTests.Utilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace Locksmith.NET.UnitTests.Examples.Azure;

public class LockManuallyHttpTriggerTests : UnitTestBase<LockManuallyHttpTrigger>
{
    private readonly Mock<ILockService> _lockServiceMock;
    private readonly Mock<IEnvironmentalSettingsProvider> _environmentalProviderMock;
    private readonly Mock<ILogger<LockManuallyHttpTrigger>> _loggerMock;

    public LockManuallyHttpTriggerTests()
    {
        _lockServiceMock = Fixture.Freeze<Mock<ILockService>>();
        _environmentalProviderMock = Fixture.Freeze<Mock<IEnvironmentalSettingsProvider>>();
        _loggerMock = Fixture.Freeze<Mock<ILogger<LockManuallyHttpTrigger>>>();
    }

    [Fact]
    public async Task WhenRequestIsValid_Executes_As_Expected()
    {
        // Arrange
        var functionContextMock = new Mock<FunctionContext>();
        var invocationId = Fixture.Create<string>();
        functionContextMock.Setup(r => r.InvocationId).Returns(invocationId);

        var httpRequestDataMock = new Mock<HttpRequestData>(functionContextMock.Object);
        Mock<HttpResponseData> httpResponseDataMock = GetHttpResponseDataMock(functionContextMock, httpRequestDataMock);
        string blobName = Fixture.Create<string>();

        httpRequestDataMock.Setup(x => x.CreateResponse()).Returns(httpResponseDataMock.Object);

        _lockServiceMock
            .Setup(x => x.AcquireLockAsync(blobName, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _environmentalProviderMock.Setup(x => x.GetEnvironmentalSetting(EnvironmentalNames.BlobAcquireDuration))
            .Returns(TimeSpan.FromSeconds(15).ToString());

        // Act
        await Sut.RunManuallyHttpTrigger(httpRequestDataMock.Object,  blobName, functionContextMock.Object);

        // Assert
        _environmentalProviderMock.Verify(x => x.GetEnvironmentalSetting(EnvironmentalNames.BlobAcquireDuration), Times.Once);
        _lockServiceMock.Verify(x => x.AcquireLockAsync(blobName, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
        _lockServiceMock.Verify(x => x.ReleaseLockAsync(It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!
                    .Contains($"RunManuallyHttpTrigger function with Invocation Id= {invocationId} is locked= True")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!
                    .Contains($"RunManuallyHttpTrigger function with Invocation Id= {invocationId} is unlocked= False")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [Fact]
    public async Task WhenRequestIsValid_But_AlreadyLocked_Throws_RequestFailedException()
    {
        // Arrange
        var functionContextMock = new Mock<FunctionContext>();
        var invocationId = Fixture.Create<string>();
        functionContextMock.Setup(r => r.InvocationId).Returns(invocationId);

        var httpRequestDataMock = new Mock<HttpRequestData>(functionContextMock.Object);
        Mock<HttpResponseData> httpResponseDataMock = GetHttpResponseDataMock(functionContextMock, httpRequestDataMock);
        string blobName = Fixture.Create<string>();

        httpRequestDataMock.Setup(x => x.CreateResponse()).Returns(httpResponseDataMock.Object);

        _lockServiceMock
        .Setup(x => x.AcquireLockAsync(blobName, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
        .Returns(Task.FromException<bool>(new RequestFailedException("Simulated failure")));
        _environmentalProviderMock.Setup(x => x.GetEnvironmentalSetting(EnvironmentalNames.BlobAcquireDuration))
            .Returns(TimeSpan.FromSeconds(15).ToString());

        // Act
        await Sut.RunManuallyHttpTrigger(httpRequestDataMock.Object,  blobName, functionContextMock.Object);

        // Assert
        _environmentalProviderMock.Verify(x => x.GetEnvironmentalSetting(EnvironmentalNames.BlobAcquireDuration), Times.Once);
        _lockServiceMock.Verify(x => x.AcquireLockAsync(blobName, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
        _lockServiceMock.Verify(x => x.ReleaseLockAsync(It.IsAny<CancellationToken>()), Times.Never);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!
                    .Contains($"RunManuallyHttpTrigger function with Invocation Id= {invocationId} is locked= True")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Never);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!
                    .Contains($"RunManuallyHttpTrigger function with Invocation Id= {invocationId} is unlocked= False")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Never);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!
                    .Contains($"Invocation with id {invocationId} failed to acquire the lock.")),
                It.IsAny<RequestFailedException>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    private static Mock<HttpResponseData> GetHttpResponseDataMock(Mock<FunctionContext> functionContextMock, Mock<HttpRequestData> httpRequestDataMock)
    {
        var httpResponseDataMock = new Mock<HttpResponseData>(functionContextMock.Object);
        httpRequestDataMock.Setup(r => r.Method).Returns("GET");
        httpRequestDataMock.Setup(r => r.Url).Returns(new Uri("https://localhost/test"));
        httpRequestDataMock.Setup(r => r.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes("test body")));
        return httpResponseDataMock;
    }
}
