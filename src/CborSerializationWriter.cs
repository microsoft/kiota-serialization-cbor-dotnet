﻿// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Linq;
using System.IO;
using System.Formats.Cbor;
using Microsoft.Kiota.Abstractions.Serialization;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Kiota.Abstractions.Extensions;
using Microsoft.Kiota.Abstractions;
using System.Xml;

namespace Microsoft.Kiota.Serialization.Cbor
{
    /// <summary>
    /// The <see cref="ISerializationWriter"/> implementation for cbor content types.
    /// </summary>
    public class CborSerializationWriter : ISerializationWriter, IDisposable
    {
        private readonly MemoryStream _stream = new MemoryStream();

        /// <summary>
        /// The <see cref="CborWriter"/> instance for writing cbor content
        /// </summary>
        public readonly CborWriter writer;

        /// <summary>
        /// The <see cref="CborSerializationWriter"/> constructor
        /// </summary>
        public CborSerializationWriter()
        {
            writer = new CborWriter();
        }

        /// <summary>
        /// The action to perform before object serialization
        /// </summary>
        public Action<IParsable>? OnBeforeObjectSerialization { get; set; }

        /// <summary>
        /// The action to perform after object serialization
        /// </summary>
        public Action<IParsable>? OnAfterObjectSerialization { get; set; }

        /// <summary>
        /// The action to perform on the start of object serialization
        /// </summary>
        public Action<IParsable, ISerializationWriter>? OnStartObjectSerialization { get; set; }

        /// <summary>
        /// Get the stream of the serialized content
        /// </summary>
        /// <returns>The <see cref="Stream"/> of the serialized content</returns>
        public Stream GetSerializedContent() {
            var stream = new MemoryStream(writer.Encode());
            return stream;
        }

        /// <summary>
        /// Write the string value
        /// </summary>
        /// <param name="key">The key of the cbor node</param>
        /// <param name="value">The string value</param>
        public void WriteStringValue(string? key, string? value)
        {
            if(value != null)
            { // we want to keep empty string because they are meaningful
                if(!string.IsNullOrEmpty(key)) writer.WriteTextString(key!);
                writer.WriteTextString(value);
            }
        }

        /// <summary>
        /// Write the boolean value
        /// </summary>
        /// <param name="key">The key of the cbor node</param>
        /// <param name="value">The boolean value</param>
        public void WriteBoolValue(string? key, bool? value)
        {
            if(!string.IsNullOrEmpty(key) && value.HasValue) writer.WriteTextString(key!);
            if(value.HasValue) writer.WriteBoolean(value.Value);
        }

        /// <summary>
        /// Write the byte value
        /// </summary>
        /// <param name="key">The key of the cbor node</param>
        /// <param name="value">The byte value</param>
        public void WriteByteValue(string? key, byte? value)
        {
            if(!string.IsNullOrEmpty(key) && value.HasValue) writer.WriteTextString(key!);
            if(value.HasValue) writer.WriteInt32(Convert.ToInt32(value.Value));
        }

        /// <summary>
        /// Write the sbyte value
        /// </summary>
        /// <param name="key">The key of the cbor node</param>
        /// <param name="value">The sbyte value</param>
        public void WriteSbyteValue(string? key, sbyte? value)
        {
            if(!string.IsNullOrEmpty(key) && value.HasValue) writer.WriteTextString(key!);
            if(value.HasValue) writer.WriteInt32(Convert.ToInt32(value.Value));
        }

        /// <summary>
        /// Write the int value
        /// </summary>
        /// <param name="key">The key of the cbor node</param>
        /// <param name="value">The int value</param>
        public void WriteIntValue(string? key, int? value)
        {
            if(!string.IsNullOrEmpty(key) && value.HasValue) writer.WriteTextString(key!);
            if(value.HasValue) writer.WriteInt32(value.Value);
        }

