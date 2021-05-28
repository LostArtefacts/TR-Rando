using System;
using System.Collections.Generic;
using System.IO;
using TR2RandomizerCore.Helpers;
using TRGE.Core;
using TRLevelReader;
using TRLevelReader.Model;

namespace TR2RandomizerCore.Processors
{
    public class LevelProcessor
    {
        protected uint _maxThreads;

        protected TR2LevelReader _reader;
        protected TR2LevelWriter _writer;
        protected TR2CombinedLevel _levelInstance;

        protected Exception _processingException;

        protected readonly object _readLock, _writeLock, _monitorLock;

        internal List<TR23ScriptedLevel> Levels { get; set; }
        internal TRSaveMonitor SaveMonitor;

        public string BasePath { get; set; }

        public LevelProcessor()
        {
            _reader = new TR2LevelReader();
            _writer = new TR2LevelWriter();

            _readLock = new object();
            _writeLock = new object();
            _monitorLock = new object();

            _maxThreads = 3;
        }

        protected void LoadLevelInstance(TR23ScriptedLevel scriptedLevel)
        {
            _levelInstance = LoadCombinedLevel(scriptedLevel);
        }

        protected void ReloadLevelInstanceData()
        {
            _levelInstance.Data = LoadLevelData(_levelInstance.Name);
        }

        protected void ReloadLevelData(TR2CombinedLevel level)
        {
            level.Data = LoadLevelData(level.Name);
        }

        public TR2CombinedLevel LoadCombinedLevel(TR23ScriptedLevel scriptedLevel)
        {
            return new TR2CombinedLevel
            {
                Data = LoadLevelData(scriptedLevel.LevelFileBaseName),
                Script = scriptedLevel
            };
        }

        public TR2Level LoadLevelData(string name)
        {
            lock (_readLock)
            {
                string fullPath = Path.Combine(BasePath, name);
                return _reader.ReadLevel(fullPath);
            }
        }

        protected void SaveLevelInstance()
        {
            SaveLevel(_levelInstance);
        }

        protected void SaveLevel(TR2CombinedLevel level)
        {
            SaveLevel(level.Data, level.Name);
        }

        public void SaveLevel(TR2Level level, string name)
        {
            lock (_writeLock)
            {
                string fullPath = Path.Combine(BasePath, name);
                _writer.WriteLevelToFile(level, fullPath);
            }
        }

        /// <summary>
        /// Informs the save monitor to update its progress by 1.
        /// </summary>
        /// <returns>True if the save process has not been cancelled and the current error state is null.</returns>
        internal bool TriggerProgress(int progress = 1)
        {
            lock (_monitorLock)
            {
                SaveMonitor.FireSaveStateChanged(progress);
                return !SaveMonitor.IsCancelled && _processingException == null;
            }
        }

        internal void SetMessage(string text)
        {
            lock (_monitorLock)
            {
                SaveMonitor.FireSaveStateChanged(customDescription: text);
            }
        }

        internal void HandleException(Exception e)
        {
            lock (_monitorLock)
            {
                if (_processingException == null)
                {
                    _processingException = e;
                }
            }
        }
    }
}