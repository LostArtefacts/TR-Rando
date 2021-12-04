using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Packing;

namespace TRRandomizerCore.Textures
{
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
    }
}