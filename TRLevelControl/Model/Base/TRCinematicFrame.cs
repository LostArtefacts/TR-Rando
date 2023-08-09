using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TRCinematicFrame : ISerializableCompact
{
    public short TargetX { get; set; }

    public short TargetY { get; set; }

    public short TargetZ { get; set; }

    public short PosZ { get; set; }

    public short PosY { get; set; }

    public short PosX { get; set; }

    public short FOV { get; set; }

    public short Roll { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder(base.ToString());

        sb.Append(" TargetX: " + TargetX);
        sb.Append(" TargetY: " + TargetY);
        sb.Append(" TargetZ: " + TargetZ);
        sb.Append(" PosZ: " + PosZ);
        sb.Append(" PosY: " + PosY);
        sb.Append(" PosX: " + PosX);
        sb.Append(" FOV: " + FOV);
        sb.Append(" Roll: " + Roll);

        return sb.ToString();
    }

    public byte[] Serialize()
    {
        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(TargetX);
                writer.Write(TargetY);
                writer.Write(TargetZ);
                writer.Write(PosZ);
                writer.Write(PosY);
                writer.Write(PosX);
                writer.Write(FOV);
                writer.Write(Roll);
            }

            return stream.ToArray();
        }
    }
}
