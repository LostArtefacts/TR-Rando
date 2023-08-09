using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TRColour4 : ISerializableCompact
{
    public byte Red { get; set; }

    public byte Green { get; set; }

    public byte Blue { get; set; }

    public byte Unused { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new(base.ToString());

        sb.Append(" Red: " + Red);
        sb.Append(" Green: " + Green);
        sb.Append(" Blue: " + Blue);
        sb.Append(" Unused: " + Unused);

        return sb.ToString();
    }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write(Red);
            writer.Write(Green);
            writer.Write(Blue);
            writer.Write(Unused);
        }

        return stream.ToArray();
    }
}
