using System.Collections.Generic;
using TREnvironmentEditor.Helpers;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMRefaceFunction : BaseEMFunction, ITextureModifier
    {
        public EMTextureMap TextureMap { get; set; }

        public override void ApplyToLevel(TR2Level level)
        {
            ApplyTextures(level);
        }

        public void ApplyTextures(TR2Level level)
        {
            foreach (ushort texture in TextureMap.Keys)
            {
                foreach (int roomIndex in TextureMap[texture].Keys)
                {
                    foreach (EMTextureFaceType faceType in TextureMap[texture][roomIndex].Keys)
                    {
                        foreach (int faceIndex in TextureMap[texture][roomIndex][faceType])
                        {
                            switch (faceType)
                            {
                                case EMTextureFaceType.Rectangles:
                                    level.Rooms[roomIndex].RoomData.Rectangles[faceIndex].Texture = texture;
                                    break;
                                case EMTextureFaceType.Triangles:
                                    level.Rooms[roomIndex].RoomData.Triangles[faceIndex].Texture = texture;
                                    break;
                            }
                        }
                    }
                }
            }
        }

        public void RemapTextures(Dictionary<ushort, ushort> indexMap)
        {
            TextureMap.Remap(indexMap);
        }
    }
}