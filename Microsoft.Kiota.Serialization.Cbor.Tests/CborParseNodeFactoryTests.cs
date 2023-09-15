using System;
using System.IO;
using Xunit;

namespace Microsoft.Kiota.Serialization.Cbor.Tests
{
    public class CborParseNodeFactoryTests
    {
        private readonly CborParseNodeFactory _cborParseNodeFactory = new();
        [Fact]
        public void GetsWriterForCborContentType()
        {
            var data = TestDataHelper.GetCBorData("TestCborString");
            using var cborStream = new MemoryStream(data);
            var cborWriter = _cborParseNodeFactory.GetRootParseNode(_cborParseNodeFactory.ValidContentType, cborStream);

            // Assert
            Assert.NotNull(cborWriter);
            Assert.IsAssignableFrom<CborParseNode>(cborWriter);
        }

        [Fact]
        public void ThrowsArgumentOutOfRangeExceptionForInvalidContentType()
        {
            var streamContentType = "application/octet-stream";
            var data = TestDataHelper.GetCBorData("TestCborString");
            using var cborStream = new MemoryStream(data);
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => _cborParseNodeFactory.GetRootParseNode(streamContentType, cborStream));

            // Assert
            Assert.NotNull(exception);
            Assert.Equal($"expected a {_cborParseNodeFactory.ValidContentType} content type", exception.ParamName, StringComparer.Ordinal);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ThrowsArgumentNullExceptionForNoContentType(string contentType)
        {
            var data = TestDataHelper.GetCBorData("TestCborString");
            using var cborStream = new MemoryStream(data);
            var exception = Assert.Throws<ArgumentNullException>(() => _cborParseNodeFactory.GetRootParseNode(contentType, cborStream));

            // Assert
            Assert.NotNull(exception);
            Assert.Equal("contentType", exception.ParamName, StringComparer.Ordinal);
        }
    }
}
