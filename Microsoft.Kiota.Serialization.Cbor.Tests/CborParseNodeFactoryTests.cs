using System;
using System.IO;
using System.Text;
using Xunit;

namespace Microsoft.Kiota.Serialization.Cbor.Tests
{
    public class CborParseNodeFactoryTests
    {
        private readonly CborParseNodeFactory _jsonParseNodeFactory;
        private const string TestCborString = "{\"key\":\"value\"}";

        public CborParseNodeFactoryTests()
        {
            _jsonParseNodeFactory = new CborParseNodeFactory();
        }

        [Fact]
        public void GetsWriterForCborContentType()
        {
            using var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(TestCborString));
            var jsonWriter = _jsonParseNodeFactory.GetRootParseNode(_jsonParseNodeFactory.ValidContentType,jsonStream);

            // Assert
            Assert.NotNull(jsonWriter);
            Assert.IsAssignableFrom<CborParseNode>(jsonWriter);
        }

        [Fact]
        public void ThrowsArgumentOutOfRangeExceptionForInvalidContentType()
        {
            var streamContentType = "application/octet-stream";
            using var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(TestCborString));
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => _jsonParseNodeFactory.GetRootParseNode(streamContentType,jsonStream));

            // Assert
            Assert.NotNull(exception);
            Assert.Equal($"expected a {_jsonParseNodeFactory.ValidContentType} content type", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ThrowsArgumentNullExceptionForNoContentType(string contentType)
        {
            using var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(TestCborString));
            var exception = Assert.Throws<ArgumentNullException>(() => _jsonParseNodeFactory.GetRootParseNode(contentType,jsonStream));

            // Assert
            Assert.NotNull(exception);
            Assert.Equal("contentType", exception.ParamName);
        }
    }
}
