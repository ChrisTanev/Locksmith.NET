// Copyright ChrisTanev. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using AutoFixture;
using AutoFixture.AutoMoq;
using Azure.Storage.Blobs;
using Moq;

namespace Locksmith.NET.UnitTests.Utilities;

public abstract class UnitTestBase<TClass>
    where TClass : class
{
    protected UnitTestBase()
    {
        Fixture = new Fixture().Customize(new AutoMoqCustomization());
        Mock<BlobClient> blobClientMock = new();
        Fixture.Register(() => blobClientMock.Object);
        LazySut = new Lazy<TClass>(() => Fixture.Create<TClass>());
    }

    protected IFixture Fixture { get; }

    protected TClass Sut => LazySut.Value;

    private Lazy<TClass> LazySut { get; }
}
