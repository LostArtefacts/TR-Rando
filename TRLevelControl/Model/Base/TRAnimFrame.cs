using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TRAnimFrame : ISerializableCompact
{
    public TRBoundingBox Box { get; set; }

    public short OffsetX { get; set; }

    public short OffsetY { get; set; }

    public short OffsetZ { get; set; }

    public short NumValues { get; set; }

    public ushort[] AngleSets { get; set; }

    public byte[] Serialize()
    {
        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(Box.Serialize());
                writer.Write(OffsetX);
                writer.Write(OffsetY);
                writer.Write(OffsetZ);
                writer.Write(NumValues);

                foreach (ushort val in AngleSets)
                {
                    writer.Write(val);
                }
            }

            return stream.ToArray();
        }
    }
}