        /// <summary>
        /// Write the float value
        /// </summary>
        /// <param name="key">The key of the cbor node</param>
        /// <param name="value">The float value</param>
        public void WriteFloatValue(string? key, float? value)
        {
            if(!string.IsNullOrEmpty(key) && value.HasValue) writer.WriteTextString(key!);
            if(value.HasValue) writer.WriteSingle(value.Value);
        }

        /// <summary>
        /// Write the long value
        /// </summary>
        /// <param name="key">The key of the cbor node</param>
        /// <param name="value">The long value</param>
        public void WriteLongValue(string? key, long? value)
        {
            if(!string.IsNullOrEmpty(key) && value.HasValue) writer.WriteTextString(key!);
            if(value.HasValue) writer.WriteInt64(value.Value);
        }

        /// <summary>
        /// Write the double value
        /// </summary>
        /// <param name="key">The key of the cbor node</param>
        /// <param name="value">The double value</param>
        public void WriteDoubleValue(string? key, double? value)
        {
            if(!string.IsNullOrEmpty(key) && value.HasValue) writer.WriteTextString(key!);
            if(value.HasValue) writer.WriteDouble(value.Value);
        }

        /// <summary>
        /// Write the decimal value
        /// </summary>
        /// <param name="key">The key of the cbor node</param>
        /// <param name="value">The decimal value</param>
        public void WriteDecimalValue(string? key, decimal? value)
        {
            if(!string.IsNullOrEmpty(key) && value.HasValue) writer.WriteTextString(key!);
            if(value.HasValue) writer.WriteDecimal(value.Value);
        }

        /// <summary>
        /// Write the Guid value
        /// </summary>
        /// <param name="key">The key of the cbor node</param>
        /// <param name="value">The Guid value</param>
        public void WriteGuidValue(string? key, Guid? value)
        {
            if(!string.IsNullOrEmpty(key) && value.HasValue) writer.WriteTextString(key!);
            if(value.HasValue) writer.WriteTextString(value.Value.ToString());
        }

        /// <summary>
        /// Write the DateTimeOffset value
        /// </summary>
        /// <param name="key">The key of the cbor node</param>
        /// <param name="value">The DateTimeOffset value</param>
        public void WriteDateTimeOffsetValue(string? key, DateTimeOffset? value)
        {
            if(!string.IsNullOrEmpty(key) && value.HasValue) writer.WriteTextString(key!);
            if(value.HasValue) writer.WriteDateTimeOffset(value.Value);
        }

        /// <summary>
        /// Write the TimeSpan(An ISO8601 duration.For example, PT1M is "period time of 1 minute") value.
        /// </summary>
        /// <param name="key">The key of the cbor node</param>
        /// <param name="value">The TimeSpan value</param>
        public void WriteTimeSpanValue(string? key, TimeSpan? value)
        {
            if(!string.IsNullOrEmpty(key) && value.HasValue) writer.WriteTextString(key!);
            if(value.HasValue) writer.WriteTextString(XmlConvert.ToString(value.Value));
        }

        /// <summary>
        /// Write the Date value
        /// </summary>
        /// <param name="key">The key of the cbor node</param>
        /// <param name="value">The Date value</param>
        public void WriteDateValue(string? key, Date? value)
        {
            if(!string.IsNullOrEmpty(key) && value.HasValue) writer.WriteTextString(key!);
            if(value.HasValue) writer.WriteTextString(value.Value.ToString());
        }

        /// <summary>
        /// Write the Time value
        /// </summary>
        /// <param name="key">The key of the cbor node</param>
        /// <param name="value">The Time value</param>
        public void WriteTimeValue(string? key, Time? value)
        {
            if(!string.IsNullOrEmpty(key) && value.HasValue) writer.WriteTextString(key!);
            if(value.HasValue) writer.WriteTextString(value.Value.ToString());
        }

        /// <summary>
        /// Write the null value
        /// </summary>
        /// <param name="key">The key of the cbor node</param>
        public void WriteNullValue(string? key)
        {
            if(!string.IsNullOrEmpty(key)) writer.WriteTextString(key!);
            writer.WriteSimpleValue(CborSimpleValue.Null);
        }

