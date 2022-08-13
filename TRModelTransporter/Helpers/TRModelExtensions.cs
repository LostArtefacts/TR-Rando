using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Model;

namespace TRModelTransporter.Helpers
{
    public static class TRModelExtensions
    {
        private static readonly FixedFloat16 _nullCoord = new FixedFloat16 { Fraction = 0, Whole = 0 };

        public static void ResetUnusedTextures(this TRLevel level)
        {
            ResetUnusedObjectTextures(level.ObjectTextures);
            ResetUnusedSpriteTextures(level.SpriteTextures);
        }

        public static void ResetUnusedTextures(this TR2Level level)
        {
            ResetUnusedObjectTextures(level.ObjectTextures);
            ResetUnusedSpriteTextures(level.SpriteTextures);
        }

        public static void ResetUnusedTextures(this TR3Level level)
        {
            ResetUnusedObjectTextures(level.ObjectTextures);
            ResetUnusedSpriteTextures(level.SpriteTextures);
        }

        private static void ResetUnusedObjectTextures(IEnumerable<TRObjectTexture> objectTextures)
        {
            foreach (TRObjectTexture texture in objectTextures)
            {
                if (texture.AtlasAndFlag == ushort.MaxValue)
                {
                    texture.Invalidate();
                }
            }
        }

        private static void ResetUnusedSpriteTextures(IEnumerable<TRSpriteTexture> spriteTextures)
        {
            foreach (TRSpriteTexture texture in spriteTextures)
            {
                if (texture.Atlas == ushort.MaxValue)
                {
                    texture.Invalidate();
                }
            }
        }

        // See TextureTransportHandler.ResetUnusedTextures
        public static bool IsValid(this TRObjectTexture texture)
        {
            if (texture.AtlasAndFlag == 0)
            {
                int coords = 0;
                foreach (TRObjectTextureVert vert in texture.Vertices)
                {
                    coords += vert.XCoordinate.Whole + vert.XCoordinate.Fraction + vert.YCoordinate.Whole + vert.XCoordinate.Fraction;
                }
                return coords > 0;
            }

            return texture.AtlasAndFlag != ushort.MaxValue;
        }

        public static void Invalidate(this TRObjectTexture texture)
        {
            texture.AtlasAndFlag = 0;
            foreach (TRObjectTextureVert vert in texture.Vertices)
            {
                vert.XCoordinate = vert.YCoordinate = _nullCoord;
            }
        }

        // See TextureTransportHandler.ResetUnusedTextures
        public static bool IsValid(this TRSpriteTexture texture)
        {
            if (texture.Atlas == 0)
            {
                if (texture.X == 0 && texture.Y == 0 && texture.Width == 1 && texture.Height == 1)
                {
                    return false;
                }
            }

            return texture.Atlas != ushort.MaxValue;
        }

        public static void Invalidate(this TRSpriteTexture texture)
        {
            texture.Atlas = 0;
            texture.X = texture.Y = 0;
            texture.Width = texture.Height = 1;
        }

        public static List<int> GetInvalidObjectTextureIndices(this TRLevel level)
        {
            return GetInvalidObjectTextureIndices(level.ObjectTextures);
        }

        public static List<int> GetInvalidObjectTextureIndices(this TR2Level level)
        {
            return GetInvalidObjectTextureIndices(level.ObjectTextures);
        }

        public static List<int> GetInvalidObjectTextureIndices(this TR3Level level)
        {
            return GetInvalidObjectTextureIndices(level.ObjectTextures);
        }

        private static List<int> GetInvalidObjectTextureIndices(TRObjectTexture[] objectTextures)
        {
            List<int> reusableIndices = new List<int>();
            for (int i = 0; i < objectTextures.Length; i++)
            {
                if (!objectTextures[i].IsValid())
                {
                    reusableIndices.Add(i);
                }
            }
            return reusableIndices;
        }

