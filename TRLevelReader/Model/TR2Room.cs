using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TR2Room
    {
        //16 bytes
        public TRRoomInfo Info { get; set; }

        //4 bytes
        public uint NumDataWords { get; set; }

        //2 * NumDataWords bytes
        public ushort[] Data { get; set; }

        //Variable
        public TRRoomData RoomData { get; set; }

        //2 bytes
        public ushort NumPortals { get; set; }

        //32 * NumPortals bytes
        public TRRoomPortal[] Portals { get; set; }

        //2 bytes
        public ushort NumZSectors { get; set; }

        //2 bytes
        public ushort NumXSectors { get; set; }

        //(Xs * Zs) * 8 bytes 
        public TRRoomSector[] SectorList { get; set; }

        //2 bytes
        public short AmbientIntensity { get; set; }

        //2 bytes
        public short AmbientIntensity2 { get; set; }

        //2 bytes
        public short LightMode { get; set; }

        //2 bytes
        public ushort NumLights { get; set; }

        //24 * NumLights bytes
        public TR2RoomLight[] Lights { get; set; }

        //2 bytes
        public ushort NumStaticMeshes { get; set; }

        //20 * NumStaticMeshes bytes
        public TR2RoomStaticMesh[] StaticMeshes { get; set; }

        //2 bytes
        public short AlternateRoom { get; set; }

        //2 bytes
        public short Flags { get; set; }
    }
}
