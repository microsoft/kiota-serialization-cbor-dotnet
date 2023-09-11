using System;
using Xunit;

namespace Microsoft.Kiota.Serialization.Cbor.Tests
{
    public class CborSerializationWriterFactoryTests
    {
        private readonly CborSerializationWriterFactory _jsonSerializationFactory;

        public CborSerializationWriterFactoryTests()
        {
            _jsonSerializationFactory = new CborSerializationWriterFactory();
        }

        [Fact]
        public void GetsWriterForCborContentType()
        {
            var jsonWriter = _jsonSerializationFactory.GetSerializationWriter(_jsonSerializationFactory.ValidContentType);
            
            // Assert
            Assert.NotNull(jsonWriter);
            Assert.IsAssignableFrom<CborSerializationWriter>(jsonWriter);
        }

        [Fact]
        public void ThrowsArgumentOutOfRangeExceptionForInvalidContentType()
        {
            var streamContentType = "application/octet-stream";
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => _jsonSerializationFactory.GetSerializationWriter(streamContentType));

            // Assert
            Assert.NotNull(exception);
            Assert.Equal($"expected a {_jsonSerializationFactory.ValidContentType} content type", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ThrowsArgumentNullExceptionForNoContentType(string contentType)
        {
            var exception = Assert.Throws<ArgumentNullException>(() => _jsonSerializationFactory.GetSerializationWriter(contentType));

            // Assert
            Assert.NotNull(exception);
            Assert.Equal("contentType", exception.ParamName);
        }
    }
}
