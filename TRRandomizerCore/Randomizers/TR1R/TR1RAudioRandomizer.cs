using System.Numerics;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRGE.Core;
using TRLevelControl;
using TRLevelControl.Model;
using TRRandomizerCore.Levels;

namespace TRRandomizerCore.Randomizers;

public class TR1RAudioRandomizer : BaseTR1RRandomizer
{
    private const int _defaultSecretTrack = 13;

    private AudioRandomizer _audioRandomizer;

    public override void Randomize(int seed)
    {
        _generator = new(seed);

        LoadAudioData();

        foreach (TRRScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);

            RandomizeMusicTriggers(_levelInstance);
            RandomizeWibble(_levelInstance);

            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    private void LoadAudioData()
    {
        _audioRandomizer = new(ScriptEditor.AudioProvider.GetCategorisedTracks());
    }

    private void RandomizeMusicTriggers(TR1RCombinedLevel level)
    {
        FDControl floorData = new();
        floorData.ParseFromLevel(level.Data);

        if (Settings.ChangeTriggerTracks)
        {
            RandomizeFloorTracks(level.Data, floorData);
        }

        if (Settings.SeparateSecretTracks)
        {
            RandomizeSecretTracks(level, floorData);
        }        

        floorData.WriteToLevel(level.Data);
    }

    private void RandomizeFloorTracks(TR1Level level, FDControl floorData)
    {
        _audioRandomizer.ResetFloorMap();
        foreach (TR1Room room in level.Rooms.Where(r => !r.Flags.HasFlag(TRRoomFlag.Unused2)))
        {
            _audioRandomizer.RandomizeFloorTracks(room.Sectors, floorData, _generator, sectorIndex =>
            {
                return new Vector2
                (
                    TRConsts.Step2 + room.Info.X + sectorIndex / room.NumZSectors * TRConsts.Step4,
                    TRConsts.Step2 + room.Info.Z + sectorIndex % room.NumZSectors * TRConsts.Step4
                );
            });
        }
    }

    private void RandomizeSecretTracks(TR1RCombinedLevel level, FDControl floorData)
    {
        List<TRAudioTrack> secretTracks = _audioRandomizer.GetTracks(TRAudioCategory.Secret);

        for (int i = 0; i < level.Script.NumSecrets; i++)
        {
            TRAudioTrack secretTrack = secretTracks[_generator.Next(0, secretTracks.Count)];
            if (secretTrack.ID == _defaultSecretTrack)
            {
                continue;
            }

            FDActionItem musicAction = new()
            {
                TrigAction = FDTrigAction.PlaySoundtrack,
                Parameter = secretTrack.ID
            };

            List<FDTriggerEntry> triggers = FDUtilities.GetSecretTriggers(floorData, i);
            foreach (FDTriggerEntry trigger in triggers)
            {
                FDActionItem currentMusicAction = trigger.TrigActionList.Find(a => a.TrigAction == FDTrigAction.PlaySoundtrack);
                if (currentMusicAction == null)
                {
                    trigger.TrigActionList.Add(musicAction);
                }
            }
        }
    }

    private void RandomizeWibble(TR1RCombinedLevel level)
    {
        if (Settings.RandomizeWibble)
        {
            foreach (var (_, effect) in level.Data.SoundEffects)
            {
                effect.RandomizePitch = true;
            }
        }
    }
}
