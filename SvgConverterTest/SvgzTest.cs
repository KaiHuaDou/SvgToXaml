using System.IO;
using System.IO.Compression;
using NUnit.Framework;

namespace SvgConverterTest;

public class SvgzTest
{
    [Test]
    public void TestUnzip( )
    {
        FileStream fs = File.OpenRead(@".\TestFiles\example.svgz");
        GZipStream stream = new(fs, CompressionMode.Decompress);
        FileStream destination = File.OpenWrite(@".\TestFiles\example.svg");
        stream.CopyTo(destination);
    }
}
