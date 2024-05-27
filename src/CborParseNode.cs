// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Cbor;
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
            value = LoadCborTree(reader) is CborParseNode node ? node.value : throw new InvalidOperationException($"unexpected value type during deserialization cbor kind : {reader.PeekState()}");
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
                    return AssignEventValues(new CborParseNode(result.ToArray()));
                case CborReaderState.StartMap:
                    rdr.ReadStartMap();
                    var map = new Dictionary<string, object?>();
                    while(rdr.PeekState() != CborReaderState.EndMap)
                    {
                        var key = rdr.ReadTextString();
                        map.Add(key, LoadCborTree(rdr));
                    }
                    rdr.ReadEndMap();
                    return AssignEventValues(new CborParseNode(map));
                case CborReaderState.Boolean:
                    return AssignEventValues(new CborParseNode(rdr.ReadBoolean()));
                case CborReaderState.DoublePrecisionFloat:
                    return AssignEventValues(new CborParseNode(rdr.ReadDouble()));
                case CborReaderState.SinglePrecisionFloat:
                    return AssignEventValues(new CborParseNode(rdr.ReadSingle()));
                case CborReaderState.HalfPrecisionFloat:
                    return AssignEventValues(new CborParseNode(rdr.ReadDouble()));
                case CborReaderState.UnsignedInteger:
                    return AssignEventValues(new CborParseNode(rdr.ReadUInt64()));
                case CborReaderState.NegativeInteger:
                    return AssignEventValues(new CborParseNode(rdr.ReadInt64()));
                case CborReaderState.TextString or CborReaderState.StartIndefiniteLengthTextString:
                    return AssignEventValues(new CborParseNode(rdr.ReadTextString()));
                case CborReaderState.ByteString when rdr.TryReadGuid(out var guid):
                    return AssignEventValues(new CborParseNode(guid));
                case CborReaderState.Tag when rdr.TryReadDateTimeOffset(out var dateTimeOffset):
                    return AssignEventValues(new CborParseNode(dateTimeOffset));
                case CborReaderState.Tag when rdr.TryReadDecimal(out var decimalValue):
                    return AssignEventValues(new CborParseNode(decimalValue));
                case CborReaderState.Undefined:
                case CborReaderState.Null:
                    rdr.ReadNull();
                    return AssignEventValues(new CborParseNode((object?)null));
                default:
                    throw new InvalidOperationException($"unexpected value type during deserialization cbor kind : {rdr.PeekState()}");
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
        public sbyte? GetSbyteValue() => GetIntValue() is int sbyteValue ? Convert.ToSByte(sbyteValue) : null;

        /// <summary>
        /// Get the int value from the cbor node
        /// </summary>
        /// <returns>A int value</returns>
        public int? GetIntValue() => value switch
        {
            int intValue => intValue,
            ulong intValue => Convert.ToInt32(intValue),
            long intValue => Convert.ToInt32(intValue),
            _ => null
        };

        /// <summary>
        /// Get the float value from the cbor node
        /// </summary>
        /// <returns>A float value</returns>
        public float? GetFloatValue() => value is float floatValue ? floatValue : null;

        /// <summary>
        /// Get the Long value from the cbor node
        /// </summary>
        /// <returns>A Long value</returns>
        public long? GetLongValue() => value switch
        {
            long longValue => longValue,
            ulong longValue => Convert.ToInt64(longValue),
            _ => null,
        };

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
            if(type.IsDefined(typeof(FlagsAttribute)))
            {
                string[] parts = rawValue.Split(',');
                int sum = 0;
                foreach(var part in parts)
                {
                    if(Enum.TryParse<T>(part, true, out var result))
                    {
                        sum += (int)(object)result;
                    }
                }
                return (T)(object)sum;
            }
            else
            {
                return Enum.TryParse<T>(rawValue, true, out var result) ? result : null;
            }
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
                foreach(var item in arrayValue)
                {
                    if(item is CborParseNode node)
                    {
                        yield return node.GetObjectValue(factory);
                    }
                }
            }
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

        private static T GetItemValue<T>(Type genericType, CborParseNode item)
        {
            if(genericType == booleanType)
                return (T)(object)item.GetBoolValue()!;
            else if(genericType == byteType)
                return (T)(object)item.GetByteValue()!;
            else if(genericType == sbyteType)
                return (T)(object)item.GetSbyteValue()!;
            else if(genericType == stringType)
                return (T)(object)item.GetStringValue()!;
            else if(genericType == intType)
                return (T)(object)item.GetIntValue()!;
            else if(genericType == floatType)
                return (T)(object)item.GetFloatValue()!;
            else if(genericType == longType)
                return (T)(object)item.GetLongValue()!;
            else if(genericType == doubleType)
                return (T)(object)item.GetDoubleValue()!;
            else if(genericType == guidType)
                return (T)(object)item.GetGuidValue()!;
            else if(genericType == dateTimeOffsetType)
                return (T)(object)item.GetDateTimeOffsetValue()!;
            else if(genericType == timeSpanType)
                return (T)(object)item.GetTimeSpanValue()!;
            else if(genericType == dateType)
                return (T)(object)item.GetDateValue()!;
            else if(genericType == timeType)
                return (T)(object)item.GetTimeValue()!;
            else
                throw new InvalidOperationException($"unknown type for deserialization {genericType.FullName}");
        }

        /// <summary>
        /// Get the collection of primitives of type <typeparam name="T"/>from the cbor node
        /// </summary>
        /// <returns>A collection of objects</returns>
        public IEnumerable<T> GetCollectionOfPrimitiveValues<T>()
        {
            var genericType = typeof(T);
            if(value is object?[] arrayValue1)
                return GetValuesFromArray<T>(genericType, arrayValue1);
            else if(value is CborParseNode { value: object?[] arrayValue2 })
                return GetValuesFromArray<T>(genericType, arrayValue2);
            else
                return [];
        
            IEnumerable<TT> GetValuesFromArray<TT>(Type genericType, object?[] array)
            {
                foreach (var item in array)
                {
                    if (item is CborParseNode node)
                    {
                        yield return GetItemValue<TT>(genericType, node);
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
                if(entry.Value is null) continue;

                // If the property is already null just continue. As calling functions like GetDouble,GetBoolValue do not process CborReaderState.Null.
                var fieldName = entry.Key;
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
            object?[] arrayValue => ProcessArray(arrayValue),
            List<object?> listValue => ProcessList(listValue),
            Dictionary<string, object?> dictionaryValue => dictionaryValue,
            CborParseNode node => TryGetAnything(node.value),
            bool b => b,
            null => null,
            _ => throw new InvalidOperationException($"unexpected additional value type during deserialization json kind : {val.GetType()}"),
        };

        private static object?[] ProcessArray(object?[] arrayValue)
        {
            var newArray = new object?[arrayValue.Length];
            for(int index = 0; index < arrayValue.Length; index++)
            {
                newArray[index] = TryGetAnything(arrayValue[index]);
            }
            return newArray;
        }

        private static object?[] ProcessList(List<object?> listValue)
        {
            var newArray = new object?[listValue.Count];
            for(int index = 0; index < listValue.Count; index++)
            {
                newArray[index] = TryGetAnything(listValue[index]);
            }
            return newArray;
        }

        /// <summary>
        /// Get the child node of the specified identifier
        /// </summary>
        /// <param name="identifier">The identifier of the child node</param>
        /// <returns>An instance of <see cref="IParseNode"/></returns>
        public IParseNode? GetChildNode(string identifier)
        {
            if (string.IsNullOrEmpty(identifier)) 
                throw new ArgumentNullException(nameof(identifier));
                
            if (value is Dictionary<string, object?> dictionary)
            {
                object? childValue;
                if (dictionary.TryGetValue(identifier, out childValue))
                {
                    if (childValue is CborParseNode childNode)
                        return childNode;
                        
                    return AssignEventValues(new CborParseNode(childValue));
                }
            }

            return default;
        }

        private CborParseNode AssignEventValues(CborParseNode node)
        {
            node.OnBeforeAssignFieldValues = OnBeforeAssignFieldValues;
            node.OnAfterAssignFieldValues = OnAfterAssignFieldValues;
            return node;
        }

        private static string ToEnumRawName<T>(Type type, string value) where T : struct, Enum
        {
            foreach(var member in type.GetMembers())
            {
                var attr = member.GetCustomAttribute<EnumMemberAttribute>();
                if(attr != null && value.Equals(attr.Value, StringComparison.Ordinal))
                {
                    return member.Name;
                }
            }
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
