using TRLevelReader.Model;
using TRModelTransporter.Model.Textures;

namespace TREnvironmentEditor.Model.Types
{
    public class EMMirrorObjectTexture : BaseEMFunction
    {
        public ushort[] Textures { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            MirrorObjectTextures(level.ObjectTextures);
        }

        public override void ApplyToLevel(TR2Level level)
        {
            MirrorObjectTextures(level.ObjectTextures);
        }

        public override void ApplyToLevel(TR3Level level)
        {
            MirrorObjectTextures(level.ObjectTextures);
        }

        private void MirrorObjectTextures(TRObjectTexture[] levelTextures)
        {
            foreach (ushort textureRef in Textures)
            {
                IndexedTRObjectTexture texture = new IndexedTRObjectTexture
                {
                    Texture = levelTextures[textureRef]
                };

                if (texture.IsTriangle)
                {
                    Swap(texture.Texture.Vertices, 0, 2);
                }
                else
                {
                    Swap(texture.Texture.Vertices, 0, 3);
                    Swap(texture.Texture.Vertices, 1, 2);
                }
            }
        }

        private static void Swap<T>(T[] arr, int pos1, int pos2)
        {
            T temp = arr[pos1];
            arr[pos1] = arr[pos2];
            arr[pos2] = temp;
        }
    }
}