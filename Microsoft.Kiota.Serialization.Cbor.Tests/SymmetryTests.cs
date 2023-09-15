using System;
using System.Formats.Cbor;
using System.IO;
using Microsoft.Kiota.Abstractions;
using Xunit;

namespace Microsoft.Kiota.Serialization.Cbor.Tests;

public sealed class SymmetryTests
{
    [Fact]
    public void SymmetricDateTimeOffset()
    {
        // Given
        var value = DateTimeOffset.UtcNow;
        using var writer = new CborSerializationWriter();

        // When
        writer.WriteDateTimeOffsetValue(string.Empty, value);
        using var serialized = writer.GetSerializedContent();
        Assert.IsAssignableFrom<MemoryStream>(serialized);
        var reader = new CborReader(((MemoryStream)serialized).ToArray());
        var parseNode = new CborParseNode(reader);

        // Then
        Assert.Equal(value, parseNode.GetDateTimeOffsetValue());
    }
    [Fact]
    public void SymmetricString()
    {
        // Given
        var value = "abc";
        using var writer = new CborSerializationWriter();

        // When
        writer.WriteStringValue(string.Empty, value);
        using var serialized = writer.GetSerializedContent();
        Assert.IsAssignableFrom<MemoryStream>(serialized);
        var reader = new CborReader(((MemoryStream)serialized).ToArray());
        var parseNode = new CborParseNode(reader);

        // Then
        Assert.Equal(value, parseNode.GetStringValue());
    }
    [Fact]
    public void SymmetricBool()
    {
        // Given
        var value = true;
        using var writer = new CborSerializationWriter();

        // When
        writer.WriteBoolValue(string.Empty, value);
        using var serialized = writer.GetSerializedContent();
        Assert.IsAssignableFrom<MemoryStream>(serialized);
        var reader = new CborReader(((MemoryStream)serialized).ToArray());
        var parseNode = new CborParseNode(reader);

        // Then
        Assert.Equal(value, parseNode.GetBoolValue());
    }
    [Fact]
    public void SymmetricSByte()
    {
        // Given
        var value = (sbyte)1;
        using var writer = new CborSerializationWriter();

        // When
        writer.WriteSbyteValue(string.Empty, value);
        using var serialized = writer.GetSerializedContent();
        Assert.IsAssignableFrom<MemoryStream>(serialized);
        var reader = new CborReader(((MemoryStream)serialized).ToArray());
        var parseNode = new CborParseNode(reader);

        // Then
        Assert.Equal(value, parseNode.GetSbyteValue());
    }
    [Theory]
    [InlineData(10)]
    [InlineData(-10)]
    public void SymmetricInt(int value)
    {
        // Given
        using var writer = new CborSerializationWriter();

        // When
        writer.WriteIntValue(string.Empty, value);
        using var serialized = writer.GetSerializedContent();
        Assert.IsAssignableFrom<MemoryStream>(serialized);
        var reader = new CborReader(((MemoryStream)serialized).ToArray());
        var parseNode = new CborParseNode(reader);

        // Then
        Assert.Equal(value, parseNode.GetIntValue());
    }
    [Theory]
    [InlineData(10)]
    [InlineData(-10)]
    public void SymmetricLong(long value)
    {
        // Given
        using var writer = new CborSerializationWriter();

        // When
        writer.WriteLongValue(string.Empty, value);
        using var serialized = writer.GetSerializedContent();
        Assert.IsAssignableFrom<MemoryStream>(serialized);
        var reader = new CborReader(((MemoryStream)serialized).ToArray());
        var parseNode = new CborParseNode(reader);

        // Then
        Assert.Equal(value, parseNode.GetLongValue());
    }
    [Theory]
    [InlineData(10)]
    [InlineData(-10)]
    public void SymmetricDouble(double value)
    {
        // Given
        using var writer = new CborSerializationWriter();

        // When
        writer.WriteDoubleValue(string.Empty, value);
        using var serialized = writer.GetSerializedContent();
        Assert.IsAssignableFrom<MemoryStream>(serialized);
        var reader = new CborReader(((MemoryStream)serialized).ToArray());
        var parseNode = new CborParseNode(reader);

        // Then
        Assert.Equal(value, parseNode.GetDoubleValue());
    }
    [Theory]
    [InlineData(10)]
    [InlineData(-10)]
    public void SymmetricDecimal(decimal value)
    {
        // Given
        using var writer = new CborSerializationWriter();

        // When
        writer.WriteDecimalValue(string.Empty, value);
        using var serialized = writer.GetSerializedContent();
        Assert.IsAssignableFrom<MemoryStream>(serialized);
        var reader = new CborReader(((MemoryStream)serialized).ToArray());
        var parseNode = new CborParseNode(reader);

        // Then
        Assert.Equal(value, parseNode.GetDecimalValue());
    }
    [Fact]
    public void SymmetricTimeSpan()
    {
        // Given
        var value = TimeSpan.FromDays(1);
        using var writer = new CborSerializationWriter();

        // When
        writer.WriteTimeSpanValue(string.Empty, value);
        using var serialized = writer.GetSerializedContent();
        Assert.IsAssignableFrom<MemoryStream>(serialized);
        var reader = new CborReader(((MemoryStream)serialized).ToArray());
        var parseNode = new CborParseNode(reader);

        // Then
        Assert.Equal(value, parseNode.GetTimeSpanValue());
    }
    [Fact]
    public void SymmetricDate()
    {
        // Given
        var value = new Date(2023, 05, 04);
        using var writer = new CborSerializationWriter();

        // When
        writer.WriteDateValue(string.Empty, value);
        using var serialized = writer.GetSerializedContent();
        Assert.IsAssignableFrom<MemoryStream>(serialized);
        var reader = new CborReader(((MemoryStream)serialized).ToArray());
        var parseNode = new CborParseNode(reader);

        // Then
        Assert.Equal(value, parseNode.GetDateValue());
    }
    [Fact]
    public void SymmetricTime()
    {
        // Given
        var value = new Time(16, 20, 0);
        using var writer = new CborSerializationWriter();

        // When
        writer.WriteTimeValue(string.Empty, value);
        using var serialized = writer.GetSerializedContent();
        Assert.IsAssignableFrom<MemoryStream>(serialized);
        var reader = new CborReader(((MemoryStream)serialized).ToArray());
        var parseNode = new CborParseNode(reader);

        // Then
        Assert.Equal(value.ToString(), parseNode.GetTimeValue().ToString(), StringComparer.OrdinalIgnoreCase);
    }
}
