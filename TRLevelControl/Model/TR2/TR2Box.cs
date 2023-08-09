using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR2Box : ISerializableCompact
{
    public byte ZMin { get; set; }

    public byte ZMax { get; set; }

    public byte XMin { get; set; }

    public byte XMax { get; set; }

    public short TrueFloor { get; set; }

    public short OverlapIndex { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new(base.ToString());

        sb.Append(" ZMin: " + ZMin);
        sb.Append(" ZMax: " + ZMax);
        sb.Append(" XMin: " + XMin);
        sb.Append(" XMax: " + XMax);
        sb.Append(" TrueFloor: " + TrueFloor);
        sb.Append(" OverlapIndex: " + OverlapIndex);

        return sb.ToString();
    }

    public byte[] Serialize()
    {
        using (MemoryStream stream = new())
        {
            using (BinaryWriter writer = new(stream))
            {
                writer.Write(ZMin);
                writer.Write(ZMax);
                writer.Write(XMin);
                writer.Write(XMax);
                writer.Write(TrueFloor);
                writer.Write(OverlapIndex);
            }

            return stream.ToArray();
        }
    }
}