        // Given a precompiled dictionary of old texture index to new, this will ensure that
        // all Meshes, RoomData and AnimatedTextures point to the new correct index.
        public static void ReindexTextures(this TR2Level level, Dictionary<int, int> indexMap, bool defaultToOriginal = true)
        {
            if (indexMap.Count == 0)
            {
                return;
            }

            foreach (TRMesh mesh in level.Meshes)
            {
                foreach (TRFace4 rect in mesh.TexturedRectangles)
                {
                    rect.Texture = ConvertTextureReference(rect.Texture, indexMap, defaultToOriginal);
                }
                foreach (TRFace3 tri in mesh.TexturedTriangles)
                {
                    tri.Texture = ConvertTextureReference(tri.Texture, indexMap, defaultToOriginal);
                }
            }

            foreach (TR2Room room in level.Rooms)
            {
                foreach (TRFace4 rect in room.RoomData.Rectangles)
                {
                    rect.Texture = ConvertTextureReference(rect.Texture, indexMap, defaultToOriginal);
                }
                foreach (TRFace3 tri in room.RoomData.Triangles)
                {
                    tri.Texture = ConvertTextureReference(tri.Texture, indexMap, defaultToOriginal);
                }
            }

            // #137 Ensure animated textures are reindexed too (these are just groups of texture indices)
            // They have to remain unique it seems, otherwise the animation speed is too fast, so while we
            // have removed the duplicated textures, we can re-add duplicate texture objects while there is 
            // enough space in that array.
            List<TRObjectTexture> textures = level.ObjectTextures.ToList();
            foreach (TRAnimatedTexture anim in level.AnimatedTextures)
            {
                for (int i = 0; i < anim.Textures.Length; i++)
                {
                    anim.Textures[i] = ConvertTextureReference(anim.Textures[i], indexMap, defaultToOriginal);
                }

                ushort previousIndex = anim.Textures[0];
                for (int i = 1; i < anim.Textures.Length; i++)
                {
                    if (anim.Textures[i] == previousIndex && textures.Count < 2048)
                    {
                        textures.Add(textures[anim.Textures[i]]);
                        anim.Textures[i] = (ushort)(textures.Count - 1);
                    }
                    previousIndex = anim.Textures[i];
                }
            }

            if (textures.Count > level.NumObjectTextures)
            {
                level.ObjectTextures = textures.ToArray();
                level.NumObjectTextures = (uint)textures.Count;
            }
        }

        public static void ReindexTextures(this TR3Level level, Dictionary<int, int> indexMap, bool defaultToOriginal = true)
        {
            if (indexMap.Count == 0)
            {
                return;
            }

            foreach (TRMesh mesh in level.Meshes)
            {
                foreach (TRFace4 rect in mesh.TexturedRectangles)
                {
                    rect.Texture = ConvertTextureReference(rect.Texture, indexMap, defaultToOriginal);
                }
                foreach (TRFace3 tri in mesh.TexturedTriangles)
                {
                    tri.Texture = ConvertTextureReference(tri.Texture, indexMap, defaultToOriginal);
                }
            }

            foreach (TR3Room room in level.Rooms)
            {
                foreach (TRFace4 rect in room.RoomData.Rectangles)
                {
                    rect.Texture = ConvertTextureReference(rect.Texture, indexMap, defaultToOriginal);
                }
                foreach (TRFace3 tri in room.RoomData.Triangles)
                {
                    tri.Texture = ConvertTextureReference(tri.Texture, indexMap, defaultToOriginal);
                }
            }

            List<TRObjectTexture> textures = level.ObjectTextures.ToList();
            foreach (TRAnimatedTexture anim in level.AnimatedTextures)
            {
                for (int i = 0; i < anim.Textures.Length; i++)
                {
                    anim.Textures[i] = ConvertTextureReference(anim.Textures[i], indexMap, defaultToOriginal);
                }

                ushort previousIndex = anim.Textures[0];
                for (int i = 1; i < anim.Textures.Length; i++)
                {
                    if (anim.Textures[i] == previousIndex && textures.Count < 2048)
                    {
                        textures.Add(textures[anim.Textures[i]]);
                        anim.Textures[i] = (ushort)(textures.Count - 1);
                    }
                    previousIndex = anim.Textures[i];
                }
            }

            if (textures.Count > level.NumObjectTextures)
            {
                level.ObjectTextures = textures.ToArray();
                level.NumObjectTextures = (uint)textures.Count;
            }
        }

        private static ushort ConvertTextureReference(ushort textureReference, Dictionary<int, int> indexMap, bool defaultToOriginal)
        {
            if (indexMap.ContainsKey(textureReference))
            {
                return (ushort)indexMap[textureReference];
            }

            return defaultToOriginal ? textureReference : (ushort)0;
        }
    }
}