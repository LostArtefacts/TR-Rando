using TRRandomizerCore.Processors.Shared;
using TRRandomizerCore.Processors.TR2;
using TRRandomizerCore.Processors.TR2.Tasks;
using TRRandomizerCore.Textures;

namespace TRRandomizerCore.Processors;

public class TR2XPreProcessor : TR2LevelProcessor
{
    private static readonly Version _minTR2XVersion = new(1, 4);

    public required TR2TextureMonitorBroker TextureMonitor { get; set; }

    public void Run()
    {
        var tasks = new List<ITR2ProcessorTask>
        {
            new TR2XDeduplicationTask(),
            new TR2XDataTask() { TextureMonitor = TextureMonitor },
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
