// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Data.SqlTypes;
using System.IO;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Microsoft.Kiota.Serialization.Cbor
{
    /// <summary>
    /// The <see cref="IParseNodeFactory"/> implementation for CBOR content types
    /// </summary>
    public class CborParseNodeFactory : IParseNodeFactory
    {
        /// <summary>
        /// The valid content type for json
        /// </summary>
        public string ValidContentType { get; } = "application/cbor";

        /// <summary>
        /// Gets the root <see cref="IParseNode"/> of the json to be read.
        /// </summary>
        /// <param name="contentType">The content type of the stream to be parsed</param>
        /// <param name="content">The <see cref="Stream"/> containing json to parse.</param>
        /// <returns>An instance of <see cref="IParseNode"/> for json manipulation</returns>
        public IParseNode GetRootParseNode(string contentType, Stream content)
        {
            if(string.IsNullOrEmpty(contentType))
                throw new ArgumentNullException(nameof(contentType));
            else if(!ValidContentType.Equals(contentType, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentOutOfRangeException($"expected a {ValidContentType} content type");

            _ = content ?? throw new ArgumentNullException(nameof(content));

            if (content is not MemoryStream ms)
            {
                ms = new MemoryStream();
                content.CopyTo(ms);
            }
            var bytes = new ReadOnlyMemory<byte>(ms.ToArray());
            return new CborParseNode(new System.Formats.Cbor.CborReader(bytes));
        }
    }
}
