using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TRRoom : ISerializableCompact
    {
        public TRRoomInfo Info { get; set; }

        public uint NumDataWords { get; set; }

        public ushort[] Data { get; set; }

        public TRRoomData RoomData { get; set; }

        public ushort NumPortals { get; set; }

        public TRRoomPortal[] Portals { get; set; }

        public ushort NumZSectors { get; set; }

        public ushort NumXSectors { get; set; }

        public TRRoomSector[] Sectors { get; set; }

        public short AmbientIntensity { get; set; }

        public ushort NumLights { get; set; }

        public TRRoomLight[] Lights { get; set; }

        public ushort NumStaticMeshes { get; set; }

        public TRRoomStaticMesh[] StaticMeshes { get; set; }

        public short AlternateRoom { get; set; }

        public short Flags { get; set; }

        public bool ContainsWater
        {
            get
            {
                return (Flags & 0x01) > 0;
            }
        }

        public void Fill()
        {
            Flags |= 0x01;
        }

        public void Drain()
        {
            Flags &= ~0x01;
        }

        //Ambient Intensity = 0 (bright) - 0x1FFF (dark)
        //Vertex Light = 0 (bright) - 0x1FFF (dark)
        //RoomStaticMesh intensity = 0 (bright) - 0x1FFF (dark)
        //but...
        //Light intensity = 0 (dark) - 0x1FFF (bright)!!!
        public void SetLights(ushort val)
        {
            foreach (TRRoomLight light in Lights)
            {
                light.Intensity = val;
            }
        }

        public void SetStaticMeshLights(ushort val)
        {
            foreach (TRRoomStaticMesh mesh in StaticMeshes)
            {
                mesh.Intensity = val;
            }
        }

        public void SetVertexLight(short val)
        {
            foreach (TRRoomVertex vert in RoomData.Vertices)
            {
                vert.Lighting = val;
            }
        }

        public void SetAmbient(short val)
        {
            AmbientIntensity = val;
        }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(Info.Serialize());
                    writer.Write(NumDataWords);

                    writer.Write(RoomData.Serialize());
                    writer.Write(NumPortals);

                    foreach (TRRoomPortal portal in Portals)
                    {
                        writer.Write(portal.Serialize());
                    }

                    writer.Write(NumZSectors);
                    writer.Write(NumXSectors);

                    foreach (TRRoomSector sector in Sectors)
                    {
                        writer.Write(sector.Serialize());
                    }

                    writer.Write(AmbientIntensity);
                    writer.Write(NumLights);

                    foreach (TRRoomLight light in Lights)
                    {
                        writer.Write(light.Serialize());
                    }

                    writer.Write(NumStaticMeshes);

                    foreach (TRRoomStaticMesh mesh in StaticMeshes)
                    {
                        writer.Write(mesh.Serialize());
                    }

                    writer.Write(AlternateRoom);
                    writer.Write(Flags);
                }

                return stream.ToArray();
            }
        }
     }
}
