using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Processors;

public class TR2EnemyAdjuster : TR2LevelProcessor
{
    private static readonly Dictionary<string, List<int>> _enemyTargets = new()
    {
        [TR2LevelNames.OPERA] = new List<int> { 127 },
        [TR2LevelNames.MONASTERY] = new List<int> { 38, 39, 118 }
    };

    public ItemFactory<TR2Entity> ItemFactory { get; set; }

    public void AdjustEnemies()
    {
        foreach (TR2ScriptedLevel lvl in Levels)
        {
            if (_enemyTargets.ContainsKey(lvl.LevelFileBaseName.ToUpper()))
            {
                LoadLevelInstance(lvl);

                AdjustInstanceEnemies();

                SaveLevelInstance();
            }
            
            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    private void AdjustInstanceEnemies()
    {
        FDControl floorData = new();
        floorData.ParseFromLevel(_levelInstance.Data);

        foreach (int enemyIndex in _enemyTargets[_levelInstance.Name])
        {
            _levelInstance.Data.Entities[enemyIndex].TypeID = TR2Type.CameraTarget_N;
            FDUtilities.RemoveEntityTriggers(_levelInstance.Data, enemyIndex, floorData);
            ItemFactory?.FreeItem(_levelInstance.Name, enemyIndex);
        }

        floorData.WriteToLevel(_levelInstance.Data);
    }
}
