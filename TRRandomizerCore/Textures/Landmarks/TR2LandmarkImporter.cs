using TRFDControl;
using TRLevelControl.Model;
using TRLevelControl.Model.Enums;
using TRModelTransporter.Packing;
using TRTexture16Importer.Textures;

namespace TRRandomizerCore.Textures;

public class TR2LandmarkImporter : AbstractLandmarkImporter<TR2Entities, TR2Level>
{
    protected override int MaxTextures => 2048;

    protected override AbstractTexturePacker<TR2Entities, TR2Level> CreatePacker(TR2Level level)
    {
        return new TR2TexturePacker(level);
    }

    protected override TRObjectTexture[] GetObjectTextures(TR2Level level)
    {
        return level.ObjectTextures;
    }

    protected override void SetObjectTextures(TR2Level level, IEnumerable<TRObjectTexture> textures)
    {
        level.ObjectTextures = textures.ToArray();
        level.NumObjectTextures = (uint)level.ObjectTextures.Length;
    }

    protected override void SetRoomTexture(TR2Level level, int roomIndex, int rectangleIndex, ushort textureIndex)
    {
        level.Rooms[roomIndex].RoomData.Rectangles[rectangleIndex].Texture = textureIndex;
    }

    protected override short? GetRoomFromPortal(TR2Level level, PortalSector portalSector, bool isLevelMirrored)
    {
        FDControl floorData = new FDControl();
        floorData.ParseFromLevel(level);

        TR2Room room = level.Rooms[portalSector.Room];
        int x = isLevelMirrored ? (room.NumXSectors - portalSector.X - 1) : portalSector.X;
        TRRoomSector sector = room.SectorList[x * room.NumZSectors + portalSector.Z];

        return GetSectorPortalRoom(sector, floorData, portalSector.Direction);
    }
}
