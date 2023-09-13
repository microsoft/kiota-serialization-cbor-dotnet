using System;
using System.IO;
using System.Text;
using Xunit;

namespace Microsoft.Kiota.Serialization.Cbor.Tests
{
    public class CborParseNodeFactoryTests
    {
        private readonly CborParseNodeFactory _cborParseNodeFactory;
        private const string TestCborString = "{\"key\":\"value\"}";

        public CborParseNodeFactoryTests()
        {
            _cborParseNodeFactory = new CborParseNodeFactory();
        }

        [Fact]
        public void GetsWriterForCborContentType()
        {
            using var cborStream = new MemoryStream(Encoding.UTF8.GetBytes(TestCborString));
            var cborWriter = _cborParseNodeFactory.GetRootParseNode(_cborParseNodeFactory.ValidContentType,cborStream);

            // Assert
            Assert.NotNull(cborWriter);
            Assert.IsAssignableFrom<CborParseNode>(cborWriter);
        }

        [Fact]
        public void ThrowsArgumentOutOfRangeExceptionForInvalidContentType()
        {
            var streamContentType = "application/octet-stream";
            using var cborStream = new MemoryStream(Encoding.UTF8.GetBytes(TestCborString));
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => _cborParseNodeFactory.GetRootParseNode(streamContentType,cborStream));

            // Assert
            Assert.NotNull(exception);
            Assert.Equal($"expected a {_cborParseNodeFactory.ValidContentType} content type", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ThrowsArgumentNullExceptionForNoContentType(string contentType)
        {
            using var cborStream = new MemoryStream(Encoding.UTF8.GetBytes(TestCborString));
            var exception = Assert.Throws<ArgumentNullException>(() => _cborParseNodeFactory.GetRootParseNode(contentType,cborStream));

            // Assert
            Assert.NotNull(exception);
            Assert.Equal("contentType", exception.ParamName);
        }
    }
}
