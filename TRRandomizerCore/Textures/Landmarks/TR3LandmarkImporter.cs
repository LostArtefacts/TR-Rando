using TRImageControl.Packing;
using TRImageControl.Textures;
using TRLevelControl.Model;

namespace TRRandomizerCore.Textures;

public class TR3LandmarkImporter : AbstractLandmarkImporter<TR3Type, TR3Level>
{
    protected override int MaxTextures => 4096;

    protected override TRTexturePacker CreatePacker(TR3Level level)
    {
        return new TR3TexturePacker(level);
    }

    protected override List<TRObjectTexture> GetObjectTextures(TR3Level level)
    {
        return level.ObjectTextures;
    }

    protected override void SetRoomTexture(TR3Level level, int roomIndex, int rectangleIndex, ushort textureIndex)
    {
        level.Rooms[roomIndex].Mesh.Rectangles[rectangleIndex].Texture = textureIndex;
    }

    protected override short? GetRoomFromPortal(TR3Level level, PortalSector portalSector, bool isLevelMirrored)
    {
        TR3Room room = level.Rooms[portalSector.Room];
        int x = isLevelMirrored ? (room.NumXSectors - portalSector.X - 1) : portalSector.X;
        TRRoomSector sector = room.Sectors[x * room.NumZSectors + portalSector.Z];

        return GetSectorPortalRoom(sector, level.FloorData, portalSector.Direction);
    }
}
