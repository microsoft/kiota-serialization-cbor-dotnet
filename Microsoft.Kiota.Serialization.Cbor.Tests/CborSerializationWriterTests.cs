using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Serialization.Cbor.Tests.Mocks;
using Xunit;

namespace Microsoft.Kiota.Serialization.Cbor.Tests;
public sealed class CborSerializationWriterTests : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    [Fact]
    public async Task WritesSampleObjectValue()
    {
        // Arrange
        var testEntity = new TestEntity()
        {
            Id = "48d31887-5fad-4d73-a9f5-3c356e68a038",
            WorkDuration = TimeSpan.FromHours(1),
            StartWorkTime = new Time(8, 0, 0),
            BirthDay = new Date(2017, 9, 4),
            AdditionalData = new Dictionary<string, object>
                {
                    {"mobilePhone",null}, // write null value
                    {"accountEnabled",false}, // write bool value
                    {"jobTitle","Author"}, // write string value
                    {"createdDateTime", DateTimeOffset.MinValue}, // write date value
                    {"businessPhones", new List<string>() {"+1 412 555 0109"}}, // write collection of primitives value
                    {"endDateTime", new DateTime(2023,03,14,0,0,0,DateTimeKind.Utc) }, // ensure the DateTime doesn't crash
                    {"manager", new TestEntity{Id = "48d31887-5fad-4d73-a9f5-3c356e68a038"}}, // write nested object value
                }
        };
        using var cborSerializerWriter = new CborSerializationWriter();
        // Act
        cborSerializerWriter.WriteObjectValue(string.Empty, testEntity);
        // Get the payload from the stream.
        using var serializedStream = cborSerializerWriter.GetSerializedContent();
        var serializedCborString = await TestDataHelper.GetHexRepresentationFromStream(serializedStream, _cancellationTokenSource.Token);

        // Assert
        var expectedHex = TestDataHelper.GetCborHex("TestUserWrite");
        Assert.Equal(expectedHex, serializedCborString, StringComparer.Ordinal);
    }

    [Fact]
    public async Task WritesSampleObjectValueWithCborElementAdditionalData()
    {
        var nullCborElement = new CborParseNode((object)null);
        var arrayCborElement = new CborParseNode(new List<object>() { "+1 412 555 0109" });
        var objectCborElement = new CborParseNode(new Dictionary<string, object> {
            { "id", "48d31887-5fad-4d73-a9f5-3c356e68a038"}
        });

        // Arrange
        var testEntity = new TestEntity()
        {
            Id = "48d31887-5fad-4d73-a9f5-3c356e68a038",
            WorkDuration = TimeSpan.FromHours(1),
            StartWorkTime = new Time(8, 0, 0),
            BirthDay = new Date(2017, 9, 4),
            AdditionalData = new Dictionary<string, object>
                {
                    {"mobilePhone", nullCborElement}, // write null value
                    {"accountEnabled",false}, // write bool value
                    {"jobTitle","Author"}, // write string value
                    {"createdDateTime", DateTimeOffset.MinValue}, // write date value
                    {"businessPhones", arrayCborElement }, // write collection of primitives value
                    {"manager", objectCborElement }, // write nested object value
                }
        };
        using var cborSerializerWriter = new CborSerializationWriter();
        // Act
        cborSerializerWriter.WriteObjectValue(string.Empty, testEntity);
        // Get the payload from the stream.
        using var serializedStream = cborSerializerWriter.GetSerializedContent();
        var serializedCborString = await TestDataHelper.GetHexRepresentationFromStream(serializedStream, _cancellationTokenSource.Token);

        // Assert
        var expectedHex = TestDataHelper.GetCborHex("TestAdditionalDataWrite");
        Assert.Equal(expectedHex, serializedCborString, StringComparer.Ordinal);
    }

    [Fact]
    public async Task WritesSampleCollectionOfObjectValues()
    {
        // Arrange
        var testEntity = new TestEntity()
        {
            Id = "48d31887-5fad-4d73-a9f5-3c356e68a038",
            Numbers = TestEnum.One | TestEnum.Two,
            TestNamingEnum = TestNamingEnum.Item2SubItem1,
            AdditionalData = new Dictionary<string, object>
                {
                    {"mobilePhone",null}, // write null value
                    {"accountEnabled",false}, // write bool value
                    {"jobTitle","Author"}, // write string value
                    {"createdDateTime", DateTimeOffset.MinValue}, // write date value
                    {"businessPhones", new List<string>() {"+1 412 555 0109"}}, // write collection of primitives value
                    {"manager", new TestEntity{Id = "48d31887-5fad-4d73-a9f5-3c356e68a038"}}, // write nested object value
                }
        };
        var entityList = new List<TestEntity>() { testEntity };
        using var cborSerializerWriter = new CborSerializationWriter();
        // Act
        cborSerializerWriter.WriteCollectionOfObjectValues(string.Empty, entityList);
        // Get the payload from the stream.
        using var serializedStream = cborSerializerWriter.GetSerializedContent();
        var serializedCborString = await TestDataHelper.GetHexRepresentationFromStream(serializedStream, _cancellationTokenSource.Token);

        // Assert
        var expectedHex = TestDataHelper.GetCborHex("TestUserCollectionWrite");
        Assert.Equal(expectedHex, serializedCborString, StringComparer.Ordinal);
    }

    [Fact]
    public async Task WritesEnumValuesAsCamelCasedIfNotEscaped()
    {
        // Arrange
        var testEntity = new TestEntity()
        {
            TestNamingEnum = TestNamingEnum.Item1,
        };
        var entityList = new List<TestEntity>() { testEntity };
        using var cborSerializerWriter = new CborSerializationWriter();
        // Act
        cborSerializerWriter.WriteCollectionOfObjectValues(string.Empty, entityList);
        // Get the payload from the stream.
        using var serializedStream = cborSerializerWriter.GetSerializedContent();
        var serializedCborString = await TestDataHelper.GetHexRepresentationFromStream(serializedStream, _cancellationTokenSource.Token);

        // Assert
        var expectedHex = TestDataHelper.GetCborHex("TestEnumWrite");
        Assert.Equal(expectedHex, serializedCborString, StringComparer.Ordinal);
    }

    [Fact]
    public async Task WritesEnumValuesAsDescribedIfEscaped()
    {
        // Arrange
        var testEntity = new TestEntity()
        {
            TestNamingEnum = TestNamingEnum.Item2SubItem1,
        };
        var entityList = new List<TestEntity>() { testEntity };
        using var cborSerializerWriter = new CborSerializationWriter();
        // Act
        cborSerializerWriter.WriteCollectionOfObjectValues(string.Empty, entityList);
        // Get the payload from the stream.
        using var serializedStream = cborSerializerWriter.GetSerializedContent();
        var serializedCborString = await TestDataHelper.GetHexRepresentationFromStream(serializedStream, _cancellationTokenSource.Token);

        // Assert
        var expectedHex = TestDataHelper.GetCborHex("TestEnumEscapedWrite");
        Assert.Equal(expectedHex, serializedCborString, StringComparer.Ordinal);
    }

    public void Dispose() => _cancellationTokenSource.Dispose();
}
