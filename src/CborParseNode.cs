// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Cbor;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Extensions;
using Microsoft.Kiota.Abstractions.Serialization;

[assembly: InternalsVisibleTo("Microsoft.Kiota.Serialization.Cbor.Tests")]
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
            value = LoadCborTree(reader);
        }
        internal CborParseNode(object? value)
        {
            this.value = value;
        }
        readonly internal object? value;
        private object? LoadCborTree(CborReader rdr)
        {
            switch(rdr.PeekState())
            {
                case CborReaderState.StartArray:
                    rdr.ReadStartArray();
                    var result = new List<object?>();
                    while(rdr.PeekState() != CborReaderState.EndArray)
                    {
                        result.Add(LoadCborTree(rdr));
                    }
                    rdr.ReadEndArray();
                    return result.ToArray();
                case CborReaderState.StartMap:
                    rdr.ReadStartMap();
                    var map = new Dictionary<string, object?>();
                    while(rdr.PeekState() != CborReaderState.EndMap)
                    {
                        var key = rdr.ReadTextString();
                        map.Add(key, LoadCborTree(rdr));
                    }
                    rdr.ReadEndMap();
                    return map;
                case CborReaderState.Boolean:
                    return new CborParseNode(rdr.ReadBoolean());
                case CborReaderState.DoublePrecisionFloat:
                    return new CborParseNode(rdr.ReadDouble());
                case CborReaderState.SinglePrecisionFloat:
                    return new CborParseNode(rdr.ReadSingle());
                case CborReaderState.HalfPrecisionFloat:
                    return new CborParseNode(rdr.ReadDecimal());
                case CborReaderState.UnsignedInteger:
                    return new CborParseNode(rdr.ReadUInt64());
                case CborReaderState.NegativeInteger:
                    return new CborParseNode(rdr.ReadInt64());
                case CborReaderState.TextString or CborReaderState.StartIndefiniteLengthTextString when rdr.TryReadDateTimeOffset(out var dateTimeOffset):
                    return new CborParseNode(dateTimeOffset);
                case CborReaderState.TextString or CborReaderState.StartIndefiniteLengthTextString:
                    return new CborParseNode(rdr.ReadTextString());
                case CborReaderState.ByteString when rdr.TryReadGuid(out var guid):
                    return new CborParseNode(guid);
                case CborReaderState.Undefined:
                case CborReaderState.Null:
                    rdr.ReadNull();
                    return new CborParseNode((object?)null);
                default:
                    throw new InvalidOperationException($"unexpected value type during deserialization json kind : {rdr.PeekState()}");
            }
        }

        /// <summary>
        /// Get the string value from the cbor node
        /// </summary>
        /// <returns>A string value</returns>
        public string? GetStringValue() => value as string;

        /// <summary>
        /// Get the boolean value from the cbor node
        /// </summary>
        /// <returns>A boolean value</returns>
        public bool? GetBoolValue() => value is bool boolValue ? boolValue : null;

        /// <summary>
        /// Get the byte value from the cbor node
        /// </summary>
        /// <returns>A byte value</returns>
        public byte? GetByteValue() => value is byte byteValue ? byteValue : null;

        /// <summary>
        /// Get the sbyte value from the cbor node
        /// </summary>
        /// <returns>A sbyte value</returns>
        public sbyte? GetSbyteValue() => value is sbyte sbyteValue ? sbyteValue : null;

        /// <summary>
        /// Get the int value from the cbor node
        /// </summary>
        /// <returns>A int value</returns>
        public int? GetIntValue() => value is int intValue ? intValue : null;

        /// <summary>
        /// Get the float value from the cbor node
        /// </summary>
        /// <returns>A float value</returns>
        public float? GetFloatValue() => value is float floatValue ? floatValue : null;

        /// <summary>
        /// Get the Long value from the cbor node
        /// </summary>
        /// <returns>A Long value</returns>
        public long? GetLongValue() => value is long longValue ? longValue : null;

        /// <summary>
        /// Get the double value from the cbor node
        /// </summary>
        /// <returns>A double value</returns>
        public double? GetDoubleValue() => value is double doubleValue ? doubleValue : null;

        /// <summary>
        /// Get the decimal value from the cbor node
        /// </summary>
        /// <returns>A decimal value</returns>
        public decimal? GetDecimalValue() => value is decimal decimalValue ? decimalValue : null;

        /// <summary>
        /// Get the guid value from the cbor node
        /// </summary>
        /// <returns>A guid value</returns>
        public Guid? GetGuidValue() => value is Guid guidValue ? guidValue : null;

        /// <summary>
        /// Get the <see cref="DateTimeOffset"/> value from the cbor node
        /// </summary>
        /// <returns>A <see cref="DateTimeOffset"/> value</returns>
        public DateTimeOffset? GetDateTimeOffsetValue() => value is DateTimeOffset dateTimeOffsetValue ? dateTimeOffsetValue : null;

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
            if(value is object?[] arrayValue)
            {
                return arrayValue.OfType<Dictionary<string, object>>().Select(static x => new CborParseNode(x)).Select(x => x.GetObjectValue(factory));
            }
            return Enumerable.Empty<T>();
        }
        /// <summary>
        /// Gets the collection of enum values of the node.
        /// </summary>
        /// <returns>The collection of enum values.</returns>
        public IEnumerable<T?> GetCollectionOfEnumValues<T>() where T : struct, Enum
        {
            if(value is object?[] arrayValue)
            {
                foreach(var item in arrayValue)
                {
                    if(item is CborParseNode itemNode)
                        yield return itemNode.GetEnumValue<T>();
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
            if(value is object?[] arrayValue)
            {
                var genericType = typeof(T);
                foreach(var rawItem in arrayValue)
                {
                    if(rawItem is CborParseNode item)
                    {
                        if(genericType == booleanType)
                            yield return (T)(object)item.GetBoolValue()!;
                        else if(genericType == byteType)
                            yield return (T)(object)item.GetByteValue()!;
                        else if(genericType == sbyteType)
                            yield return (T)(object)item.GetSbyteValue()!;
                        else if(genericType == stringType)
                            yield return (T)(object)item.GetStringValue()!;
                        else if(genericType == intType)
                            yield return (T)(object)item.GetIntValue()!;
                        else if(genericType == floatType)
                            yield return (T)(object)item.GetFloatValue()!;
                        else if(genericType == longType)
                            yield return (T)(object)item.GetLongValue()!;
                        else if(genericType == doubleType)
                            yield return (T)(object)item.GetDoubleValue()!;
                        else if(genericType == guidType)
                            yield return (T)(object)item.GetGuidValue()!;
                        else if(genericType == dateTimeOffsetType)
                            yield return (T)(object)item.GetDateTimeOffsetValue()!;
                        else if(genericType == timeSpanType)
                            yield return (T)(object)item.GetTimeSpanValue()!;
                        else if(genericType == dateType)
                            yield return (T)(object)item.GetDateValue()!;
                        else if(genericType == timeType)
                            yield return (T)(object)item.GetTimeValue()!;
                        else
                            throw new InvalidOperationException($"unknown type for deserialization {genericType.FullName}");

                    }
                    else
                    {
                        if(genericType == booleanType)
                            yield return (T)(object)GetBoolValue()!;
                        else if(genericType == byteType)
                            yield return (T)(object)GetByteValue()!;
                        else if(genericType == sbyteType)
                            yield return (T)(object)GetSbyteValue()!;
                        else if(genericType == stringType)
                            yield return (T)(object)GetStringValue()!;
                        else if(genericType == intType)
                            yield return (T)(object)GetIntValue()!;
                        else if(genericType == floatType)
                            yield return (T)(object)GetFloatValue()!;
                        else if(genericType == longType)
                            yield return (T)(object)GetLongValue()!;
                        else if(genericType == doubleType)
                            yield return (T)(object)GetDoubleValue()!;
                        else if(genericType == guidType)
                            yield return (T)(object)GetGuidValue()!;
                        else if(genericType == dateTimeOffsetType)
                            yield return (T)(object)GetDateTimeOffsetValue()!;
                        else if(genericType == timeSpanType)
                            yield return (T)(object)GetTimeSpanValue()!;
                        else if(genericType == dateType)
                            yield return (T)(object)GetDateValue()!;
                        else if(genericType == timeType)
                            yield return (T)(object)GetTimeValue()!;
                        else
                            throw new InvalidOperationException($"unknown type for deserialization {genericType.FullName}");
                    }
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

            if(value is not Dictionary<string, object?> dictionaryValue) return;
            IDictionary<string, object>? itemAdditionalData = null;
            if(item is IAdditionalDataHolder holder)
            {
                holder.AdditionalData ??= new Dictionary<string, object>();
                itemAdditionalData = holder.AdditionalData;
            }
            //When targeting maccatalyst, new keyword for hiding an existing member is not being respected, returning only id and odata type
            //the below line fixes the issue
            var fieldDeserializers = (IDictionary<string, Action<IParseNode>>)item.GetType().GetMethod("GetFieldDeserializers").Invoke(item, null);

            foreach(var entry in dictionaryValue)
            {
                var fieldName = entry.Key;
                if(entry.Value is null)
                    continue;// If the property is already null just continue. As calling functions like GetDouble,GetBoolValue do not process CborReaderState.Null.
                if(fieldDeserializers.ContainsKey(fieldName) && entry.Value is CborParseNode valueParseNode)
                {
                    var fieldDeserializer = fieldDeserializers[fieldName];
                    Debug.WriteLine($"found property {fieldName} to deserialize");
                    fieldDeserializer.Invoke(valueParseNode);
                }
                else if(itemAdditionalData != null)
                {
                    Debug.WriteLine($"found additional property {fieldName} to deserialize");
                    IDictionaryExtensions.TryAdd(itemAdditionalData, fieldName, TryGetAnything(entry.Value)!);
                }
                else
                {
                    Debug.WriteLine($"found additional property {fieldName} to deserialize but the model doesn't support additional data");
                }
            }
        }
        private static object? TryGetAnything(object? val) => val switch
        {
            double d => d,
            float f => f,
            short s => s,
            decimal de => de,
            ulong ui64 => ui64,
            uint ui32 => ui32,
            ushort ui16 => ui16,
            int i => i,
            long l => l,
            byte b => b,
            string str when DateTime.TryParse(str, out var dt) => dt,
            string str when DateTimeOffset.TryParse(str, out var dto) => dto,
            string str when Guid.TryParse(str, out var g) => g,
            string str => str,
            int[] intArray => intArray,
            string[] strArray => strArray,
            object?[] arrayValue => arrayValue.Select(TryGetAnything).ToArray(),
            List<object?> listValue => listValue.Select(TryGetAnything).ToArray(),
            Dictionary<string, object?> dictionaryValue => dictionaryValue,
            CborParseNode node => TryGetAnything(node.value),
            // case JsonValueKind.Object:
            //     return element;
            bool b => b,
            null => null,
            _ => throw new InvalidOperationException($"unexpected additional value type during deserialization json kind : {val.GetType()}"),
        };

        /// <summary>
        /// Get the child node of the specified identifier
        /// </summary>
        /// <param name="identifier">The identifier of the child node</param>
        /// <returns>An instance of <see cref="IParseNode"/></returns>
        public IParseNode? GetChildNode(string identifier)
        {
            if(string.IsNullOrEmpty(identifier)) throw new ArgumentNullException(nameof(identifier));
            if(value is Dictionary<string, object?> dictionary && dictionary.TryGetValue(identifier, out var childValue))
            {
                return new CborParseNode(childValue)
                {
                    OnBeforeAssignFieldValues = OnBeforeAssignFieldValues,
                    OnAfterAssignFieldValues = OnAfterAssignFieldValues
                };
            }

            return default;
        }

        private static string ToEnumRawName<T>(Type type, string value) where T : struct, Enum
        {
            if(type.GetMembers().FirstOrDefault(member =>
                   member.GetCustomAttribute<EnumMemberAttribute>() is { } attr &&
                   value.Equals(attr.Value, StringComparison.Ordinal))?.Name is { } strValue)
                return strValue;

            return value;
        }

        internal void WriteTo(CborSerializationWriter writer)
        {
            if(writer is null) throw new ArgumentNullException(nameof(writer));
            var value = TryGetAnything(this.value);
            writer.WriteAnyValue(string.Empty, value);
        }
    }
}