        /// <summary>
        /// Write the enumeration value of type  <typeparam name="T"/>
        /// </summary>
        /// <param name="key">The key of the cbor node</param>
        /// <param name="value">The enumeration value</param>
        public void WriteEnumValue<T>(string? key, T? value) where T : struct, Enum
        {
            if(!string.IsNullOrEmpty(key) && value.HasValue) writer.WriteTextString(key!);
            if(value.HasValue)
            {
                if(typeof(T).GetCustomAttributes<FlagsAttribute>().Any())
                    writer.WriteTextString(Enum.GetValues(typeof(T))
                                            .Cast<T>()
                                            .Where(x => value.Value.HasFlag(x))
                                            .Select(GetEnumName)
                                            .Aggregate((x, y) => $"{x},{y}") ?? "");
                else writer.WriteTextString(GetEnumName(value.Value)?? "");
            }
        }

        /// <summary>
        /// Write the collection of primitives of type  <typeparam name="T"/>
        /// </summary>
        /// <param name="key">The key of the cbor node</param>
        /// <param name="values">The primitive collection</param>
        public void WriteCollectionOfPrimitiveValues<T>(string? key, IEnumerable<T>? values)
        {
            if(values != null)
            { //empty array is meaningful
                if(!string.IsNullOrEmpty(key)) writer.WriteTextString(key!);
                writer.WriteStartArray(values.Count());
                foreach(var collectionValue in values)
                    WriteAnyValue(null, collectionValue);
                writer.WriteEndArray();
            }
        }

        /// <summary>
        /// Write the collection of objects of type  <typeparam name="T"/>
        /// </summary>
        /// <param name="key">The key of the cbor node</param>
        /// <param name="values">The object collection</param>
        public void WriteCollectionOfObjectValues<T>(string? key, IEnumerable<T>? values) where T : IParsable
        {
            if(values != null)
            { //empty array is meaningful
                if(!string.IsNullOrEmpty(key)) writer.WriteTextString(key!);
                writer.WriteStartArray(values.Count());
                foreach(var item in values)
                    WriteObjectValue<T>(null, item);
                writer.WriteEndArray();
            }
        }
        /// <summary>
        /// Writes the specified collection of enum values to the stream with an optional given key.
        /// </summary>
        /// <param name="key">The key to be used for the written value. May be null.</param>
        /// <param name="values">The enum values to be written.</param>
        public void WriteCollectionOfEnumValues<T>(string? key, IEnumerable<T?>? values) where T : struct, Enum
        {
            if(values != null)
            { //empty array is meaningful
                if(!string.IsNullOrEmpty(key)) writer.WriteTextString(key!);
                writer.WriteStartArray(values.Count());
                foreach(var item in values)
                    WriteEnumValue<T>(null, item);
                writer.WriteEndArray();
            }
        }
        /// <summary>
        /// Writes the specified byte array as a base64 string to the stream with an optional given key.
        /// </summary>
        /// <param name="key">The key to be used for the written value. May be null.</param>
        /// <param name="value">The byte array to be written.</param>
        public void WriteByteArrayValue(string? key, byte[]? value)
        {
            if(value != null)//empty array is meaningful
                WriteStringValue(key, value.Any() ? Convert.ToBase64String(value) : string.Empty);
        }

        /// <summary>
        /// Write the object of type <typeparam name="T"/>
        /// </summary>
        /// <param name="key">The key of the cbor node</param>
        /// <param name="value">The object instance to write</param>
        /// <param name="additionalValuesToMerge">The additional values to merge to the main value when serializing an intersection wrapper.</param>
        public void WriteObjectValue<T>(string? key, T? value, params IParsable?[] additionalValuesToMerge) where T : IParsable
        {
            var filteredAdditionalValuesToMerge = additionalValuesToMerge.OfType<IParsable>().ToArray();
            if(value != null || filteredAdditionalValuesToMerge.Any())
            {
                if(!string.IsNullOrEmpty(key)) writer.WriteTextString(key!);
                if(value != null) OnBeforeObjectSerialization?.Invoke(value);
                var serializingScalarValue = value is IComposedTypeWrapper;
                if (!serializingScalarValue)
                    writer.WriteStartMap(null);
                if(value != null) {
                    OnStartObjectSerialization?.Invoke(value, this);
                    value.Serialize(this);
                }
                foreach(var additionalValueToMerge in filteredAdditionalValuesToMerge)
                {
                    OnBeforeObjectSerialization?.Invoke(additionalValueToMerge!);
                    OnStartObjectSerialization?.Invoke(additionalValueToMerge!, this);
                    additionalValueToMerge!.Serialize(this);
                    OnAfterObjectSerialization?.Invoke(additionalValueToMerge);
                }
                if (!serializingScalarValue)
                    writer.WriteEndMap();
                if(value != null) OnAfterObjectSerialization?.Invoke(value);
            }
        }

