using TRFDControl;
using TRLevelControl.Model;
using TRModelTransporter.Packing;
using TRTexture16Importer.Textures;

namespace TRRandomizerCore.Textures;

public class TR3LandmarkImporter : AbstractLandmarkImporter<TR3Entities, TR3Level>
{
    protected override int MaxTextures => 4096;

    protected override AbstractTexturePacker<TR3Entities, TR3Level> CreatePacker(TR3Level level)
    {
        return new TR3TexturePacker(level);
    }

    protected override TRObjectTexture[] GetObjectTextures(TR3Level level)
    {
        return level.ObjectTextures;
    }

    protected override void SetObjectTextures(TR3Level level, IEnumerable<TRObjectTexture> textures)
    {
        level.ObjectTextures = textures.ToArray();
        level.NumObjectTextures = (uint)level.ObjectTextures.Length;
    }

    protected override void SetRoomTexture(TR3Level level, int roomIndex, int rectangleIndex, ushort textureIndex)
    {
        level.Rooms[roomIndex].RoomData.Rectangles[rectangleIndex].Texture = textureIndex;
    }

    protected override short? GetRoomFromPortal(TR3Level level, PortalSector portalSector, bool isLevelMirrored)
    {
        FDControl floorData = new();
        floorData.ParseFromLevel(level);

        TR3Room room = level.Rooms[portalSector.Room];
        int x = isLevelMirrored ? (room.NumXSectors - portalSector.X - 1) : portalSector.X;
        TRRoomSector sector = room.Sectors[x * room.NumZSectors + portalSector.Z];

        return GetSectorPortalRoom(sector, floorData, portalSector.Direction);
    }
}
