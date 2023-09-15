using System.Reflection;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.Globalization;

namespace Microsoft.Kiota.Serialization.Cbor.Tests;

internal static class TestDataHelper
{
    static internal MemoryStream GetCBorDataAsStream(string fileName) => new MemoryStream(GetCBorData(fileName));
    static internal byte[] GetCBorData(string fileName)
    {
        var stringHexValue = GetCborHex(fileName);
        var result = new byte[stringHexValue.Length / 2];
        for(var i = 0; i < stringHexValue.Length; i += 2)
        {
            result[i / 2] = Convert.ToByte(stringHexValue.Substring(i, 2), 16);
        }
        return result;
    }
    static internal string GetCborHex(string fileName)
    {
        using var ms = new MemoryStream();
        var typeReference = typeof(TestDataHelper);
        if(typeReference.GetTypeInfo().Assembly.GetManifestResourceStream($"{typeReference.Namespace}.TestData.{fileName}.hex") is { } stream)
            stream.CopyTo(ms);
        else throw new InvalidOperationException($"Could not find the resource {fileName}.hex in the assembly {typeReference.GetTypeInfo().Assembly.FullName}");
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    static internal async Task<string> GetHexRepresentationFromStream(Stream stream, CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, 4096, cancellationToken).ConfigureAwait(false);
        var sb = new StringBuilder();
        foreach(var b in ms.ToArray())
        {
            sb.Append(b.ToString("X2", CultureInfo.InvariantCulture));
        }
        return sb.ToString();
    }
}