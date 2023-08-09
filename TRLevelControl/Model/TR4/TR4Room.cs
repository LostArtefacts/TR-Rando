﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

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
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
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

            foreach (TR4RoomLight light in Lights)
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
