using System;
using Xunit;

namespace Microsoft.Kiota.Serialization.Cbor.Tests
{
    public class CborSerializationWriterFactoryTests
    {
        private readonly CborSerializationWriterFactory _cborSerializationFactory;

        public CborSerializationWriterFactoryTests()
        {
            _cborSerializationFactory = new CborSerializationWriterFactory();
        }

        [Fact]
        public void GetsWriterForCborContentType()
        {
            var cborWriter = _cborSerializationFactory.GetSerializationWriter(_cborSerializationFactory.ValidContentType);

            // Assert
            Assert.NotNull(cborWriter);
            Assert.IsAssignableFrom<CborSerializationWriter>(cborWriter);
        }

        [Fact]
        public void ThrowsArgumentOutOfRangeExceptionForInvalidContentType()
        {
            var streamContentType = "application/octet-stream";
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => _cborSerializationFactory.GetSerializationWriter(streamContentType));

            // Assert
            Assert.NotNull(exception);
            Assert.Equal($"expected a {_cborSerializationFactory.ValidContentType} content type", exception.ParamName, StringComparer.Ordinal);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ThrowsArgumentNullExceptionForNoContentType(string? contentType)
        {
#nullable disable
            var exception = Assert.Throws<ArgumentNullException>(() => _cborSerializationFactory.GetSerializationWriter(contentType));
#nullable restore
            
            // Assert
            Assert.NotNull(exception);
            Assert.Equal("contentType", exception.ParamName, StringComparer.Ordinal);
        }
    }
}
