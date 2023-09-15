using System.Reflection;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Text;

namespace Microsoft.Kiota.Serialization.Cbor.Tests;

public static class TestDataHelper
{
    public static MemoryStream GetCBorDataAsStream(string fileName) => new MemoryStream(GetCBorData(fileName));
    public static byte[] GetCBorData(string fileName)
    {
        var stringHexValue = GetCborHex(fileName);
        var result = new byte[stringHexValue.Length / 2];
        for(var i = 0; i < stringHexValue.Length; i += 2)
        {
            result[i / 2] = Convert.ToByte(stringHexValue.Substring(i, 2), 16);
        }
        return result;
    }
    public static string GetCborHex(string fileName)
    {
        using var ms = new MemoryStream();
        var typeReference = typeof(TestDataHelper);
        typeReference.GetTypeInfo().Assembly.GetManifestResourceStream($"{typeReference.Namespace}.TestData.{fileName}.hex").CopyTo(ms);
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    public static async Task<string> GetHexRepresentationFromStream(Stream stream, CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, 4096, cancellationToken).ConfigureAwait(false);
        var sb = new StringBuilder();
        foreach(var b in ms.ToArray())
        {
            sb.Append(b.ToString("X2"));
        }
        return sb.ToString();
    }
}