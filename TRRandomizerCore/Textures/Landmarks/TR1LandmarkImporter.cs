using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Packing;

namespace TRRandomizerCore.Textures
{
    public class TR1LandmarkImporter : AbstractLandmarkImporter<TREntities, TRLevel>
    {
        protected override int MaxTextures => IsCommunityPatch ? 8192 : 2048;

        protected override AbstractTexturePacker<TREntities, TRLevel> CreatePacker(TRLevel level)
        {
            return new TR1TexturePacker(level);
        }

        protected override TRObjectTexture[] GetObjectTextures(TRLevel level)
        {
            return level.ObjectTextures;
        }

        protected override void SetObjectTextures(TRLevel level, IEnumerable<TRObjectTexture> textures)
        {
            level.ObjectTextures = textures.ToArray();
            level.NumObjectTextures = (uint)level.ObjectTextures.Length;
        }

        protected override void SetRoomTexture(TRLevel level, int roomIndex, int rectangleIndex, ushort textureIndex)
        {
            level.Rooms[roomIndex].RoomData.Rectangles[rectangleIndex].Texture = textureIndex;
        }
    }
}