using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR3Room : ISerializableCompact
{
    public TRRoomInfo Info { get; set; }
    public TR3RoomData RoomData { get; set; }
    public List<TRRoomPortal> Portals { get; set; }
    public ushort NumZSectors { get; set; }
    public ushort NumXSectors { get; set; }
    public List<TRRoomSector> Sectors { get; set; }
    public short AmbientIntensity { get; set; }
    public short LightMode { get; set; }
    public List<TR3RoomLight> Lights { get; set; }
    public List<TR3RoomStaticMesh> StaticMeshes { get; set; }
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

    public bool IsSwamp
    {
        get => (Flags & 0x80) > 0;
        set
        {
            if (value)
            {
                Flags |= 0x80;
            }
            else
            {
                Flags &= ~0x80;
            }
        }
    }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write(Info.Serialize());

            byte[] meshData = RoomData.Serialize();
            writer.Write((uint)meshData.Length / sizeof(short));
            writer.Write(meshData);

            writer.Write((ushort)Portals.Count);
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

            writer.Write((ushort)Lights.Count);
            foreach (TR3RoomLight light in Lights)
            {
                writer.Write(light.Serialize());
            }

            writer.Write((ushort)StaticMeshes.Count);
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
