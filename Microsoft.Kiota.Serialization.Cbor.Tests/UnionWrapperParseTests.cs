using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Kiota.Serialization.Cbor.Tests.Mocks;
using Xunit;

namespace Microsoft.Kiota.Serialization.Cbor.Tests;

public class UnionWrapperParseTests {
    private readonly CborParseNodeFactory _parseNodeFactory = new();
    private readonly CborSerializationWriterFactory _serializationWriterFactory = new();
    private const string contentType = "application/cbor";
    [Fact]
    public void ParsesUnionTypeComplexProperty1()
    {
        // Given
        using var payload = new MemoryStream(Encoding.UTF8.GetBytes("{\"@odata.type\":\"#microsoft.graph.testEntity\",\"officeLocation\":\"Montreal\", \"id\": \"opaque\"}"));
        var parseNode = _parseNodeFactory.GetRootParseNode(contentType, payload);
    
        // When
        var result = parseNode.GetObjectValue<UnionTypeMock>(UnionTypeMock.CreateFromDiscriminator);
    
        // Then
        Assert.NotNull(result);
        Assert.NotNull(result.ComposedType1);
        Assert.Null(result.ComposedType2);
        Assert.Null(result.ComposedType3);
        Assert.Null(result.StringValue);
        Assert.Equal("opaque", result.ComposedType1.Id);
    }
    [Fact]
    public void ParsesUnionTypeComplexProperty2()
    {
        // Given
        using var payload = new MemoryStream(Encoding.UTF8.GetBytes("{\"@odata.type\":\"#microsoft.graph.secondTestEntity\",\"officeLocation\":\"Montreal\", \"id\": 10}"));
        var parseNode = _parseNodeFactory.GetRootParseNode(contentType, payload);
    
        // When
        var result = parseNode.GetObjectValue<UnionTypeMock>(UnionTypeMock.CreateFromDiscriminator);
    
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
        using var payload = new MemoryStream(Encoding.UTF8.GetBytes("[{\"@odata.type\":\"#microsoft.graph.TestEntity\",\"officeLocation\":\"Ottawa\", \"id\": \"11\"}, {\"@odata.type\":\"#microsoft.graph.TestEntity\",\"officeLocation\":\"Montreal\", \"id\": \"10\"}]"));
        var parseNode = _parseNodeFactory.GetRootParseNode(contentType, payload);
    
        // When
        var result = parseNode.GetObjectValue<UnionTypeMock>(UnionTypeMock.CreateFromDiscriminator);
    
        // Then
        Assert.NotNull(result);
        Assert.NotNull(result.ComposedType3);
        Assert.Null(result.ComposedType2);
        Assert.Null(result.ComposedType1);
        Assert.Null(result.StringValue);
        Assert.Equal(2, result.ComposedType3.Count);
        Assert.Equal("11", result.ComposedType3.First().Id);
    }
    [Fact]
    public void ParsesUnionTypeStringValue()
    {
        // Given
        using var payload = new MemoryStream(Encoding.UTF8.GetBytes("\"officeLocation\""));
        var parseNode = _parseNodeFactory.GetRootParseNode(contentType, payload);
    
        // When
        var result = parseNode.GetObjectValue<UnionTypeMock>(UnionTypeMock.CreateFromDiscriminator);
    
        // Then
        Assert.NotNull(result);
        Assert.Null(result.ComposedType2);
        Assert.Null(result.ComposedType1);
        Assert.Equal("officeLocation", result.StringValue);
    }
    [Fact]
    public void SerializesUnionTypeStringValue()
    {
        // Given
        using var writer = _serializationWriterFactory.GetSerializationWriter(contentType);
        var model = new UnionTypeMock {
            StringValue = "officeLocation"
        };
    
        // When
        writer.WriteObjectValue(string.Empty, model);
        using var resultStream = writer.GetSerializedContent();
        using var streamReader = new StreamReader(resultStream);
        var result = streamReader.ReadToEnd();
    
        // Then
        Assert.Equal("\"officeLocation\"", result);
    }
    [Fact]
    public void SerializesUnionTypeComplexProperty1()
    {
        // Given
        using var writer = _serializationWriterFactory.GetSerializationWriter(contentType);
        var model = new UnionTypeMock {
            ComposedType1 = new() {
                Id = "opaque",
                OfficeLocation = "Montreal",
            },
            ComposedType2 = new() {
                DisplayName = "McGill",
            },
        };
    
        // When
        writer.WriteObjectValue(string.Empty, model);
        using var resultStream = writer.GetSerializedContent();
        using var streamReader = new StreamReader(resultStream);
        var result = streamReader.ReadToEnd();
    
        // Then
        Assert.Equal("{\"id\":\"opaque\",\"officeLocation\":\"Montreal\"}", result);
    }
    [Fact]
    public void SerializesUnionTypeComplexProperty2()
    {
        // Given
        using var writer = _serializationWriterFactory.GetSerializationWriter(contentType);
        var model = new UnionTypeMock {
            ComposedType2 = new() {
                DisplayName = "McGill",
                Id = 10,
            },
        };
    
        // When
        writer.WriteObjectValue(string.Empty, model);
        using var resultStream = writer.GetSerializedContent();
        using var streamReader = new StreamReader(resultStream);
        var result = streamReader.ReadToEnd();
    
        // Then
        Assert.Equal("{\"displayName\":\"McGill\",\"id\":10}", result);
    }

    [Fact]
    public void SerializesUnionTypeComplexProperty3()
    {
        // Given
        using var writer = _serializationWriterFactory.GetSerializationWriter(contentType);
        var model = new UnionTypeMock {
            ComposedType3 = new() {
                new() {
                    OfficeLocation = "Montreal",
                    Id = "10",
                },
                new() {
                    OfficeLocation = "Ottawa",
                    Id = "11",
                }
            },
        };
    
        // When
        writer.WriteObjectValue(string.Empty, model);
        using var resultStream = writer.GetSerializedContent();
        using var streamReader = new StreamReader(resultStream);
        var result = streamReader.ReadToEnd();
    
        // Then
        Assert.Equal("[{\"id\":\"10\",\"officeLocation\":\"Montreal\"},{\"id\":\"11\",\"officeLocation\":\"Ottawa\"}]", result);
    }
}