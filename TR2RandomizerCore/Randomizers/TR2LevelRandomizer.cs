using System;
using System.Collections.Generic;
using System.Linq;
using TRGE.Coord;
using TRGE.Core;

namespace TR2RandomizerCore.Randomizers
{
    public class TR2LevelRandomizer : TR2LevelEditor
    {
        internal bool RandomizeSecrets { get; set; }
        internal bool RandomizeItems { get; set; }
        internal bool RandomizeEnemies { get; set; }
        internal bool RandomizeTextures { get; set; }

        internal int SecretSeed { get; set; }
        internal int ItemSeed { get; set; }
        internal int EnemySeed { get; set; }
        internal int TextureSeed { get; set; }

        internal bool HardSecrets { get; set; }

        internal TR2LevelRandomizer(TRDirectoryIOArgs args)
            : base(args) { }

        protected override void ApplyConfig(Config config)
        {
            RandomizeSecrets = config.GetBool("RandomizeSecrets");
            RandomizeItems = config.GetBool("RandomizeItems");
            RandomizeEnemies = config.GetBool("RandomizeEnemies");
            RandomizeTextures = config.GetBool("RandomizeTextures");

            int defaultSeed = int.Parse(DateTime.Now.ToString("yyyyMMdd"));
            SecretSeed = config.GetInt("SecretSeed", defaultSeed);
            ItemSeed = config.GetInt("ItemSeed", defaultSeed);
            EnemySeed = config.GetInt("EnemySeed", defaultSeed);
            TextureSeed = config.GetInt("TextureSeed", defaultSeed);

            HardSecrets = config.GetBool("HardSecrets");
        }

        protected override void StoreConfig(Config config)
        {
            config["RandomizeSecrets"] = RandomizeSecrets;
            config["RandomizeItems"] = RandomizeItems;
            config["RandomizeEnemies"] = RandomizeEnemies;
            config["RandomizeTextures"] = RandomizeTextures;

            config["SecretSeed"] = SecretSeed;
            config["ItemSeed"] = ItemSeed;
            config["EnemySeed"] = EnemySeed;
            config["TextureSeed"] = TextureSeed;

            config["HardSecrets"] = HardSecrets;
        }

        protected override int GetSaveTarget(int numLevels)
        {
            int target = base.GetSaveTarget(numLevels);
            if (RandomizeSecrets)  target += numLevels;
            if (RandomizeItems)    target += numLevels;
            if (RandomizeEnemies)  target += numLevels;
            if (RandomizeTextures) target += numLevels;
            return target;
        }

        /// <summary>
        /// The scripting randomization will be complete at this stage, so access to which levels
        /// are available, which are unarmed, ammoless etc is accurate at this point. The list of
        /// levels is always in the original sequence when accessed here, but can be sorted by level
        /// sequence if it's preferred to add an additional randomization factor. The save monitor
        /// is used to report progress and checks should be performed regularly on IsCancelled.
        /// </summary>
        protected override void SaveImpl(AbstractTRScriptEditor scriptEditor, TRSaveMonitor monitor)
        {
            List<TR23ScriptedLevel> levels = new List<TR23ScriptedLevel>
            (
                scriptEditor.ScriptedLevels.Cast<TR23ScriptedLevel>().ToList()
            );

            // Optionally sort based on randomized sequencing. Perhaps this should be an option for users?
            /*levels.Sort(delegate (TR23ScriptedLevel lvl1, TR23ScriptedLevel lvl2)
            {
                return lvl1.Sequence.CompareTo(lvl2.Sequence);
            });*/

            string wipDirectory = _io.WIPOutputDirectory.FullName;

            if (!monitor.IsCancelled && RandomizeSecrets)
            {
                monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing secrets");
                new SecretReplacer
                {
                    AllowHard = HardSecrets,
                    Levels = levels,
                    BasePath = wipDirectory,
                    SaveMonitor = monitor,
                }.Randomize(SecretSeed);
            }

            if (!monitor.IsCancelled && RandomizeItems)
            {
                monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing items");
                new ItemRandomizer
                {
                    Levels = levels,
                    BasePath = wipDirectory,
                    SaveMonitor = monitor
                }.Randomize(ItemSeed);
            }

            if (!monitor.IsCancelled && RandomizeEnemies)
            {
                monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing enemies");
                new EnemyRandomizer
                {
                    Levels = levels,
                    BasePath = wipDirectory,
                    SaveMonitor = monitor
                }.Randomize(EnemySeed);
            }

            if (!monitor.IsCancelled && RandomizeTextures)
            {
                monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing textures");
                new TextureRandomizer
                {
                    Levels = levels,
                    BasePath = wipDirectory,
                    SaveMonitor = monitor
                }.Randomize(TextureSeed);
            }
        }
    }
}