using TRImageControl.Packing;
using TRImageControl.Textures;
using TRLevelControl.Model;

namespace TRRandomizerCore.Textures;

public class TR2LandmarkImporter : AbstractLandmarkImporter<TR2Type, TR2Level>
{
    protected override int MaxTextures => int.MaxValue;

    protected override TRTexturePacker CreatePacker(TR2Level level)
    {
        return new TR2TexturePacker(level, maximumTiles: short.MaxValue);
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
