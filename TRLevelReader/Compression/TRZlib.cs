using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//ref: https://benfoster.io/blog/zlib-compression-net-core/

namespace TRLevelReader.Compression
{
    public static class TRZlib
    {
        public static byte[] Decompress(byte[] data)
        {
            var outputStream = new MemoryStream();
            using (var compressedStream = new MemoryStream(data))
            using (var inputStream = new InflaterInputStream(compressedStream))
            {
                inputStream.CopyTo(outputStream);
                return outputStream.ToArray();
            }
        }

        public static byte[] Compress(byte[] data)
        {
            var outputStream = new MemoryStream();
            using (var uncompressedStream = new MemoryStream(data))
            using (var inputStream = new DeflaterOutputStream(uncompressedStream))
            {
                inputStream.CopyTo(outputStream);
                return outputStream.ToArray();
            }
        }
    }
}
