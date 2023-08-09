using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TRAnimation : ISerializableCompact
{
    public uint FrameOffset { get; set; }

    public byte FrameRate { get; set; }

    public byte FrameSize { get; set; }

    public ushort StateID { get; set; }

    //fixed Speed - 4 bytes (2 for whole 2 for frac);
    public FixedFloat32 Speed { get; set; }

    //fixed Accel - 4 bytes (2 for whole 2 for frac);
    public FixedFloat32 Accel { get; set; }

    public ushort FrameStart { get; set; }

    public ushort FrameEnd { get; set; }

    public ushort NextAnimation { get; set; }

    public ushort NextFrame { get; set; }

    public ushort NumStateChanges { get; set; }

    public ushort StateChangeOffset { get; set; }

    public ushort NumAnimCommands { get; set; }

    public ushort AnimCommand { get; set; }

    public byte[] Serialize()
    {
        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(FrameOffset);
                writer.Write(FrameRate);
                writer.Write(FrameSize);
                writer.Write(StateID);
                writer.Write(Speed.Serialize());
                writer.Write(Accel.Serialize());
                writer.Write(FrameStart);
                writer.Write(FrameEnd);
                writer.Write(NextAnimation);
                writer.Write(NextFrame);
                writer.Write(NumStateChanges);
                writer.Write(StateChangeOffset);
                writer.Write(NumAnimCommands);
                writer.Write(AnimCommand);
            }

            return stream.ToArray();
        }
    }
}
