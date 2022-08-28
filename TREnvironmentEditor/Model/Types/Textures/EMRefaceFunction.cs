using System.Collections.Generic;
using TREnvironmentEditor.Helpers;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMRefaceFunction : BaseEMFunction, ITextureModifier
    {
        public EMTextureMap TextureMap { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            ApplyTextures(level);
        }

        public override void ApplyToLevel(TR2Level level)
        {
            ApplyTextures(level);
        }

        public override void ApplyToLevel(TR3Level level)
        {
            ApplyTextures(level);
        }

        public void ApplyTextures(TRLevel level)
        {
            EMLevelData data = GetData(level);

            foreach (ushort texture in TextureMap.Keys)
            {
                foreach (int roomIndex in TextureMap[texture].Keys)
                {
                    TRRoom room = level.Rooms[data.ConvertRoom(roomIndex)];
                    ApplyTextures(texture, TextureMap[texture][roomIndex], room.RoomData.Rectangles, room.RoomData.Triangles);
                }
            }
        }

        public void ApplyTextures(TR2Level level)
        {
            EMLevelData data = GetData(level);

            foreach (ushort texture in TextureMap.Keys)
            {
                foreach (int roomIndex in TextureMap[texture].Keys)
                {
                    TR2Room room = level.Rooms[data.ConvertRoom(roomIndex)];
                    ApplyTextures(texture, TextureMap[texture][roomIndex], room.RoomData.Rectangles, room.RoomData.Triangles);
                }
            }
        }

        public void ApplyTextures(TR3Level level)
        {
            EMLevelData data = GetData(level);

            foreach (ushort texture in TextureMap.Keys)
            {
                foreach (int roomIndex in TextureMap[texture].Keys)
                {
                    TR3Room room = level.Rooms[data.ConvertRoom(roomIndex)];
                    ApplyTextures(texture, TextureMap[texture][roomIndex], room.RoomData.Rectangles, room.RoomData.Triangles);
                }
            }
        }

        private void ApplyTextures(ushort texture, Dictionary<EMTextureFaceType, int[]> faceMap, TRFace4[] rectangles, TRFace3[] triangles)
        {
            foreach (EMTextureFaceType faceType in faceMap.Keys)
            {
                foreach (int faceIndex in faceMap[faceType])
                {
                    switch (faceType)
                    {
                        case EMTextureFaceType.Rectangles:
                            rectangles[faceIndex].Texture = texture;
                            break;
                        case EMTextureFaceType.Triangles:
                            triangles[faceIndex].Texture = texture;
                            break;
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