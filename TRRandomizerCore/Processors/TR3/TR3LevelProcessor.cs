using System.IO;
using TRRandomizerCore.Levels;
using TRGE.Core;
using TRLevelReader;
using TRLevelReader.Model;

namespace TRRandomizerCore.Processors
{
    public class TR3LevelProcessor : AbstractLevelProcessor<TR3ScriptedLevel, TR3CombinedLevel>
    {
        /// <summary>
        /// EU version has 7 RPLs, JP has 5. This allows for determining if different mods are needed per level.
        /// </summary>
        public bool IsJPVersion => (ScriptEditor.Script as TR23Script).NumRPLs == 5;

        protected TR3LevelReader _reader;
        protected TR3LevelWriter _writer;

        public TR3LevelProcessor()
        {
            _reader = new TR3LevelReader();
            _writer = new TR3LevelWriter();
        }

        protected override TR3CombinedLevel LoadCombinedLevel(TR3ScriptedLevel scriptedLevel)
        {
            TR3CombinedLevel level = new TR3CombinedLevel
            {
                Data = LoadLevelData(scriptedLevel.LevelFileBaseName),
                Script = scriptedLevel
            };

            if (scriptedLevel.HasCutScene)
            {
                level.CutSceneLevel = LoadCombinedLevel(scriptedLevel.CutSceneLevel as TR3ScriptedLevel);
                level.CutSceneLevel.ParentLevel = level;
            }

            return level;
        }

        public TR3Level LoadLevelData(string name)
        {
            lock (_readLock)
            {
                string fullPath = Path.Combine(BasePath, name);
                return _reader.ReadLevel(fullPath);
            }
        }

        protected override void ReloadLevelData(TR3CombinedLevel level)
        {
            level.Data = LoadLevelData(level.Name);
            if (level.HasCutScene)
            {
                level.CutSceneLevel.Data = LoadLevelData(level.CutSceneLevel.Name);
            }
        }

        protected override void SaveLevel(TR3CombinedLevel level)
        {
            SaveLevel(level.Data, level.Name);
            if (level.HasCutScene)
            {
                SaveLevel(level.CutSceneLevel.Data, level.CutSceneLevel.Name);
            }

            SaveScript();
        }

        public void SaveLevel(TR3Level level, string name)
        {
            lock (_writeLock)
            {
                string fullPath = Path.Combine(BasePath, name);
                _writer.WriteLevelToFile(level, fullPath);
            }
        }
    }
}