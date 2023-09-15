using System;
using System.Collections.Generic;
using System.Formats.Cbor;
using System.IO;
using System.Text;
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
    public void WritesSampleObjectValueWithCborElementAdditionalData()
    {
        var writer = new CborWriter();
        writer.WriteSimpleValue(CborSimpleValue.Null);
        var nullCborElement = writer.Encode();
        writer.Reset();
        writer.WriteStartArray(1);
        writer.WriteTextString("[\"+1 412 555 0109\"]");
        writer.WriteEndArray();
        var arrayCborElement = writer.Encode();
        writer.Reset();
        writer.WriteStartMap(1);
        writer.WriteTextString("id");
        writer.WriteTextString("48d31887-5fad-4d73-a9f5-3c356e68a038\"}");
        writer.WriteEndMap();
        var objectCborElement = writer.Encode();
        writer.Reset();

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
        var serializedStream = cborSerializerWriter.GetSerializedContent();
        using var reader = new StreamReader(serializedStream, Encoding.UTF8);
        var serializedCborString = reader.ReadToEnd();

        // Assert
        var expectedString = "{" +
                             "\"id\":\"48d31887-5fad-4d73-a9f5-3c356e68a038\"," +
                             "\"workDuration\":\"PT1H\"," +    // Serializes timespans
                             "\"birthDay\":\"2017-09-04\"," + // Serializes dates
                             "\"startWorkTime\":\"08:00:00\"," + //Serializes times
                             "\"mobilePhone\":null," +
                             "\"accountEnabled\":false," +
                             "\"jobTitle\":\"Author\"," +
                             "\"createdDateTime\":\"0001-01-01T00:00:00+00:00\"," +
                             "\"businessPhones\":[\"\\u002B1 412 555 0109\"]," +
                             "\"manager\":{\"id\":\"48d31887-5fad-4d73-a9f5-3c356e68a038\"}" +
                             "}";
        Assert.Equal(expectedString, serializedCborString, StringComparer.Ordinal);
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
    public void WritesEnumValuesAsCamelCasedIfNotEscaped()
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
        var serializedStream = cborSerializerWriter.GetSerializedContent();
        using var reader = new StreamReader(serializedStream, Encoding.UTF8);
        var serializedCborString = reader.ReadToEnd();

        // Assert
        var expectedString = "[{" +
                             "\"testNamingEnum\":\"item1\"" + // Camel Cased
                             "}]";
        Assert.Equal(expectedString, serializedCborString, StringComparer.Ordinal);
    }

    [Fact]
    public void WritesEnumValuesAsDescribedIfEscaped()
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
        var serializedStream = cborSerializerWriter.GetSerializedContent();
        using var reader = new StreamReader(serializedStream, Encoding.UTF8);
        var serializedCborString = reader.ReadToEnd();

        // Assert
        var expectedString = "[{" +
                             "\"testNamingEnum\":\"Item2:SubItem1\"" + // Appears same as attribute
                             "}]";
        Assert.Equal(expectedString, serializedCborString, StringComparer.Ordinal);
    }

    public void Dispose() => _cancellationTokenSource.Dispose();
}