        /// <summary>
        /// Write the additional data property bag
        /// </summary>
        /// <param name="value">The additional data dictionary</param>
        public void WriteAdditionalData(IDictionary<string, object> value)
        {
            if(value == null) return;

            foreach(var dataValue in value)
                WriteAnyValue(dataValue.Key, dataValue.Value);
        }

        private void WriteNonParsableObjectValue<T>(string? key, T value)
        {
            if(!string.IsNullOrEmpty(key))
                writer.WriteTextString(key!);
            writer.WriteStartMap(null);
            if(value == null) writer.WriteSimpleValue(CborSimpleValue.Null);
            else
                foreach(var oProp in value.GetType().GetProperties())
                    WriteAnyValue(oProp.Name, oProp.GetValue(value));
            writer.WriteEndMap();
        }
        private void WriteAnyValue<T>(string? key, T value)
        {
            switch(value)
            {
                case string s:
                    WriteStringValue(key, s);
                    break;
                case bool b:
                    WriteBoolValue(key, b);
                    break;
                case byte b:
                    WriteByteValue(key, b);
                    break;
                case sbyte b:
                    WriteSbyteValue(key, b);
                    break;
                case int i:
                    WriteIntValue(key, i);
                    break;
                case float f:
                    WriteFloatValue(key, f);
                    break;
                case long l:
                    WriteLongValue(key, l);
                    break;
                case double d:
                    WriteDoubleValue(key, d);
                    break;
                case Guid g:
                    WriteGuidValue(key, g);
                    break;
                case DateTimeOffset dto:
                    WriteDateTimeOffsetValue(key, dto);
                    break;
                case TimeSpan timeSpan:
                    WriteTimeSpanValue(key, timeSpan);
                    break;
                case IEnumerable<object> coll:
                    WriteCollectionOfPrimitiveValues(key, coll);
                    break;
                case IParsable parseable:
                    WriteObjectValue(key, parseable);
                    break;
                case Date date:
                    WriteDateValue(key, date);
                    break;
                case DateTime dateTime:
                    WriteDateTimeOffsetValue(key, new DateTimeOffset(dateTime));
                    break;
                case Time time:
                    WriteTimeValue(key, time);
                    break;
                // case JsonElement jsonElement:
                //     if(!string.IsNullOrEmpty(key)) writer.WriteTextString(key!);
                //     jsonElement.WriteTo(writer);
                //     break;
                case object o:
                    WriteNonParsableObjectValue(key, o);
                    break;
                case null:
                    WriteNullValue(key);
                    break;
                default:
                    throw new InvalidOperationException($"error serialization additional data value with key {key}, unknown type {value?.GetType()}");
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            //writer.Dispose();
            GC.SuppressFinalize(this);
        }
        
        private string? GetEnumName<T>(T value) where T : struct, Enum
        {
            var type = typeof(T);

            if (Enum.GetName(type, value) is not { } name)
                throw new ArgumentException($"Invalid Enum value {value} for enum of type {type}");
            
            if (type.GetMember(name).FirstOrDefault()?.GetCustomAttribute<EnumMemberAttribute>() is { } attribute)
                return attribute.Value;
            
            return name.ToFirstCharacterLowerCase();
        }
    }
}
