using AutoFixture;
using AutoFixture.AutoMoq;
using Azure.Storage.Blobs;
using Moq;

namespace Locksmith.NET.UnitTests.Utilities;

public abstract class UnitTestBase<TClass> where TClass : class
{
    protected UnitTestBase()
    {
        Fixture = new Fixture().Customize(new AutoMoqCustomization());
        // Register a mock BlobClient
        var blobClientMock = new Mock<BlobClient>();
        Fixture.Register(() => blobClientMock.Object);
        LazySut = new(() => Fixture.Create<TClass>());
    }

    protected IFixture Fixture { get; }

    protected TClass Sut => LazySut.Value;

    private Lazy<TClass> LazySut { get; }
}