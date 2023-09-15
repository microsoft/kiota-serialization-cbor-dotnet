using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Serialization.Cbor.Tests.Mocks;
using Xunit;

namespace Microsoft.Kiota.Serialization.Cbor.Tests;

public sealed class IntersectionWrapperParseTests : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly CborParseNodeFactory _parseNodeFactory = new();
    private readonly CborSerializationWriterFactory _serializationWriterFactory = new();
    private const string contentType = "application/cbor";
    [Fact]
    public void ParsesIntersectionTypeComplexProperty1()
    {
        // Given
        using var payload = TestDataHelper.GetCBorDataAsStream("TestIntersectionTypeComplexProperty1");

        var parseNode = _parseNodeFactory.GetRootParseNode(contentType, payload);

        // When
        var result = parseNode.GetObjectValue(IntersectionTypeMock.CreateFromDiscriminator);

        // Then
        Assert.NotNull(result);
        Assert.NotNull(result.ComposedType1);
        Assert.NotNull(result.ComposedType2);
        Assert.Null(result.ComposedType3);
        Assert.Null(result.StringValue);
        Assert.Equal("opaque", result.ComposedType1.Id, StringComparer.Ordinal);
        Assert.Equal("McGill", result.ComposedType2.DisplayName, StringComparer.Ordinal);
    }
    [Fact]
    public void ParsesIntersectionTypeComplexProperty2()
    {
        // Given
        using var payload = TestDataHelper.GetCBorDataAsStream("TestIntersectionTypeComplexProperty2");
        var parseNode = _parseNodeFactory.GetRootParseNode(contentType, payload);

        // When
        var result = parseNode.GetObjectValue(IntersectionTypeMock.CreateFromDiscriminator);

        // Then
        Assert.NotNull(result);
        Assert.NotNull(result.ComposedType1);
        Assert.NotNull(result.ComposedType2);
        Assert.Null(result.ComposedType3);
        Assert.Null(result.StringValue);
        Assert.Null(result.ComposedType1.Id);
        Assert.Null(result.ComposedType2.Id); // it's expected to be null since we have conflicting properties here and the parser will only try one to avoid having to brute its way through
        Assert.Equal("McGill", result.ComposedType2.DisplayName, StringComparer.Ordinal);
        Assert.Equal("Montreal", result.ComposedType1.OfficeLocation, StringComparer.Ordinal);
    }
    [Fact]
    public void ParsesIntersectionTypeComplexProperty3()
    {
        // Given
        using var payload = TestDataHelper.GetCBorDataAsStream("TestIntersectionTypeComplexProperty3");
        var parseNode = _parseNodeFactory.GetRootParseNode(contentType, payload);

        // When
        var result = parseNode.GetObjectValue(IntersectionTypeMock.CreateFromDiscriminator);

        // Then
        Assert.NotNull(result);
        Assert.Null(result.ComposedType1);
        Assert.Null(result.ComposedType2);
        Assert.NotNull(result.ComposedType3);
        Assert.Null(result.StringValue);
        Assert.Equal(2, result.ComposedType3.Count);
        Assert.Equal("Ottawa", result.ComposedType3.First().OfficeLocation, StringComparer.Ordinal);
    }
    [Fact]
    public void ParsesIntersectionTypeStringValue()
    {
        // Given
        using var payload = TestDataHelper.GetCBorDataAsStream("TestIntersectionTypeString");
        var parseNode = _parseNodeFactory.GetRootParseNode(contentType, payload);

        // When
        var result = parseNode.GetObjectValue(IntersectionTypeMock.CreateFromDiscriminator);

        // Then
        Assert.NotNull(result);
        Assert.Null(result.ComposedType2);
        Assert.Null(result.ComposedType1);
        Assert.Null(result.ComposedType3);
        Assert.Equal("officeLocation", result.StringValue, StringComparer.Ordinal);
    }
    [Fact]
    public async Task SerializesIntersectionTypeStringValue()
    {
        // Given
        using var writer = _serializationWriterFactory.GetSerializationWriter(contentType);
        var model = new IntersectionTypeMock
        {
            StringValue = "officeLocation"
        };

        // When
        model.Serialize(writer);
        // Get the payload from the stream.
        using var serializedStream = writer.GetSerializedContent();
        var serializedCborString = await TestDataHelper.GetHexRepresentationFromStream(serializedStream, _cancellationTokenSource.Token);

        // Assert
        var expectedHex = TestDataHelper.GetCborHex("TestIntersectionTypeString");

        // Then
        Assert.Equal(expectedHex, serializedCborString, StringComparer.Ordinal);
    }
    [Fact]
    public async Task SerializesIntersectionTypeComplexProperty1()
    {
        // Given
        using var writer = _serializationWriterFactory.GetSerializationWriter(contentType);
        var model = new IntersectionTypeMock
        {
            ComposedType1 = new()
            {
                Id = "opaque",
                OfficeLocation = "Montreal",
            },
            ComposedType2 = new()
            {
                DisplayName = "McGill",
            },
        };

        // When
        model.Serialize(writer);
        // Get the payload from the stream.
        using var serializedStream = writer.GetSerializedContent();
        var serializedCborString = await TestDataHelper.GetHexRepresentationFromStream(serializedStream, _cancellationTokenSource.Token);

        // Assert
        var expectedHex = TestDataHelper.GetCborHex("TestIntersectionTypeComplexProperty1");

        // Then
        Assert.Equal(expectedHex, serializedCborString, StringComparer.Ordinal);
    }
    [Fact]
    public async Task SerializesIntersectionTypeComplexProperty2()
    {
        // Given
        using var writer = _serializationWriterFactory.GetSerializationWriter(contentType);
        var model = new IntersectionTypeMock
        {
            ComposedType2 = new()
            {
                DisplayName = "McGill",
                Id = 10,
            },
            ComposedType1 = new()
            {
                OfficeLocation = "Montreal",
            },
        };

        // When
        model.Serialize(writer);
        // Get the payload from the stream.
        using var serializedStream = writer.GetSerializedContent();
        var serializedCborString = await TestDataHelper.GetHexRepresentationFromStream(serializedStream, _cancellationTokenSource.Token);

        // Assert
        var expectedHex = TestDataHelper.GetCborHex("TestIntersectionTypeComplexProperty2");

        // Then
        Assert.Equal(expectedHex, serializedCborString, StringComparer.Ordinal);
    }

    [Fact]
    public async Task SerializesIntersectionTypeComplexProperty3()
    {
        // Given
        using var writer = _serializationWriterFactory.GetSerializationWriter(contentType);
        var model = new IntersectionTypeMock
        {
            ComposedType3 = new() {
                new() {
                    OfficeLocation = "Ottawa",
                    Id = "11",
                    AdditionalData = new Dictionary<string, object>{
                        { "@odata.type", "#microsoft.graph.TestEntity"}
                    }
                },
                new() {
                    OfficeLocation = "Montreal",
                    Id = "10",
                    AdditionalData = new Dictionary<string, object>{
                        { "@odata.type", "#microsoft.graph.TestEntity"}
                    }
                },
            },
        };

        // When
        model.Serialize(writer);
        // Get the payload from the stream.
        using var serializedStream = writer.GetSerializedContent();
        var serializedCborString = await TestDataHelper.GetHexRepresentationFromStream(serializedStream, _cancellationTokenSource.Token);

        // Assert
        var expectedHex = TestDataHelper.GetCborHex("TestIntersectionTypeComplexProperty3");

        // Then
        Assert.Equal(expectedHex, serializedCborString, StringComparer.Ordinal);
    }
    public void Dispose() => _cancellationTokenSource.Dispose();
}