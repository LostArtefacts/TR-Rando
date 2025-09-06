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
        ChangeGoldModels(level);
        AmendBirdMonster(level.Data);
        MoveBartoli(level.Data);
        RemoveEnemies(level);
    }

    private static void ChangeGoldModels(TR2CombinedLevel level)
    {
        if (!level.IsExpansion)
        {
            return;
        }

        ChangeModel(level.Data, TR2Type.Spider, TR2Type.Wolf);
        ChangeModel(level.Data, TR2Type.GiantSpider, TR2Type.Bear);
        ChangeModel(level.Data, TR2Type.MonkWithLongStick, TR2Type.MonkWithNoShadow);
    }

    private static void ChangeModel(TR2Level level, TR2Type oldType, TR2Type newType)
    {
        if (level.Models.ChangeKey(oldType, newType))
        {
            foreach (var item in level.Entities.Where(e => e.TypeID == oldType))
            {
                item.TypeID = newType;
            }
        }
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
