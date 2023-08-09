using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR3CDAudioEntry : ISerializableCompact
{
    public string Name { get; set; }

    public uint WavLength { get; set; }

    public uint WavOffset { get; set; }

    public byte[] Serialize()
    {
        using (MemoryStream stream = new())
        {
            using (BinaryWriter writer = new(stream))
            {
                writer.Write(Name);
                writer.Write(WavLength);
                writer.Write(WavOffset);
            }

            return stream.ToArray();
        }
    }
}
