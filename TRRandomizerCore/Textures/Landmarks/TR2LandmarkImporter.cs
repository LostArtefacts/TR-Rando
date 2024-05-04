using TRImageControl.Packing;
using TRLevelControl.Model;
using TRTexture16Importer.Textures;

namespace TRRandomizerCore.Textures;

public class TR2LandmarkImporter : AbstractLandmarkImporter<TR2Type, TR2Level>
{
    protected override int MaxTextures => 2048;

    protected override AbstractTexturePacker<TR2Type, TR2Level> CreatePacker(TR2Level level)
    {
        return new TR2TexturePacker(level);
    }

    protected override List<TRObjectTexture> GetObjectTextures(TR2Level level)
    {
        return level.ObjectTextures;
    }

    protected override void SetRoomTexture(TR2Level level, int roomIndex, int rectangleIndex, ushort textureIndex)
    {
        level.Rooms[roomIndex].Mesh.Rectangles[rectangleIndex].Texture = textureIndex;
    }

    protected override short? GetRoomFromPortal(TR2Level level, PortalSector portalSector, bool isLevelMirrored)
    {
        TR2Room room = level.Rooms[portalSector.Room];
        int x = isLevelMirrored ? (room.NumXSectors - portalSector.X - 1) : portalSector.X;
        TRRoomSector sector = room.Sectors[x * room.NumZSectors + portalSector.Z];

        return GetSectorPortalRoom(sector, level.FloorData, portalSector.Direction);
    }
}
