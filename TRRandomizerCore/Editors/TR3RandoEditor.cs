using System.Collections.Generic;
using System.Linq;
using TRGE.Coord;
using TRGE.Core;

namespace TRRandomizerCore.Editors
{
    public class TR3RandoEditor : TR3LevelEditor, ISettingsProvider
    {
        public RandomizerSettings Settings { get; private set; }

        public TR3RandoEditor(TRDirectoryIOArgs args, TREdition edition)
            : base(args, edition) { }

        protected override void ApplyConfig(Config config)
        {
            Settings = new RandomizerSettings();
            Settings.ApplyConfig(config);
        }

        protected override void StoreConfig(Config config)
        {
            Settings.StoreConfig(config);
        }

        protected override int GetSaveTarget(int numLevels)
        {
            return base.GetSaveTarget(numLevels);// + Settings.GetSaveTarget(numLevels);
        }

        protected override void SaveImpl(AbstractTRScriptEditor scriptEditor, TRSaveMonitor monitor)
        {
            List<TR23ScriptedLevel> levels = new List<TR23ScriptedLevel>
            (
                scriptEditor.EnabledScriptedLevels.Cast<TR23ScriptedLevel>().ToList()
            );

            if (scriptEditor.GymAvailable)
            {
                levels.Add(scriptEditor.AssaultLevel as TR23ScriptedLevel);
            }

            // Each processor will have a reference to the script editor, so can
            // make on-the-fly changes as required.
            TR23ScriptEditor tr23ScriptEditor = scriptEditor as TR23ScriptEditor;
            // string wipDirectory = _io.WIPOutputDirectory.FullName;

            if (Settings.DevelopmentMode)
            {
                (tr23ScriptEditor.Script as TR23Script).LevelSelectEnabled = true;
                scriptEditor.SaveScript();
            }

            // TODO: Randomize...
        }
    }
}