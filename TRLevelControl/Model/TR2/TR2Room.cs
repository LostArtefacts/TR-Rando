using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model
{
    public class TR2Room : ISerializableCompact
    {
        public static readonly short DarknessIntensity1 = 8000;
        public static readonly ushort DarknessIntensity2 = 1000;

        //16 bytes
        public TRRoomInfo Info { get; set; }

        //4 bytes
        public uint NumDataWords { get; set; }

        //2 * NumDataWords bytes
        public ushort[] Data { get; set; }

        //Variable
        public TR2RoomData RoomData { get; set; }

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

        public bool ContainsWater 
        {
            get
            {
                return ((Flags & 0x01) > 0);
            }
        }

        public void Fill()
        {
            Flags |= 0x01;
        }

        public void Drain()
        {
            Flags &= ~(0x01);
        }

        public bool IsSkyboxVisible
        {
            get => (Flags & 0x08) > 0;
            set
            {
                if (value)
                {
                    Flags |= 0x08;
                }
                else
                {
                    Flags &= ~0x08;
                }
            }
        }

        public bool IsWindy
        {
            get => (Flags & 0x20) > 0;
            set
            {
                if (value)
                {
                    Flags |= 0x20;
                }
                else
                {
                    Flags &= ~0x20;
                }
            }
        }

        //Ambient Intensity = 0 (bright) - 0x1FFF (dark)
        //Vertex Light = 0 (bright) - 0x1FFF (dark)
        //RoomStaticMesh intensity = 0 (bright) - 0x1FFF (dark)
        //but...
        //Light intensity = 0 (dark) - 0x1FFF (bright)!!!
        public void SetLights(ushort val)
        {
            foreach (TR2RoomLight light in Lights)
            {
                light.Intensity1 = val;
                light.Intensity2 = val;
            }
        }

        public void SetStaticMeshLights(ushort val)
        {
            foreach (TR2RoomStaticMesh mesh in StaticMeshes)
            {
                mesh.Intensity1 = val;
                mesh.Intensity2 = val;
            }
        }

        public void SetVertexLight(short val)
        {
            foreach (TR2RoomVertex vert in RoomData.Vertices)
            {
                vert.Lighting = val;
                vert.Lighting2 = val;
            }
        }

        public void SetAmbient(short val)
        {
            AmbientIntensity = val;
            AmbientIntensity2 = val;
        }

        public void Darken()
        {
            SetAmbient(DarknessIntensity1);
            SetLights(DarknessIntensity2);
            SetStaticMeshLights((ushort)DarknessIntensity1);
            SetVertexLight(DarknessIntensity1);
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

                    foreach (TRRoomSector sector in SectorList)
                    {
                        writer.Write(sector.Serialize());
                    }

                    writer.Write(AmbientIntensity);
                    writer.Write(AmbientIntensity2);
                    writer.Write(LightMode);
                    writer.Write(NumLights);

                    foreach (TR2RoomLight light in Lights)
                    {
                        writer.Write(light.Serialize());
                    }

                    writer.Write(NumStaticMeshes);

                    foreach (TR2RoomStaticMesh mesh in StaticMeshes)
                    {
                        writer.Write(mesh.Serialize());
                    }

                    writer.Write(AlternateRoom);
                    writer.Write(Flags);
                }

                return stream.ToArray();
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(" RoomInfo: { " + Info.ToString() + "}");

            return sb.ToString();
        }
    }
}
