using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TRAnimDispatch : ISerializableCompact
{
    public short Low { get; set; }

    public short High { get; set; }

    public short NextAnimation { get; set; }

    public short NextFrame { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new(base.ToString());

        sb.Append(" Low: " + Low);
        sb.Append(" High: " + High);
        sb.Append(" NextAnimation: " + NextAnimation);
        sb.Append(" NextFrame: " + NextFrame);

        return sb.ToString();
    }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write(Low);
            writer.Write(High);
            writer.Write(NextAnimation);
            writer.Write(NextFrame);
        }

        return stream.ToArray();
    }
}
