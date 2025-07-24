// Copyright ChrisTanev. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using AutoFixture;
using Azure;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using FluentAssertions;
using Locksmith.NET.Azure;
using Locksmith.NET.Azure.Configurations;
using Locksmith.NET.Azure.Factories;
using Locksmith.NET.UnitTests.Utilities;
using Microsoft.Extensions.Logging;
using Moq;

namespace Locksmith.NET.UnitTests;

public class BlobStorageLockServiceTests : UnitTestBase<BlobStorageLockService>
{
    private readonly Mock<IBlobClientFactory> _blobClientFactoryMock;
    private readonly Mock<IEnvironmentalSettingsProvider> _environmentalProviderMock;
    private readonly Mock<ILogger<BlobStorageLockService>> _loggerMock;

    public BlobStorageLockServiceTests()
    {
        _environmentalProviderMock = Fixture.Freeze<Mock<IEnvironmentalSettingsProvider>>();
        _blobClientFactoryMock = Fixture.Freeze<Mock<IBlobClientFactory>>();
        _loggerMock = Fixture.Freeze<Mock<ILogger<BlobStorageLockService>>>();
    }

    [Fact]
    public async Task When_Calling_AcquireLockAsync_And_LeaseIsAcquired_Returns_True()
    {
        // Arrange
        string blobName = Fixture.Create<string>();

        _environmentalProviderMock.Setup(x => x.GetEnvironmentalSetting(EnvironmentalNames.BlobAcquireDuration))
                                  .Returns(TimeSpan.FromSeconds(15).ToString());

        Mock<BlobLeaseClient> blobLeaseClient = new();

        _blobClientFactoryMock.Setup(x => x.GetBlobLeaseClientAsync(blobName))
                              .ReturnsAsync(blobLeaseClient.Object);

        Mock<BlobLease>? blobLease = Fixture.Freeze<Mock<BlobLease>>();
        blobLeaseClient.Setup(x => x.AcquireAsync(It.IsAny<TimeSpan>(), null, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(Response.FromValue(blobLease.Object, null!));

        // Act
        bool result = await Sut.AcquireLockAsync(blobName);

        // Assert
        result.Should().BeTrue();

        _blobClientFactoryMock.Verify(x => x.GetBlobLeaseClientAsync(blobName), Times.Once);
        blobLeaseClient.Verify(
                              x => x.AcquireAsync(
                                                  It.IsAny<TimeSpan>(),
                                                  null,
                                                  It.IsAny<CancellationToken>()),
                              Times.Once);
    }

    [Fact]
    public async Task When_Calling_AcquireLockAsync_And_RequestFailedExceptionIsTrown_Its_Caught_And_Logged_And_Lease_Is_Released()
    {
        // Arrange
        string blobName = Fixture.Create<string>();

        _environmentalProviderMock.Setup(x => x.GetEnvironmentalSetting(EnvironmentalNames.BlobAcquireDuration))
                                  .Returns(TimeSpan.FromSeconds(15).ToString());

        Mock<BlobLeaseClient> blobLeaseClientMock = new();

        _blobClientFactoryMock.Setup(x => x.GetBlobLeaseClientAsync(blobName))
                              .ReturnsAsync(blobLeaseClientMock.Object);

        blobLeaseClientMock.Setup(x => x.AcquireAsync(It.IsAny<TimeSpan>(), null, It.IsAny<CancellationToken>()))
                           .Throws(new RequestFailedException(500, "Internal Server Error"));

        // Act
        bool result = await Sut.AcquireLockAsync(blobName);

        // Assert
        result.Should().BeFalse();

        _blobClientFactoryMock.Verify(x => x.GetBlobLeaseClientAsync(blobName), Times.Once);

        _loggerMock.Verify(
                           x => x.Log(
                                      LogLevel.Error,
                                      It.IsAny<EventId>(),
                                      It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to acquire the lock.")),
                                      It.IsAny<RequestFailedException>(),
                                      It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                           Times.Once);

        blobLeaseClientMock.Verify(
                                   x => x.ReleaseAsync(It.IsAny<RequestConditions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task When_Calling_ReleaseLockAsync_And_LeaseClientIsNull_Throws_InvalidOperationException()
    {
        // Act
        Func<Task> act = async () => await Sut.ReleaseLockAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("LeaseClient is not initialized. Call AcquireLockAsync first.");
    }

    [Fact]
    public async Task When_Calling_ReleaseLockAsync_And_ReleaseAsyncValue_Is_False_Throws_InvalidOperationException()
    {
        // Arrange
        Mock<BlobLeaseClient> blobLeaseClientMock = new();
        Mock<Response<ReleasedObjectInfo>> releasedObjectInfoX = new();
        blobLeaseClientMock.Setup(x => x.ReleaseAsync(It.IsAny<RequestConditions>(), It.IsAny<CancellationToken>()))
                           .ReturnsAsync(releasedObjectInfoX.Object);

        PropertyInfo? prop = typeof(BlobStorageLockService).GetProperty("LeaseClient", BindingFlags.NonPublic | BindingFlags.Instance);
        prop!.SetValue(Sut, blobLeaseClientMock.Object);
        releasedObjectInfoX.Setup(x => x.HasValue).Returns(false);

        // Act
        Func<Task> act = async () => await Sut.ReleaseLockAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("Failed to release the lock.");
    }
}
