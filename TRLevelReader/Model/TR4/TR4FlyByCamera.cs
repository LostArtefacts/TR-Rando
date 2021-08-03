using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelReader.Serialization;

namespace TRLevelReader.Model
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
            throw new NotImplementedException();
        }
    }
}
