using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelControl.Compression;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR4Sample : ISerializableCompact
{
    public uint UncompSize { get; set; }

    public uint CompSize { get; set; }

    public byte[] SoundData { get; set; }

    //Optional - mainly just for testing, this is just to store the raw zlib compressed chunk.
    public byte[] CompressedChunk { get; set; }

    public byte[] Serialize()
    {
        //we cheat a bit here - sample is not actually zlib compressed, it is simply a WAV file.
        //So in the TR4Level file we will write the sizes and compressed chunk straight.
        throw new NotImplementedException();
    }
}
