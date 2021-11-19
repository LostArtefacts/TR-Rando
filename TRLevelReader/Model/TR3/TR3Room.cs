using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TR3Room : ISerializableCompact
    {
        private readonly byte SPOT_LIGHT_TYPE = 0;
        private readonly byte SUN_LIGHT_TYPE = 1;

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

        public TR3RoomLight[] Lights { get; set; }

        public ushort NumStaticMeshes { get; set; }

        public TR3RoomStaticMesh[] StaticMeshes { get; set; }

        public short AlternateRoom { get; set; }

        public short Flags { get; set; }

        public byte WaterScheme { get; set; }

        public byte ReverbInfo { get; set; }

        public byte Filler { get; set; }

        public bool ContainsWater
        {
            get
            {
                return (Flags & 0x01) > 0;
            }
            set
            {
                if (value)
                {
                    Flags |= 0x01;
                }
                else
                {
                    Flags &= ~0x01;
                }
            }
        }

        public void SetAmbient(short val)
        {
            AmbientIntensity = val;
        }

        public void SetLights(ushort val)
        {
            foreach (TR3RoomLight light in Lights)
            {
                Debug.Assert(light.LightProperties.Length == 4);

                if (light.LightType == SUN_LIGHT_TYPE)
                {
                    light.LightProperties[0] = 0;
                    light.LightProperties[1] = 0;
                    light.LightProperties[2] = 0;
                    light.LightProperties[3] = 0;
                } 
                else if (light.LightType == SPOT_LIGHT_TYPE)
                {
                    TR3RoomSpotlight spotLight = new TR3RoomSpotlight
                    {
                        Intensity = ((light.LightProperties[0] << 16) | ((light.LightProperties[1]) & 0xffff)),
                        Fade = ((light.LightProperties[2] << 16) | ((light.LightProperties[3]) & 0xffff))
                    };

                    spotLight.Intensity = val;

                    light.LightProperties[0] = (short)(spotLight.Intensity >> 16);
                    light.LightProperties[1] = (short)(spotLight.Intensity & 0xFFFF);
                    light.LightProperties[2] = (short)(spotLight.Fade >> 16);
                    light.LightProperties[3] = (short)(spotLight.Fade & 0xFFFF);
                }
            }
        }

        public void SetStaticMeshLights(ushort val)
        {
            foreach (TR3RoomStaticMesh mesh in StaticMeshes)
            {
                //No light properties?
            }
        }

        public void SetVertexLight(short val)
        {
            foreach (TR3RoomVertex vert in RoomData.Vertices)
            {
                vert.Lighting = val;

                byte red = (byte)((vert.Colour & 0x7C00) >> 10);
                byte green = (byte)((vert.Colour & 0x03E0) >> 5);
                byte blue = (byte)(vert.Colour & 0x001F);

                red -= (byte)(red * val / 100);
                green -= (byte)(green * val / 100);
                blue -= (byte)(blue * val / 100);

                vert.Colour = (ushort)((red << 10) | (green << 5) | (blue));
            }
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
                    writer.Write(LightMode);
                    writer.Write(NumLights);

                    foreach (TR3RoomLight light in Lights)
                    {
                        writer.Write(light.Serialize());
                    }

                    writer.Write(NumStaticMeshes);

                    foreach (TR3RoomStaticMesh mesh in StaticMeshes)
                    {
                        writer.Write(mesh.Serialize());
                    }

                    writer.Write(AlternateRoom);
                    writer.Write(Flags);
                    writer.Write(WaterScheme);
                    writer.Write(ReverbInfo);
                    writer.Write(Filler);
                }

                return stream.ToArray();
            }
        }
    }
}
