using System.IO;
using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TRAnimatedTexture : ISerializableCompact
    {
        //https://opentomb.github.io/TRosettaStone3/trosettastone.html#_animated_textures_2
        public ushort NumTextures => (ushort)(Textures.Length - 1);
        public ushort[] Textures { get; set; }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(NumTextures);
                foreach (ushort texture in Textures)
                {
                    writer.Write(texture);
                }

                return stream.ToArray();
            }
        }
    }
}