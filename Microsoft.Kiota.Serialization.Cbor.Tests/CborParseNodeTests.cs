using System;
using System.Formats.Cbor;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Serialization.Cbor.Tests.Mocks;
using Xunit;

namespace Microsoft.Kiota.Serialization.Cbor.Tests
{
    public class CborParseNodeTests
    {
        private static byte[] GetCBorData(string fileName)
        {
            var stringHexValue = File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(CborParseNodeTests)).Location), "TestData", fileName)); //async method not available for net462
            var result = new byte[stringHexValue.Length / 2];
            for(var i = 0; i < stringHexValue.Length; i += 2)
            {
                result[i / 2] = Convert.ToByte(stringHexValue.Substring(i, 2), 16);
            }
            return result;
        }
        [Fact]
        public void GetsEntityValueFromCbor()
        {
            // Arrange
            var data = GetCBorData("TestUserCbor.hex");
            var reader = new CborReader(data);
            var cborParseNode = new CborParseNode(reader);
            // Act
            var testEntity = cborParseNode.GetObjectValue<TestEntity>(x => new TestEntity());
            // Assert
            Assert.NotNull(testEntity);
            Assert.Null(testEntity.OfficeLocation);
            Assert.NotEmpty(testEntity.AdditionalData);
            Assert.True(testEntity.AdditionalData.ContainsKey("jobTitle"));
            Assert.True(testEntity.AdditionalData.ContainsKey("mobilePhone"));
            Assert.Equal("Auditor", testEntity.AdditionalData["jobTitle"]);
            Assert.Equal("48d31887-5fad-4d73-a9f5-3c356e68a038", testEntity.Id);
            Assert.Equal(TestEnum.One | TestEnum.Two, testEntity.Numbers); // Unknown enum value is not included
            Assert.Equal(TestNamingEnum.Item2SubItem1, testEntity.TestNamingEnum); // correct value is chosen
            Assert.Equal(TimeSpan.FromHours(1), testEntity.WorkDuration); // Parses timespan values
            Assert.Equal(new Time(8, 0, 0).ToString(), testEntity.StartWorkTime.ToString());// Parses time values
            Assert.Equal(new Time(17, 0, 0).ToString(), testEntity.EndWorkTime.ToString());// Parses time values
            Assert.Equal(new Date(2017, 9, 4).ToString(), testEntity.BirthDay.ToString());// Parses date values
        }

        [Fact]
        public void GetCollectionOfObjectValuesFromCbor()
        {
            // Arrange
            var data = GetCBorData("TestUserCollectionCbor.hex");
            var reader = new CborReader(data);
            var cborParseNode = new CborParseNode(reader);
            // Act
            var testEntityCollection = cborParseNode.GetCollectionOfObjectValues(static x => new TestEntity()).ToArray();
            // Assert
            Assert.NotEmpty(testEntityCollection);
            Assert.Equal("48d31887-5fad-4d73-a9f5-3c356e68a038", testEntityCollection[0].Id);
        }

        [Fact]
        public void GetsChildNodeAndGetCollectionOfPrimitiveValuesFromCborParseNode()
        {
            // Arrange
            var data = GetCBorData("TestUserCollectionCbor.hex");
            var reader = new CborReader(data);
            var rootParseNode = new CborParseNode(reader);
            // Act to get business phones list
            var phonesListChildNode = rootParseNode.GetChildNode("businessPhones");
            var phonesList = phonesListChildNode.GetCollectionOfPrimitiveValues<string>().ToArray();
            // Assert
            Assert.NotEmpty(phonesList);
            Assert.Equal("+1 412 555 0109", phonesList[0]);
        }

        [Fact]
        public void ReturnsDefaultIfChildNodeDoesNotExist()
        {
            // Arrange
            var data = GetCBorData("TestUserCollectionCbor.hex");
            var reader = new CborReader(data);
            var rootParseNode = new CborParseNode(reader);
            // Try to get an imaginary node value
            var imaginaryNode = rootParseNode.GetChildNode("imaginaryNode");
            // Assert
            Assert.Null(imaginaryNode);
        }
    }
}
