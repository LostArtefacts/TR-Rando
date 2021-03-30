using System;
using System.Collections.Generic;
using TRGE.Core;
using TRTexture16Importer.Textures;
using TRTexture16Importer.Textures.Source;

namespace TR2RandomizerCore.Randomizers
{
    public class TextureRandomizer : RandomizerBase
    {
        private readonly Dictionary<AbstractTextureSource, string> _persistentVariants;

        public bool PersistVariants { get; set; }

        public TextureRandomizer()
        {
            _persistentVariants = new Dictionary<AbstractTextureSource, string>();

            PersistVariants = false;
        }

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
                            foreach (DynamicTextureSource source in levelMap.DynamicMapping.Keys)
                            {
                                levelMap.RedrawDynamicTargets(source, GetSourceVariant(source));
                            }

                            foreach (StaticTextureSource source in levelMap.StaticMapping.Keys)
                            {
                                levelMap.RedrawStaticTargets(source, GetSourceVariant(source));
                            }
                        }
                    }

                    SaveLevelInstance();

                    SaveMonitor.FireSaveStateChanged(1);
                }
            }
        }

        private string GetSourceVariant(AbstractTextureSource source)
        {
            if (PersistVariants && _persistentVariants.ContainsKey(source))
            {
                return _persistentVariants[source];
            }

            string[] variants = source.Variants;
            string variant = variants[_generator.Next(0, variants.Length)];

            if (PersistVariants)
            {
                _persistentVariants[source] = variant;
            }
            return variant;
        }
    }
}