﻿using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

//ref: https://benfoster.io/blog/zlib-compression-net-core/

//Looking at a .TR4 file in HxD, the zlib header is 78 9C which means default compression.

//Level | ZLIB  | GZIP
//  1   | 78 01 | 1F 8B - No/Low
//  2   | 78 5E | 1F 8B
//  3   | 78 5E | 1F 8B
//  4   | 78 5E | 1F 8B
//  5   | 78 5E | 1F 8B
//  6   | 78 9C | 1F 8B - Default
//  7   | 78 DA | 1F 8B - Best
//  8   | 78 DA | 1F 8B
//  9   | 78 DA | 1F 8B

namespace TRLevelControl.Compression;

public static class TRZlib
{
    public static byte[] Decompress(byte[] data)
    {
        using MemoryStream outputStream = new();
        using MemoryStream compressedStream = new(data);
        using InflaterInputStream inputStream = new(compressedStream);
        inputStream.CopyTo(outputStream);
        return outputStream.ToArray();
    }

    public static byte[] Compress(byte[] data)
    {
        using MemoryStream outMemoryStream = new();
        using DeflaterOutputStream outZStream = new(outMemoryStream);
        using Stream inMemoryStream = new MemoryStream(data);
        inMemoryStream.CopyTo(outZStream);
        outZStream.Finish();
        return outMemoryStream.ToArray();
    }
}
