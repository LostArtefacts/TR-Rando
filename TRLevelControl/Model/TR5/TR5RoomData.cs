namespace TRLevelControl.Model;

public class TR5RoomData
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

    public byte[] SerializeRaw()
    {
        return AsBytes;
    }

    public bool FlattenLightsBulbsAndSectors()
    {
        if (AsBytes != null)
        {
            using MemoryStream stream = new();
            using (BinaryWriter writer = new(stream))
            {
                foreach (TR5RoomLight light in Lights) { writer.Write(light.Serialize()); }
                foreach (TR5FogBulb fbulb in FogBulbs) { writer.Write(fbulb.Serialize()); }
                foreach (TRRoomSector sector in SectorList) { writer.Write(sector.Serialize()); }
            }

            byte[] flattenedData = stream.ToArray();

            Array.Copy(flattenedData, 0, AsBytes, 0, flattenedData.Length);

            return true;
        }

        return false;
    }
}
