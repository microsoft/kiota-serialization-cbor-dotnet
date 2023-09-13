// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Cbor;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Extensions;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Microsoft.Kiota.Serialization.Cbor
{
    /// <summary>
    /// The <see cref="IParseNode"/> implementation for the CBOR content type
    /// </summary>
    public class CborParseNode : IParseNode
    {
        /// <summary>
        /// The <see cref="CborParseNode"/> constructor.
        /// </summary>
        /// <param name="reader">The CborReader to initialize the node with</param>
        public CborParseNode(CborReader reader)
        {
            this.reader = reader;
        }

        /// <summary>
        /// Get the string value from the cbor node
        /// </summary>
        /// <returns>A string value</returns>
        public string? GetStringValue() => reader.ReadTextString();

        /// <summary>
        /// Get the boolean value from the cbor node
        /// </summary>
        /// <returns>A boolean value</returns>
        public bool? GetBoolValue() => reader.ReadBoolean();

        /// <summary>
        /// Get the byte value from the cbor node
        /// </summary>
        /// <returns>A byte value</returns>
        public byte? GetByteValue() => Convert.ToByte(reader.ReadUInt32());

        /// <summary>
        /// Get the sbyte value from the cbor node
        /// </summary>
        /// <returns>A sbyte value</returns>
        public sbyte? GetSbyteValue() => Convert.ToSByte(reader.ReadInt32());

        /// <summary>
        /// Get the int value from the cbor node
        /// </summary>
        /// <returns>A int value</returns>
        public int? GetIntValue() => reader.ReadInt32();

        /// <summary>
        /// Get the float value from the cbor node
        /// </summary>
        /// <returns>A float value</returns>
        public float? GetFloatValue() => reader.ReadSingle();

        /// <summary>
        /// Get the Long value from the cbor node
        /// </summary>
        /// <returns>A Long value</returns>
        public long? GetLongValue() => reader.ReadInt64();

        /// <summary>
        /// Get the double value from the cbor node
        /// </summary>
        /// <returns>A double value</returns>
        public double? GetDoubleValue() => reader.ReadDouble();

        /// <summary>
        /// Get the decimal value from the cbor node
        /// </summary>
        /// <returns>A decimal value</returns>
        public decimal? GetDecimalValue() => reader.ReadDecimal();

        /// <summary>
        /// Get the guid value from the cbor node
        /// </summary>
        /// <returns>A guid value</returns>
        public Guid? GetGuidValue()
        {
            var guidString = GetStringValue();
            if(!Guid.TryParse(guidString, out var result))
                return null;

            return result;
        }

        /// <summary>
        /// Get the <see cref="DateTimeOffset"/> value from the cbor node
        /// </summary>
        /// <returns>A <see cref="DateTimeOffset"/> value</returns>
        public DateTimeOffset? GetDateTimeOffsetValue()
        {
            if(reader.PeekState() is CborReaderState.TextString or CborReaderState.StartIndefiniteLengthTextString)
                return DateTimeOffset.Parse(reader.ReadTextString()!);
            else
                return reader.ReadDateTimeOffset();
        }

        /// <summary>
        /// Get the <see cref="TimeSpan"/> value from the cbor node
        /// </summary>
        /// <returns>A <see cref="TimeSpan"/> value</returns>
        public TimeSpan? GetTimeSpanValue()
        {
            var jsonString = GetStringValue();
            if(string.IsNullOrEmpty(jsonString))
                return null;

            // Parse an ISO8601 duration.http://en.wikipedia.org/wiki/ISO_8601#Durations to a TimeSpan
            return XmlConvert.ToTimeSpan(jsonString);
        }

        /// <summary>
        /// Get the <see cref="Date"/> value from the cbor node
        /// </summary>
        /// <returns>A <see cref="Date"/> value</returns>
        public Date? GetDateValue()
        {
            var dateString = GetStringValue();
            if(!DateTime.TryParse(dateString, out var result))
                return null;

            return new Date(result);
        }

        /// <summary>
        /// Get the <see cref="Time"/> value from the cbor node
        /// </summary>
        /// <returns>A <see cref="Time"/> value</returns>
        public Time? GetTimeValue()
        {
            var dateString = GetStringValue();
            if(!DateTime.TryParse(dateString, out var result))
                return null;

            return new Time(result);
        }

        /// <summary>
        /// Get the enumeration value of type <typeparam name="T"/>from the cbor node
        /// </summary>
        /// <returns>An enumeration value or null</returns>
        public T? GetEnumValue<T>() where T : struct, Enum
        {
            var rawValue = GetStringValue();
            if(string.IsNullOrEmpty(rawValue)) return null;

            var type = typeof(T);
            rawValue = ToEnumRawName<T>(type, rawValue!);
            if(type.GetCustomAttributes<FlagsAttribute>().Any())
            {
                return (T)(object)rawValue!
                    .Split(',')
                    .Select(static x => Enum.TryParse<T>(x, true, out var result) ? result : (T?)null)
                    .Where(static x => !x.Equals(null))
                    .Select(static x => (int)(object)x!)
                    .Sum();
            }
            else
                return Enum.TryParse<T>(rawValue, true, out var result) ? result : null;
        }

        /// <summary>
        /// Get the collection of type <typeparam name="T"/>from the cbor node
        /// </summary>
        /// <param name="factory">The factory to use to create the model object.</param>
        /// <returns>A collection of objects</returns>
        public IEnumerable<T> GetCollectionOfObjectValues<T>(ParsableFactory<T> factory) where T : IParsable
        {
            if(reader.PeekState() == CborReaderState.StartArray)
            {
                while(reader.PeekState() != CborReaderState.EndArray)
                {
                    yield return GetObjectValue(factory);
                }
            }
        }
        /// <summary>
        /// Gets the collection of enum values of the node.
        /// </summary>
        /// <returns>The collection of enum values.</returns>
        public IEnumerable<T?> GetCollectionOfEnumValues<T>() where T : struct, Enum
        {
            if(reader.PeekState() == CborReaderState.StartArray)
            {
                while(reader.PeekState() != CborReaderState.EndArray)
                {
                    yield return GetEnumValue<T>();
                }
            }
        }

        /// <summary>
        /// Gets the byte array value of the node.
        /// </summary>
        /// <returns>The byte array value of the node.</returns>
        public byte[]? GetByteArrayValue()
        {
            var rawValue = GetStringValue();
            if(string.IsNullOrEmpty(rawValue)) return null;
            return Convert.FromBase64String(rawValue);
        }
        private static readonly Type booleanType = typeof(bool?);
        private static readonly Type byteType = typeof(byte?);
        private static readonly Type sbyteType = typeof(sbyte?);
        private static readonly Type stringType = typeof(string);
        private static readonly Type intType = typeof(int?);
        private static readonly Type floatType = typeof(float?);
        private static readonly Type longType = typeof(long?);
        private static readonly Type doubleType = typeof(double?);
        private static readonly Type guidType = typeof(Guid?);
        private static readonly Type dateTimeOffsetType = typeof(DateTimeOffset?);
        private static readonly Type timeSpanType = typeof(TimeSpan?);
        private static readonly Type dateType = typeof(Date?);
        private static readonly Type timeType = typeof(Time?);

        /// <summary>
        /// Get the collection of primitives of type <typeparam name="T"/>from the cbor node
        /// </summary>
        /// <returns>A collection of objects</returns>
        public IEnumerable<T> GetCollectionOfPrimitiveValues<T>()
        {

            if(reader.PeekState() == CborReaderState.StartArray)
            {
                var genericType = typeof(T);
                while(reader.PeekState() != CborReaderState.EndArray)
                {
                    if(genericType == booleanType)
                        yield return (T)(object)reader.ReadBoolean()!;
                    // else if(genericType == byteType)
                    //     yield return (T)(object)reader.ReadByte()!;
                    // else if(genericType == sbyteType)
                    //     yield return (T)(object)currentParseNode.GetSbyteValue()!;
                    else if(genericType == stringType)
                        yield return (T)(object)reader.ReadTextString()!;
                    else if(genericType == intType)
                        yield return (T)(object)reader.ReadInt32()!;
                    else if(genericType == floatType)
                        yield return (T)(object)reader.ReadSingle()!;
                    else if(genericType == longType)
                        yield return (T)(object)reader.ReadInt64()!;
                    else if(genericType == doubleType)
                        yield return (T)(object)reader.ReadDouble()!;
                    // else if(genericType == guidType)
                    //     yield return (T)(object)currentParseNode.GetGuidValue()!;
                    else if(genericType == dateTimeOffsetType)
                        yield return (T)(object)reader.ReadDateTimeOffset()!;
                    // else if(genericType == timeSpanType)
                    //     yield return (T)(object)currentParseNode.GetTimeSpanValue()!;
                    // else if(genericType == dateType)
                    //     yield return (T)(object)currentParseNode.GetDateValue()!;
                    // else if(genericType == timeType)
                    //     yield return (T)(object)currentParseNode.GetTimeValue()!;
                    else
                        throw new InvalidOperationException($"unknown type for deserialization {genericType.FullName}");
                }
            }
        }

        /// <summary>
        /// The action to perform before assigning field values.
        /// </summary>
        public Action<IParsable>? OnBeforeAssignFieldValues { get; set; }

        /// <summary>
        /// The action to perform after assigning field values.
        /// </summary>
        public Action<IParsable>? OnAfterAssignFieldValues { get; set; }
        private readonly CborReader reader;

        /// <summary>
        /// Get the object of type <typeparam name="T"/>from the cbor node
        /// </summary>
        /// <param name="factory">The factory to use to create the model object.</param>
        /// <returns>A object of the specified type</returns>
        public T GetObjectValue<T>(ParsableFactory<T> factory) where T : IParsable
        {
            var item = factory(this);
            OnBeforeAssignFieldValues?.Invoke(item);
            AssignFieldValues(item);
            OnAfterAssignFieldValues?.Invoke(item);
            return item;
        }
        private void AssignFieldValues<T>(T item) where T : IParsable
        {

            if(reader.PeekState() != CborReaderState.StartMap) return;
            reader.ReadStartMap();
            IDictionary<string, object>? itemAdditionalData = null;
            if(item is IAdditionalDataHolder holder)
            {
                holder.AdditionalData ??= new Dictionary<string, object>();
                itemAdditionalData = holder.AdditionalData;
            }
            //When targeting maccatalyst, new keyword for hiding an existing member is not being respected, returning only id and odata type
            //the below line fixes the issue
            var fieldDeserializers = (IDictionary<string, Action<IParseNode>>)item.GetType().GetMethod("GetFieldDeserializers").Invoke(item, null);

            while(reader.PeekState() != CborReaderState.EndMap)
            {
                var fieldName = reader.ReadTextString();
                if(fieldDeserializers.ContainsKey(fieldName))
                {
                    if(reader.PeekState() is CborReaderState.Null or CborReaderState.Undefined)
                    {
                        reader.ReadNull();
                        continue;// If the property is already null just continue. As calling functions like GetDouble,GetBoolValue do not process CborReaderState.Null.
                    }

                    var fieldDeserializer = fieldDeserializers[fieldName];
                    Debug.WriteLine($"found property {fieldName} to deserialize");
                    fieldDeserializer.Invoke(this);
                }
                else if(itemAdditionalData != null)
                {
                    Debug.WriteLine($"found additional property {fieldName} to deserialize");
                    IDictionaryExtensions.TryAdd(itemAdditionalData, fieldName, TryGetAnything(reader)!);
                }
                else
                {
                    Debug.WriteLine($"found additional property {fieldName} to deserialize but the model doesn't support additional data");
                }
            }
        }
        private object? TryGetAnything(CborReader reader)
        {
            switch(reader.PeekState())
            {
                case CborReaderState.DoublePrecisionFloat:
                    return reader.ReadDouble();
                case CborReaderState.SinglePrecisionFloat:
                    return reader.ReadSingle();
                case CborReaderState.HalfPrecisionFloat:
                    return reader.ReadDecimal();
                case CborReaderState.UnsignedInteger:
                    return reader.ReadUInt64();
                case CborReaderState.NegativeInteger:
                    return reader.ReadInt64();
                case CborReaderState.TextString:
                case CborReaderState.StartIndefiniteLengthTextString:
                    var value = GetStringValue();
                    if(DateTime.TryParse(value, out var dt)) return dt;
                    else if(DateTimeOffset.TryParse(value, out var dto)) return dto;
                    else if(Guid.TryParse(value, out var g)) return g;
                    else return value;
                case CborReaderState.StartArray when reader.ReadStartArray() is int itemsCount:
                    var result = new List<object?>(itemsCount);
                    for(var i = 0; i < itemsCount; i++)
                    {
                        result.Add(TryGetAnything(reader));
                    }
                    reader.ReadEndArray();
                    return result.ToArray();
                // case JsonValueKind.Object:
                //     return element;
                case CborReaderState.Boolean:
                    return reader.ReadBoolean();
                case CborReaderState.Undefined:
                case CborReaderState.Null:
                    reader.ReadNull();
                    return null;
                default:
                    throw new InvalidOperationException($"unexpected additional value type during deserialization json kind : {reader.PeekState()}");
            }
        }

        /// <summary>
        /// Get the child node of the specified identifier
        /// </summary>
        /// <param name="identifier">The identifier of the child node</param>
        /// <returns>An instance of <see cref="IParseNode"/></returns>
        public IParseNode? GetChildNode(string identifier)
        {
            if(string.IsNullOrEmpty(identifier)) throw new ArgumentNullException(nameof(identifier));
            throw new NotImplementedException();
            // if(reader.PeekState() == CborReaderState.StartMap && _jsonNode.TryGetProperty(identifier, out var jsonElement))
            // {
            //     return new CborParseNode(reader)
            //     {
            //         OnBeforeAssignFieldValues = OnBeforeAssignFieldValues,
            //         OnAfterAssignFieldValues = OnAfterAssignFieldValues
            //     };
            // }

            // return default;
        }

        private static string ToEnumRawName<T>(Type type, string value) where T : struct, Enum
        {
            if(type.GetMembers().FirstOrDefault(member =>
                   member.GetCustomAttribute<EnumMemberAttribute>() is { } attr &&
                   value.Equals(attr.Value, StringComparison.Ordinal))?.Name is { } strValue)
                return strValue;

            return value;
        }
    }
}
