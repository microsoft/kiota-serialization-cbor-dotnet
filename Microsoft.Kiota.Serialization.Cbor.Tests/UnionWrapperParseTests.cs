using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Serialization.Cbor.Tests.Mocks;
using Xunit;

namespace Microsoft.Kiota.Serialization.Cbor.Tests;

public sealed class UnionWrapperParseTests : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly CborParseNodeFactory _parseNodeFactory = new();
    private readonly CborSerializationWriterFactory _serializationWriterFactory = new();
    private const string contentType = "application/cbor";
    [Fact]
    public void ParsesUnionTypeComplexProperty1()
    {
        // Given
        using var payload = TestDataHelper.GetCBorDataAsStream("TestUnionTypeComplexProperty1");
        var parseNode = _parseNodeFactory.GetRootParseNode(contentType, payload);

        // When
        var result = parseNode.GetObjectValue(UnionTypeMock.CreateFromDiscriminator);

        // Then
        Assert.NotNull(result);
        Assert.NotNull(result.ComposedType1);
        Assert.Null(result.ComposedType2);
        Assert.Null(result.ComposedType3);
        Assert.Null(result.StringValue);
        Assert.Equal("opaque", result.ComposedType1.Id, StringComparer.Ordinal);
    }
    [Fact]
    public void ParsesUnionTypeComplexProperty2()
    {
        // Given
        using var payload = TestDataHelper.GetCBorDataAsStream("TestUnionTypeComplexProperty2");
        var parseNode = _parseNodeFactory.GetRootParseNode(contentType, payload);

        // When
        var result = parseNode.GetObjectValue(UnionTypeMock.CreateFromDiscriminator);

        // Then
        Assert.NotNull(result);
        Assert.NotNull(result.ComposedType2);
        Assert.Null(result.ComposedType1);
        Assert.Null(result.ComposedType3);
        Assert.Null(result.StringValue);
        Assert.Equal(10, result.ComposedType2.Id);
    }
    [Fact]
    public void ParsesUnionTypeComplexProperty3()
    {
        // Given
        using var payload = TestDataHelper.GetCBorDataAsStream("TestUnionTypeComplexProperty3");
        var parseNode = _parseNodeFactory.GetRootParseNode(contentType, payload);

        // When
        var result = parseNode.GetObjectValue(UnionTypeMock.CreateFromDiscriminator);

        // Then
        Assert.NotNull(result);
        Assert.NotNull(result.ComposedType3);
        Assert.Null(result.ComposedType2);
        Assert.Null(result.ComposedType1);
        Assert.Null(result.StringValue);
        Assert.Equal(2, result.ComposedType3.Count);
        Assert.Equal("11", result.ComposedType3.First().Id, StringComparer.Ordinal);
    }
    [Fact]
    public void ParsesUnionTypeStringValue()
    {
        // Given
        using var payload = TestDataHelper.GetCBorDataAsStream("TestIntersectionTypeString");
        var parseNode = _parseNodeFactory.GetRootParseNode(contentType, payload);

        // When
        var result = parseNode.GetObjectValue(UnionTypeMock.CreateFromDiscriminator);

        // Then
        Assert.NotNull(result);
        Assert.Null(result.ComposedType2);
        Assert.Null(result.ComposedType1);
        Assert.Equal("officeLocation", result.StringValue, StringComparer.Ordinal);
    }
    [Fact]
    public async Task SerializesUnionTypeStringValue()
    {
        // Given
        using var writer = _serializationWriterFactory.GetSerializationWriter(contentType);
        var model = new UnionTypeMock
        {
            StringValue = "officeLocation"
        };

        // When
        writer.WriteObjectValue(string.Empty, model);
        using var serializedStream = writer.GetSerializedContent();
        var serializedCborString = await TestDataHelper.GetHexRepresentationFromStream(serializedStream, _cancellationTokenSource.Token);

        // Assert
        var expectedHex = TestDataHelper.GetCborHex("TestIntersectionTypeString");

        // Then
        Assert.Equal(expectedHex, serializedCborString, StringComparer.Ordinal);
    }
    [Fact]
    public async Task SerializesUnionTypeComplexProperty1()
    {
        // Given
        using var writer = _serializationWriterFactory.GetSerializationWriter(contentType);
        var model = new UnionTypeMock
        {
            ComposedType1 = new()
            {
                Id = "opaque",
                OfficeLocation = "Montreal",
                AdditionalData = new Dictionary<string, object>{
                    {"@odata.type", "#microsoft.graph.testEntity"}
                }
            },
            ComposedType2 = new()
            {
                DisplayName = "McGill",
            },
        };

        // When
        writer.WriteObjectValue(string.Empty, model);
        using var serializedStream = writer.GetSerializedContent();
        var serializedCborString = await TestDataHelper.GetHexRepresentationFromStream(serializedStream, _cancellationTokenSource.Token);

        // Assert
        var expectedHex = TestDataHelper.GetCborHex("TestUnionTypeComplexProperty1");

        // Then
        Assert.Equal(expectedHex, serializedCborString, StringComparer.Ordinal);
    }
    [Fact]
    public async Task SerializesUnionTypeComplexProperty2()
    {
        // Given
        using var writer = _serializationWriterFactory.GetSerializationWriter(contentType);
        var model = new UnionTypeMock
        {
            ComposedType2 = new()
            {
                DisplayName = "McGill",
                Id = 10,
            },
        };

        // When
        writer.WriteObjectValue(string.Empty, model);
        using var serializedStream = writer.GetSerializedContent();
        var serializedCborString = await TestDataHelper.GetHexRepresentationFromStream(serializedStream, _cancellationTokenSource.Token);

        // Assert
        var expectedHex = TestDataHelper.GetCborHex("TestSerializeUnionTypeComplexProperty2");

        // Then
        Assert.Equal(expectedHex, serializedCborString, StringComparer.Ordinal);
    }

    [Fact]
    public async Task SerializesUnionTypeComplexProperty3()
    {
        // Given
        using var writer = _serializationWriterFactory.GetSerializationWriter(contentType);
        var model = new UnionTypeMock
        {
            ComposedType3 = new() {
                new() {
                    OfficeLocation = "Ottawa",
                    Id = "11",
                    AdditionalData = new Dictionary<string, object>{
                        {"@odata.type", "#microsoft.graph.testEntity"}
                    }
                },
                new() {
                    OfficeLocation = "Montreal",
                    Id = "10",
                    AdditionalData = new Dictionary<string, object>{
                        {"@odata.type", "#microsoft.graph.testEntity"}
                    }
                },
            },
        };

        // When
        writer.WriteObjectValue(string.Empty, model);
        using var serializedStream = writer.GetSerializedContent();
        var serializedCborString = await TestDataHelper.GetHexRepresentationFromStream(serializedStream, _cancellationTokenSource.Token);

        // Assert
        var expectedHex = TestDataHelper.GetCborHex("TestUnionTypeComplexProperty3");

        // Then
        Assert.Equal(expectedHex, serializedCborString, StringComparer.Ordinal);
    }
    public void Dispose() => _cancellationTokenSource.Dispose();
}