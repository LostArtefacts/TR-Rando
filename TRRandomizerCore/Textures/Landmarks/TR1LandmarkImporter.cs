using System.Collections.Generic;
using System.Linq;
using TRFDControl;
using TRLevelControl.Model;
using TRLevelControl.Model.Enums;
using TRModelTransporter.Packing;
using TRTexture16Importer.Textures;

namespace TRRandomizerCore.Textures
{
    public class TR1LandmarkImporter : AbstractLandmarkImporter<TREntities, TR1Level>
    {
        protected override int MaxTextures => IsCommunityPatch ? 8192 : 2048;

        protected override AbstractTexturePacker<TREntities, TR1Level> CreatePacker(TR1Level level)
        {
            return new TR1TexturePacker(level);
        }

        protected override TRObjectTexture[] GetObjectTextures(TR1Level level)
        {
            return level.ObjectTextures;
        }

        protected override void SetObjectTextures(TR1Level level, IEnumerable<TRObjectTexture> textures)
        {
            level.ObjectTextures = textures.ToArray();
            level.NumObjectTextures = (uint)level.ObjectTextures.Length;
        }

        protected override void SetRoomTexture(TR1Level level, int roomIndex, int rectangleIndex, ushort textureIndex)
        {
            level.Rooms[roomIndex].RoomData.Rectangles[rectangleIndex].Texture = textureIndex;
        }

        protected override short? GetRoomFromPortal(TR1Level level, PortalSector portalSector, bool isLevelMirrored)
        {
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            TRRoom room = level.Rooms[portalSector.Room];
            int x = isLevelMirrored ? (room.NumXSectors - portalSector.X - 1) : portalSector.X;
            TRRoomSector sector = room.Sectors[x * room.NumZSectors + portalSector.Z];

            return GetSectorPortalRoom(sector, floorData, portalSector.Direction);
        }
    }
}