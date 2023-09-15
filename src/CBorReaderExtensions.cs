// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Formats.Cbor;

namespace Microsoft.Kiota.Serialization.Cbor;
static internal class CBorReaderExtensions
{
    static internal bool TryReadDateTimeOffset(this CborReader reader, out DateTimeOffset value)
    {
        try
        {
            value = reader.ReadDateTimeOffset();
            return true;
        }
        catch
        {
            value = default;
            return false;
        }
    }
    static internal bool TryReadGuid(this CborReader reader, out Guid value)
    {
        try
        {
            var rawValue = reader.ReadByteString();
            value = new Guid(rawValue);
            return true;
        }
        catch
        {
            value = default;
            return false;
        }
    }
    static internal bool TryReadDecimal(this CborReader reader, out decimal value)
    {
        try
        {
            value = reader.ReadDecimal();
            return true;
        }
        catch
        {
            value = default;
            return false;
        }
    }

}