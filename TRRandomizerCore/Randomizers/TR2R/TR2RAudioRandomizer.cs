using Newtonsoft.Json;
using System.Numerics;
using TRGE.Core;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.SFX;

namespace TRRandomizerCore.Randomizers;

public class TR2RAudioRandomizer : BaseTR2RRandomizer
{
    private AudioRandomizer _audioRandomizer;

    private List<TR2SFXDefinition> _soundEffects;
    private List<TRSFXGeneralCategory> _sfxCategories;
    private List<TRRScriptedLevel> _uncontrolledLevels;

    public override void Randomize(int seed)
    {
        _generator = new Random(seed);

        LoadAudioData();
        ChooseUncontrolledLevels();

        foreach (TRRScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);

            RandomizeMusicTriggers(_levelInstance);
            RandomizeSoundEffects(_levelInstance);
            RandomizeWibble(_levelInstance);

            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    private void ChooseUncontrolledLevels()
    {
        TRRScriptedLevel assaultCourse = Levels.Find(l => l.Is(TR2LevelNames.ASSAULT));
        HashSet<TRRScriptedLevel> exlusions = new() { assaultCourse };

        _uncontrolledLevels = Levels.RandomSelection(_generator, (int)Settings.UncontrolledSFXCount, exclusions: exlusions);
        if (Settings.AssaultCourseWireframe)
        {
            _uncontrolledLevels.Add(assaultCourse);
        }
    }

    public bool IsUncontrolledLevel(TRRScriptedLevel level)
    {
        return _uncontrolledLevels.Contains(level);
    }

    private void RandomizeMusicTriggers(TR2RCombinedLevel level)
    {
        FDControl floorData = new();
        floorData.ParseFromLevel(level.Data);

        if (Settings.ChangeTriggerTracks)
        {
            RandomizeFloorTracks(level.Data, floorData);
        }

        if (Settings.SeparateSecretTracks)
        {
            RandomizeSecretTracks(level.Data, floorData);
        }

        floorData.WriteToLevel(level.Data);
    }

    private void RandomizeFloorTracks(TR2Level level, FDControl floorData)
    {
        _audioRandomizer.ResetFloorMap();
        foreach (TR2Room room in level.Rooms)
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

    private void RandomizeSecretTracks(TR2Level level, FDControl floorData)
    {
        List<TRAudioTrack> secretTracks = _audioRandomizer.GetTracks(TRAudioCategory.Secret);
        Dictionary<int, TR2Entity> secrets = GetSecretItems(level);
        foreach (int entityIndex in secrets.Keys)
        {
            TR2Entity secret = secrets[entityIndex];
            TRRoomSector sector = FDUtilities.GetRoomSector(secret.X, secret.Y, secret.Z, secret.Room, level, floorData);
            if (sector.FDIndex == 0)
            {
                floorData.CreateFloorData(sector);
            }

            List<FDEntry> entries = floorData.Entries[sector.FDIndex];
            FDTriggerEntry existingTriggerEntry = entries.Find(e => e is FDTriggerEntry) as FDTriggerEntry;
            bool existingEntityPickup = false;
            if (existingTriggerEntry != null)
            {
                if (existingTriggerEntry.TrigType == FDTrigType.Pickup && existingTriggerEntry.TrigActionList[0].Parameter == entityIndex)
                {
                    existingEntityPickup = true;
                }
                else
                {
                    continue;
                }
            }

            FDActionItem musicAction = new()
            {
                TrigAction = FDTrigAction.PlaySoundtrack,
                Parameter = secretTracks[_generator.Next(0, secretTracks.Count)].ID
            };

            if (existingEntityPickup)
            {
                existingTriggerEntry.TrigActionList.Add(musicAction);
            }
            else
            {
                entries.Add(new FDTriggerEntry
                {
                    Setup = new FDSetup { Value = 1028 },
                    TrigSetup = new FDTrigSetup { Value = 15872 },
                    TrigActionList = new List<FDActionItem>
                    {
                        new() {
                            TrigAction = FDTrigAction.Object,
                            Parameter = (ushort)entityIndex
                        },
                        musicAction
                    }
                });
            }
        }
    }

    private static Dictionary<int, TR2Entity> GetSecretItems(TR2Level level)
    {
        Dictionary<int, TR2Entity> entities = new();
        for (int i = 0; i < level.Entities.Count; i++)
        {
            if (TR2TypeUtilities.IsSecretType(level.Entities[i].TypeID))
            {
                entities[i] = level.Entities[i];
            }
        }

        return entities;
    }

    private void LoadAudioData()
    {
        _audioRandomizer = new AudioRandomizer(ScriptEditor.AudioProvider.GetCategorisedTracks());
        _sfxCategories = AudioRandomizer.GetSFXCategories(Settings);
        if (_sfxCategories.Count > 0)
        {
            _soundEffects = JsonConvert.DeserializeObject<List<TR2SFXDefinition>>(ReadResource(@"TR2\Audio\sfx.json"));

            Dictionary<string, TR2Level> levels = new();
            TR2LevelControl reader = new();
            foreach (TR2SFXDefinition definition in _soundEffects)
            {
                if (!levels.ContainsKey(definition.SourceLevel))
                {
                    levels[definition.SourceLevel] = reader.Read(Path.Combine(BackupPath, definition.SourceLevel));
                }

                TR2Level level = levels[definition.SourceLevel];
                definition.SoundEffect = level.SoundEffects[definition.InternalIndex];
            }
        }
    }

    private void RandomizeSoundEffects(TR2RCombinedLevel level)
    {
        if (_sfxCategories.Count == 0)
        {
            return;
        }

        if (IsUncontrolledLevel(level.Script))
        {
            int maxSample = Enum.GetValues<TR2SFX>().Length;            
            HashSet<uint> indices = new();
            foreach (var (_, effect) in level.Data.SoundEffects)
            {
                for (int i = 0; i < effect.Samples.Count; i++)
                {
                    uint sample;
                    do
                    {
                        sample = (uint)_generator.Next(0, maxSample + 1);
                    }
                    while (!indices.Add(sample));
                    effect.Samples[i] = sample;
                }
            }
        }
        else
        {
            foreach (TR2SFX internalIndex in Enum.GetValues<TR2SFX>())
            {
                TR2SFXDefinition definition = _soundEffects.Find(sfx => sfx.InternalIndex == internalIndex);
                if (!level.Data.SoundEffects.ContainsKey(internalIndex) || definition == null
                    || definition.Creature == TRSFXCreatureCategory.Lara || !_sfxCategories.Contains(definition.PrimaryCategory))
                {
                    continue;
                }

                Predicate<TR2SFXDefinition> pred;
                if (Settings.LinkCreatureSFX && definition.Creature > TRSFXCreatureCategory.Lara)
                {
                    pred = sfx =>
                    {
                        return sfx.Categories.Contains(definition.PrimaryCategory) &&
                        sfx != definition &&
                        (
                            sfx.Creature == definition.Creature ||
                            (sfx.Creature == TRSFXCreatureCategory.Lara && definition.Creature == TRSFXCreatureCategory.Human)
                        );
                    };
                }
                else
                {
                    pred = sfx => sfx.Categories.Contains(definition.PrimaryCategory) && sfx != definition;
                }

                List<TR2SFXDefinition> otherDefinitions = _soundEffects.FindAll(pred);
                if (otherDefinitions.Count > 0)
                {
                    TR2SFXDefinition nextDefinition = otherDefinitions[_generator.Next(0, otherDefinitions.Count)];
                    if (nextDefinition != definition)
                    {
                        level.Data.SoundEffects[internalIndex] = nextDefinition.SoundEffect;
                    }
                }
            }
        }
    }

    private void RandomizeWibble(TR2RCombinedLevel level)
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
