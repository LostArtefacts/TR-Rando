using System.Collections.Generic;
using TRFDControl;
using TRFDControl.Utilities;
using TRGE.Core;
using TRLevelReader.Helpers;
using TRLevelReader.Model.Enums;

namespace TRRandomizerCore.Processors
{
    public class TR2EnemyAdjuster : TR2LevelProcessor
    {
        private static readonly Dictionary<string, List<int>> _enemyTargets = new Dictionary<string, List<int>>
        {
            [TR2LevelNames.OPERA] = new List<int> { 127 },
            [TR2LevelNames.MONASTERY] = new List<int> { 38, 39, 118 }
        };

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
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(_levelInstance.Data);

            foreach (int enemyIndex in _enemyTargets[_levelInstance.Name])
            {
                _levelInstance.Data.Entities[enemyIndex].TypeID = (short)TR2Entities.CameraTarget_N;
                FDUtilities.RemoveEntityTriggers(_levelInstance.Data, enemyIndex, floorData);
            }

            floorData.WriteToLevel(_levelInstance.Data);
        }
    }
}