using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TR4Room : ISerializableCompact
    {
        public TRRoomInfo Info { get; set; }

        public uint NumDataWords { get; set; }

        public ushort[] Data { get; set; }

        public TR3RoomData RoomData { get; set; }

        public ushort NumPortals { get; set; }

        public TRRoomPortal[] Portals { get; set; }

        public ushort NumZSectors { get; set; }

        public ushort NumXSectors { get; set; }

        public TRRoomSector[] Sectors { get; set; }

        public short AmbientIntensity { get; set; }

        public short LightMode { get; set; }

        public ushort NumLights { get; set; }

        public TR4RoomLight[] Lights { get; set; }

        public ushort NumStaticMeshes { get; set; }

        public TR3RoomStaticMesh[] StaticMeshes { get; set; }

        public short AlternateRoom { get; set; }

        public short Flags { get; set; }

        public byte WaterScheme { get; set; }

        public byte ReverbInfo { get; set; }

        public byte Filler { get; set; }

        public byte[] Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
