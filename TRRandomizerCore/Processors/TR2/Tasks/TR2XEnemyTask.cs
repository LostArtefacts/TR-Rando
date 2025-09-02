using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;

namespace TRRandomizerCore.Processors.TR2.Tasks;

public class TR2XEnemyTask : ITR2ProcessorTask
{
    private static readonly Dictionary<string, List<int>> _unusedEnemies = new()
    {
        [TR2LevelNames.OPERA] = [127],
        [TR2LevelNames.MONASTERY] = [38, 39, 118],
    };

    public required ItemFactory<TR2Entity> ItemFactory { get; set; }

    public void Run(TR2CombinedLevel level)
    {
        AmendBirdMonster(level.Data);
        MoveBartoli(level.Data);
        RemoveEnemies(level);
    }

    private static void AmendBirdMonster(TR2Level level)
    {
        if (level.Models.TryGetValue(TR2Type.BirdMonster, out var birdMonster))
        {
            var endAnim = birdMonster.Animations[20];
            endAnim.Commands.Add(new TRFXCommand
            {
                EffectID = (short)TR2FX.EndLevel,
                FrameNumber = endAnim.FrameEnd,
            });
        }
    }

    private static void MoveBartoli(TR2Level level)
    {
        foreach (var item in level.Entities.Where(e => e.TypeID == TR2Type.MarcoBartoli))
        {
            item.X -= TRConsts.Step2;
            item.Z -= TRConsts.Step2;
        }
    }

    private void RemoveEnemies(TR2CombinedLevel level)
    {
        if (_unusedEnemies.TryGetValue(level.Name, out var enemies))
        {
            foreach (var itemIdx in enemies)
            {
                level.Data.Entities[itemIdx].TypeID = TR2Type.CameraTarget_N;
                level.Data.FloorData.RemoveEntityTriggers(itemIdx);
                ItemFactory.FreeItem(level.Name, itemIdx);
            }
        }
    }
}
