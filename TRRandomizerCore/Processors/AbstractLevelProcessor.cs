﻿using System.Runtime.ExceptionServices;
using TRGE.Core;
using TRLevelControl.Model;
using TRLevelControl;

namespace TRRandomizerCore.Processors;

public abstract class AbstractLevelProcessor<S, C> : ILevelProcessor where S : AbstractTRScriptedLevel
{
    protected uint _maxThreads;
    protected C _levelInstance; // Combined script/data level

    protected ExceptionDispatchInfo _processingException;

    protected readonly object _controlLock, _monitorLock;

    internal AbstractTRScriptEditor ScriptEditor { get; set; }
    internal List<S> Levels { get; set; }
    internal TRSaveMonitor SaveMonitor;

    // The WIP folder where levels are saved before being copied to the target at the end of the process
    public string BasePath { get; set; }
    // The backup folder that contains the untouched levels from when the user first opened the folder
    public string BackupPath { get; set; }

    public AbstractLevelProcessor()
    {
        _controlLock = new();
        _monitorLock = new();

        _maxThreads = 3;
    }

    protected void Process(Action<C> processAction)
    {
        foreach (S lvl in Levels)
        {
            LoadLevelInstance(lvl);
            processAction(_levelInstance);
            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    protected void LoadLevelInstance(S scriptedLevel)
    {
        _levelInstance = LoadCombinedLevel(scriptedLevel);
    }

    protected abstract C LoadCombinedLevel(S scriptedLevel);

    protected void ReloadLevelInstanceData()
    {
        ReloadLevelData(_levelInstance);
    }

    protected abstract void ReloadLevelData(C level);     

    protected void SaveLevelInstance()
    {
        SaveLevel(_levelInstance);
    }

    protected abstract void SaveLevel(C level);

    public TRGData LoadTRGData(string name)
    {
        lock (_controlLock)
        {
            string fullPath = Path.Combine(BasePath, name);
            return File.Exists(fullPath) ? TRGControlBase.Read(fullPath) : null;
        }
    }

    public void SaveTRGData(TRGData data, string name)
    {
        if (data == null)
        {
            return;
        }

        lock (_controlLock)
        {
            string fullPath = Path.Combine(BasePath, name);
            TRGControlBase.Write(data, fullPath);
        }
    }

    protected void SaveScript()
    {
        lock (_controlLock)
        {
            // Save any script changes.
            ScriptEditor.SaveScript();
        }
    }

    /// <summary>
    /// Informs the save monitor to update its progress by 1.
    /// </summary>
    /// <returns>True if the save process has not been cancelled and the current error state is null.</returns>
    public bool TriggerProgress(int progress = 1)
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

    internal void SetWarning(string text)
    {
        lock (_monitorLock)
        {
            SaveMonitor.FireSaveStateChanged(category: TRSaveCategory.Warning, customDescription: text);
        }
    }

    public void HandleException(Exception e)
    {
        lock (_monitorLock)
        {
            _processingException ??= ExceptionDispatchInfo.Capture(e);
        }
    }

    protected bool ResourceExists(string filePath)
    {
        return File.Exists(GetResourcePath(filePath));
    }

    protected string GetResourcePath(string filePath)
    {
        return Path.Combine("Resources", filePath);
    }

    protected string ReadResource(string filePath)
    {
        return File.ReadAllText(GetResourcePath(filePath));
    }

    public string GetBackupChecksum(string filePath)
    {
        string fullPath = Path.Combine(BackupPath, filePath);
        return new FileInfo(fullPath).Checksum();
    }
}
