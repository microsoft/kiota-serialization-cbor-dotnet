using System.Reflection;
using System.IO;
using System;

namespace Microsoft.Kiota.Serialization.Cbor.Tests;

public static class TestDataHelper
{
    public static byte[] GetCBorData(string fileName)
    {
        var stringHexValue = File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(CborParseNodeTests)).Location), "TestData", fileName)); //async method not available for net462
        var result = new byte[stringHexValue.Length / 2];
        for(var i = 0; i < stringHexValue.Length; i += 2)
        {
            result[i / 2] = Convert.ToByte(stringHexValue.Substring(i, 2), 16);
        }
        return result;
    }
}