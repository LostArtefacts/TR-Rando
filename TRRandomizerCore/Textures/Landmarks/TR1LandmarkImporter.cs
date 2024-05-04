using TRImageControl.Packing;
using TRLevelControl.Model;
using TRTexture16Importer.Textures;

namespace TRRandomizerCore.Textures;

public class TR1LandmarkImporter : AbstractLandmarkImporter<TR1Type, TR1Level>
{
    protected override int MaxTextures => IsCommunityPatch ? 8192 : 2048;

    protected override AbstractTexturePacker<TR1Type, TR1Level> CreatePacker(TR1Level level)
    {
        return new TR1TexturePacker(level);
    }

    protected override List<TRObjectTexture> GetObjectTextures(TR1Level level)
    {
        return level.ObjectTextures;
    }

    protected override void SetRoomTexture(TR1Level level, int roomIndex, int rectangleIndex, ushort textureIndex)
    {
        level.Rooms[roomIndex].Mesh.Rectangles[rectangleIndex].Texture = textureIndex;
    }

    protected override short? GetRoomFromPortal(TR1Level level, PortalSector portalSector, bool isLevelMirrored)
    {
        TR1Room room = level.Rooms[portalSector.Room];
        int x = isLevelMirrored ? (room.NumXSectors - portalSector.X - 1) : portalSector.X;
        TRRoomSector sector = room.Sectors[x * room.NumZSectors + portalSector.Z];

        return GetSectorPortalRoom(sector, level.FloorData, portalSector.Direction);
    }
}
