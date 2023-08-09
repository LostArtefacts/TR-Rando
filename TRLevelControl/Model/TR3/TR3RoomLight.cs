using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR3RoomLight : ISerializableCompact
{
    public int X { get; set; }

    public int Y { get; set; }

    public int Z { get; set; }

    public TRColour Colour { get; set; }

    public byte LightType { get; set; }

    //8 bytes, depends on LightType (sun or spot)
    public short[] LightProperties { get; set; }

    public byte[] Serialize()
    {
        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(X);
                writer.Write(Y);
                writer.Write(Z);
                writer.Write(Colour.Serialize());
                writer.Write(LightType);

                foreach (ushort property in LightProperties)
                {
                    writer.Write(property);
                }
            }

            return stream.ToArray();
        }
    }
}
