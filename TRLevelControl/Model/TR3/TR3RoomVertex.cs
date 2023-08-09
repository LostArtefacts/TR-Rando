using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR3RoomVertex : ISerializableCompact
{
    public TRVertex Vertex { get; set; }

    public short Lighting { get; set; }

    public ushort Attributes { get; set; }

    public ushort Colour { get; set; }

    public bool UseWaveMovement
    {
        get => (Attributes & 0x2000) > 0;
        set
        {
            if (value)
            {
                Attributes |= 0x2000;
            }
            else
            {
                Attributes = (ushort)(Attributes & ~0x2000);
            }
        }
    }

    public bool UseCaustics
    {
        get => (Attributes & 0x4000) > 0;
        set
        {
            if (value)
            {
                Attributes |= 0x4000;
            }
            else
            {
                Attributes = (ushort)(Attributes & ~0x4000);
            }
        }
    }

    public byte[] Serialize()
    {
        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(Vertex.Serialize());
                writer.Write(Lighting);
                writer.Write(Attributes);
                writer.Write(Colour);
            }

            return stream.ToArray();
        }
    }
}
