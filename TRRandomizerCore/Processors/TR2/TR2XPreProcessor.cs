using TRLevelControl.Model;
using TRRandomizerCore.Editors;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Processors.Shared;
using TRRandomizerCore.Processors.TR2;
using TRRandomizerCore.Processors.TR2.Tasks;
using TRRandomizerCore.Textures;

namespace TRRandomizerCore.Processors;

public class TR2XPreProcessor : TR2LevelProcessor
{
    private static readonly Version _minTR2XVersion = new(1, 4);

    public required TR2TextureMonitorBroker TextureMonitor { get; set; }
    public required ItemFactory<TR2Entity> ItemFactory { get; set; }
    public required RandomizerSettings Settings { get; set; }

    public void Run()
    {
        var tasks = new List<ITR2ProcessorTask>
        {
            new TR2XDeduplicationTask(),
            new TR2XSFXTask(),
            new TR2XDataTask() { TextureMonitor = TextureMonitor },
            new TR2XFixLaraTask() { TextureMonitor = TextureMonitor },
            new TR2XEnemyTask() { ItemFactory = ItemFactory },
            new TR2XPickupTask() { ReassignPuzzleItems = Settings.ReassignPuzzleItems },
            new TR2XFloorDataTask(),
        };

        var commonProcessor = new TRXCommonProcessor(ScriptEditor, _minTR2XVersion);
        commonProcessor.AdjustScript();

        Parallel.ForEach(Levels, (scriptedLevel, state) =>
        {
            commonProcessor.AdjustInjections(scriptedLevel);

            var level = LoadCombinedLevel(scriptedLevel);
            tasks.ForEach(t =>
            {
                t.Run(level);
                if (level.HasCutScene)
                {
                    t.Run(level.CutSceneLevel);
                }
            });

            SaveLevel(level);
            if (!TriggerProgress())
            {
                state.Break();
            }
        });

        SaveScript();
    }
}
