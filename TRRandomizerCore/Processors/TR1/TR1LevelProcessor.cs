using System.IO;
using TRRandomizerCore.Levels;
using TRGE.Core;
using TRLevelReader;
using TRLevelReader.Model;

namespace TRRandomizerCore.Processors
{
    public class TR1LevelProcessor : AbstractLevelProcessor<TR1ScriptedLevel, TR1CombinedLevel>
    {
        protected TR1LevelReader _reader;
        protected TR1LevelWriter _writer;

        public TR1LevelProcessor()
        {
            _reader = new TR1LevelReader();
            _writer = new TR1LevelWriter();
        }

        protected override TR1CombinedLevel LoadCombinedLevel(TR1ScriptedLevel scriptedLevel)
        {
            TR1CombinedLevel level = new TR1CombinedLevel
            {
                Data = LoadLevelData(scriptedLevel.LevelFileBaseName),
                Script = scriptedLevel
            };

            if (scriptedLevel.HasCutScene)
            {
                level.CutSceneLevel = LoadCombinedLevel(scriptedLevel.CutSceneLevel as TR1ScriptedLevel);
                level.CutSceneLevel.ParentLevel = level;
            }

            return level;
        }

        public TRLevel LoadLevelData(string name)
        {
            lock (_readLock)
            {
                string fullPath = Path.Combine(BasePath, name);
                return _reader.ReadLevel(fullPath);
            }
        }

        protected override void ReloadLevelData(TR1CombinedLevel level)
        {
            level.Data = LoadLevelData(level.Name);
            if (level.HasCutScene)
            {
                level.CutSceneLevel.Data = LoadLevelData(level.CutSceneLevel.Name);
            }
        }

        protected override void SaveLevel(TR1CombinedLevel level)
        {
            SaveLevel(level.Data, level.Name);
            if (level.HasCutScene)
            {
                SaveLevel(level.CutSceneLevel.Data, level.CutSceneLevel.Name);
            }

            SaveScript();
        }

        public void SaveLevel(TRLevel level, string name)
        {
            lock (_writeLock)
            {
                string fullPath = Path.Combine(BasePath, name);
                _writer.WriteLevelToFile(level, fullPath);
            }
        }
    }
}