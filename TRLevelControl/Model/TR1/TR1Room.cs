using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR1Room : ISerializableCompact
{
    public TRRoomInfo Info { get; set; }
    public TR1RoomMesh Mesh { get; set; }
    public List<TRRoomPortal> Portals { get; set; }
    public ushort NumZSectors { get; set; }
    public ushort NumXSectors { get; set; }
    public List<TRRoomSector> Sectors { get; set; }
    public short AmbientIntensity { get; set; }
    public List<TR1RoomLight> Lights { get; set; }
    public List<TR1RoomStaticMesh> StaticMeshes { get; set; }
    public short AlternateRoom { get; set; }
    public short Flags { get; set; }

    public bool ContainsWater
    {
        get => (Flags & 0x01) > 0;
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

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write(Info.Serialize());

            byte[] meshData = Mesh.Serialize();
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
            
            writer.Write((ushort)Lights.Count);
            foreach (TR1RoomLight light in Lights)
            {
                writer.Write(light.Serialize());
            }

            writer.Write((ushort)StaticMeshes.Count);
            foreach (TR1RoomStaticMesh mesh in StaticMeshes)
            {
                writer.Write(mesh.Serialize());
            }

            writer.Write(AlternateRoom);
            writer.Write(Flags);
        }

        return stream.ToArray();
    }
 }
