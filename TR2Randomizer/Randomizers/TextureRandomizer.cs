using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRTexture16Importer;

namespace TR2Randomizer.Randomizers
{
    public class TextureRandomizer : RandomizerBase
    {
        public TextureRandomizer() : base()
        {

        }

        public override void Randomize(int seed)
        {
            ReplacementStatusManager.CanRandomize = false;

            _generator = new Random(seed);

            foreach (string lvl in _levels)
            {
                _levelInstance = LoadLevel(lvl);

                string appDir = Directory.GetCurrentDirectory();

                //Access texture packs directory for the specified level
                Directory.SetCurrentDirectory("TexturePacks\\" + lvl.Remove(lvl.IndexOf('.')));

                //List the sub directories
                string[] packDirs = Directory.GetDirectories(Directory.GetCurrentDirectory());

                //Repeat
                //  Pick a random sub-directory and enter it
                //Until no more directories
                int DepthCount = 0;

                while (packDirs.Count() > 0 && DepthCount <= 5)
                {
                    Directory.SetCurrentDirectory(packDirs[_generator.Next(0, packDirs.Count())]);

                    packDirs = Directory.GetDirectories(Directory.GetCurrentDirectory());

                    DepthCount++;
                }

                //Apply each texture to the correct textile index
                for (int i = 0; i < _levelInstance.NumImages; i++)
                {
                    string textureFileName = lvl + i + ".png";

                    if (File.Exists(textureFileName))
                    {
                        _levelInstance.Images16[i].Pixels = T16Importer.ImportFrom32PNG(textureFileName);
                    }
                }

                //Restore app working directory
                Directory.SetCurrentDirectory(appDir);

                SaveLevel(_levelInstance, lvl);

                ReplacementStatusManager.LevelProgress++;
            }

            ReplacementStatusManager.LevelProgress = 0;
            ReplacementStatusManager.CanRandomize = true;
        }
    }
}
