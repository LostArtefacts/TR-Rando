using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TR5RoomData : ISerializableCompact
    {
        public TR5RoomLight[] Lights { get; set; }

        public TR5FogBulb[] FogBulbs { get; set; }

        public TRRoomSector[] SectorList { get; set; }

        public ushort NumPortals { get; set; }

        public TRRoomPortal[] Portals { get; set; }

        public ushort Seperator { get; set; }

        public TR3RoomStaticMesh[] StaticMeshes { get; set; }

        public TR5RoomLayer[] Layers { get; set; }

        public byte[] Faces { get; set; }

        public TR5RoomVertex[] Vertices { get; set; }

        public byte[] AsBytes { get; set; }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    foreach (TR5RoomLight light in Lights) { writer.Write(light.Serialize()); }
                    foreach (TR5FogBulb fbulb in FogBulbs) { writer.Write(fbulb.Serialize()); }
                    foreach (TRRoomSector sector in SectorList) { writer.Write(sector.Serialize()); }
                    writer.Write(NumPortals);
                    foreach (TRRoomPortal portal in Portals) { writer.Write(portal.Serialize()); }
                    writer.Write(Seperator);
                    foreach (TR3RoomStaticMesh smesh in StaticMeshes) { writer.Write(smesh.Serialize()); }
                    foreach (TR5RoomLayer layer in Layers) { writer.Write(layer.Serialize()); }
                    writer.Write(Faces);
                    foreach (TR5RoomVertex vert in Vertices) { writer.Write(vert.Serialize()); }
                }

                return stream.ToArray();
            }
        }

        public byte[] SerializeRaw()
        {
            return AsBytes;
        }
    }
}
