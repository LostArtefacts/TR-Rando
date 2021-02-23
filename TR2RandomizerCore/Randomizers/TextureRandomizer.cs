using System;
using System.IO;
using System.Linq;
using TRGE.Core;
using TRTexture16Importer;

namespace TR2RandomizerCore.Randomizers
{
    public class TextureRandomizer : RandomizerBase
    {
        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            foreach (TR23ScriptedLevel lvl in Levels)
            {
                if (SaveMonitor.IsCancelled) return;
                //SaveMonitor.FireSaveStateBeginning(TRSaveCategory.Custom, string.Format("Randomizing textures in {0}", lvl.Name));

                LoadLevelInstance(lvl);

                string appDir = Directory.GetCurrentDirectory();
                string lvlName = lvl.LevelFileBaseName.ToUpper();

                //Access texture packs directory for the specified level
                Directory.SetCurrentDirectory(@"Resources\TexturePacks\" + lvlName.Remove(lvlName.IndexOf('.')));

                try
                {
                    //List the sub directories
                    string[] packDirs = Directory.GetDirectories(Directory.GetCurrentDirectory());

                    //Repeat
                    //  Pick a random sub-directory and enter it
                    //Until no more directories
                    int DepthCount = 0;
                    while (packDirs.Count() > 0 && DepthCount <= 6)
                    {
                        Directory.SetCurrentDirectory(packDirs[_generator.Next(0, packDirs.Count())]);

                        packDirs = Directory.GetDirectories(Directory.GetCurrentDirectory());

                        DepthCount++;
                    }

                    //Apply each texture to the correct textile index
                    for (int i = 0; i < _levelInstance.NumImages; i++)
                    {
                        string textureFileName = lvlName + i + ".png";

                        if (File.Exists(textureFileName))
                        {
                            _levelInstance.Images16[i].Pixels = T16Importer.ImportFrom32PNG(textureFileName);
                        }
                    }

                    SaveLevelInstance();

                    SaveMonitor.FireSaveStateChanged(1);
                }
                finally
                {
                    //Restore app working directory
                    Directory.SetCurrentDirectory(appDir);
                }
            }
        }
    }
}