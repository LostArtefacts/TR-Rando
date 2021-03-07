using System;
using TRGE.Core;
using TRTexture16Importer.Textures;

namespace TR2RandomizerCore.Randomizers
{
    public class TextureRandomizer : RandomizerBase
    {
        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            // TODO: Split the processing into separate threads to boost performance

            using (TextureDatabase textureDatabase = new TextureDatabase())
            {
                foreach (TR23ScriptedLevel lvl in Levels)
                {
                    if (SaveMonitor.IsCancelled) return;
                    //SaveMonitor.FireSaveStateBeginning(TRSaveCategory.Custom, string.Format("Randomizing textures in {0}", lvl.Name));

                    LoadLevelInstance(lvl);

                    string lvlName = lvl.LevelFileBaseName.ToUpper();

                    using (TextureLevelMapping levelMap = TextureLevelMapping.Get(_levelInstance, lvlName, textureDatabase))
                    {
                        if (levelMap != null)
                        {
                            foreach (TextureSource source in levelMap.Mapping.Keys)
                            {
                                string[] availableTextures = source.Textures;
                                int rand = _generator.Next(-1, availableTextures.Length);
                                if (rand == -1)
                                {
                                    // Leave the standard texture for this particular source. Possibly make a UI option
                                    // so some defaults can remain, or otherwise that everything possible should be changed.
                                    continue;
                                }
                                
                                levelMap.RedrawTargets(source, availableTextures[rand]);
                            }
                        }
                    }

                    SaveLevelInstance();

                    SaveMonitor.FireSaveStateChanged(1);
                }
            }
        }
    }
}