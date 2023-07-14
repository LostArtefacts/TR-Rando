using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelControl.Serialization;

namespace TRLevelControl.Model
{
    public class TR4FlyByCamera : ISerializableCompact
    {
        public int X { get; set; }

        public int Y { get; set; }

        public int Z { get; set; }

        public int dx { get; set; }

        public int dy { get; set; }

        public int dz { get; set; }

        public byte Sequence { get; set; }

        public byte Index { get; set; }

        public ushort FOV { get; set; }

        public short Roll { get; set; }

        public ushort Timer { get; set; }

        public ushort Speed { get; set; }

        public ushort Flags { get; set; }

        public uint RoomID { get; set; }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(X);
                    writer.Write(Y);
                    writer.Write(Z);
                    writer.Write(dx);
                    writer.Write(dy);
                    writer.Write(dz);
                    writer.Write(Sequence);
                    writer.Write(Index);
                    writer.Write(FOV);
                    writer.Write(Roll);
                    writer.Write(Timer);
                    writer.Write(Speed);
                    writer.Write(Flags);
                    writer.Write(RoomID);
                }

                return stream.ToArray();
            }
        }
    }
}
