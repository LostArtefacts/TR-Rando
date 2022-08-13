using System;
using TRGE.Core;

namespace TRRandomizerCore.Randomizers
{
    public class TR1HealthRandomizer : BaseTR1Randomizer
    {
        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            // For now we just set the global hitpoints value for lara. We could potentially adjust
            // meds per level based on difficulty.

            (ScriptEditor.Script as TR1Script).StartLaraHitpoints = _generator.Next((int)Settings.MinStartingHealth, (int)Settings.MaxStartingHealth + 1);
            ScriptEditor.SaveScript();

            TriggerProgress(Levels.Count);
        }
    }
}