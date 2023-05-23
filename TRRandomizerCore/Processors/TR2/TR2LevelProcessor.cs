using System.IO;
using TRRandomizerCore.Levels;
using TRGE.Core;
using TRLevelReader;
using TRLevelReader.Model;

namespace TRRandomizerCore.Processors
{
    public class TR2LevelProcessor : AbstractLevelProcessor<TR2ScriptedLevel, TR2CombinedLevel>
    {
        protected TR2LevelReader _reader;
        protected TR2LevelWriter _writer;

        public TR2LevelProcessor()
        {
            _reader = new TR2LevelReader();
            _writer = new TR2LevelWriter();
        }

        protected override TR2CombinedLevel LoadCombinedLevel(TR2ScriptedLevel scriptedLevel)
        {
            TR2CombinedLevel level = new TR2CombinedLevel
            {
                Data = LoadLevelData(scriptedLevel.LevelFileBaseName),
                Script = scriptedLevel,
                Checksum = GetBackupChecksum(scriptedLevel.LevelFileBaseName)
            };

            if (scriptedLevel.HasCutScene)
            {
                level.CutSceneLevel = LoadCombinedLevel(scriptedLevel.CutSceneLevel as TR2ScriptedLevel);
                level.CutSceneLevel.ParentLevel = level;
            }

            return level;
        }

        public TR2Level LoadLevelData(string name)
        {
            lock (_readLock)
            {
                string fullPath = Path.Combine(BasePath, name);
                return _reader.ReadLevel(fullPath);
            }
        }

        protected override void ReloadLevelData(TR2CombinedLevel level)
        {
            level.Data = LoadLevelData(level.Name);
            if (level.HasCutScene)
            {
                level.CutSceneLevel.Data = LoadLevelData(level.CutSceneLevel.Name);
            }
        }

        protected override void SaveLevel(TR2CombinedLevel level)
        {
            SaveLevel(level.Data, level.Name);
            if (level.HasCutScene)
            {
                SaveLevel(level.CutSceneLevel.Data, level.CutSceneLevel.Name);
            }

            SaveScript();
        }

        public void SaveLevel(TR2Level level, string name)
        {
            lock (_writeLock)
            {
                string fullPath = Path.Combine(BasePath, name);
                _writer.WriteLevelToFile(level, fullPath);
            }
        }
    }
}