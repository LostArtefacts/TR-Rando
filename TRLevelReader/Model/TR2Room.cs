using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TR2Room
    {
        public TRRoomInfo Info { get; set; }

        public uint NumDataWords { get; set; }

        public ushort[] Data { get; set; }

        public TRRoomData RoomData { get; set; }

        public ushort NumPortals { get; set; }

        public TRRoomPortal[] Portals { get; set; }

        public ushort NumZSectors { get; set; }

        public ushort NumXSectors { get; set; }

        public TRRoomSector[] SectorList { get; set; }

        public short AmbientIntensity { get; set; }

        public short AmbientIntensity2 { get; set; }

        public short LightMode { get; set; }

        public ushort NumLights { get; set; }

        public TR2RoomLight[] Lights { get; set; }

        public ushort NumStaticMeshes { get; set; }

        public TR2RoomStaticMesh[] StaticMeshes { get; set; }

        public short AlternateRoom { get; set; }

        public short Flags { get; set; }
    }
}
