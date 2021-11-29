using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Packing;

namespace TRRandomizerCore.Textures
{
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
    }
}