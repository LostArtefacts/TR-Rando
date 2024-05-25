using Newtonsoft.Json;
using TRGE.Core;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.SFX;

namespace TRRandomizerCore.Randomizers;

public class TR1AudioAllocator : AudioAllocator
{
    private const int _defaultSecretTrack = 13;

    private List<TR1SFXDefinition> _soundEffects;
    private TR1SFXDefinition _psUziDefinition;
    private List<TRSFXGeneralCategory> _persistentCategories;

    public TR1AudioAllocator(IReadOnlyDictionary<TRAudioCategory, List<TRAudioTrack>> tracks)
        : base(tracks) { }

    public List<TR1SFXDefinition> GetDefinitions()
        => _soundEffects;

    public bool IsPersistent(TRSFXGeneralCategory category)
        => _persistentCategories.Contains(category);

    public TR1SFXDefinition GetPSUziDefinition()
        => _psUziDefinition;

    protected override string GetAssaultName()
        => TR1LevelNames.ASSAULT;

    protected override void LoadData(string backupPath)
    {
        // SFX in these categories can potentially remain as they are
        _persistentCategories = new()
        {
            TRSFXGeneralCategory.StandardWeaponFiring,
            TRSFXGeneralCategory.Ricochet,
            TRSFXGeneralCategory.Flying,
            TRSFXGeneralCategory.Explosion,
        };

        _soundEffects = JsonConvert.DeserializeObject<List<TR1SFXDefinition>>(File.ReadAllText(@"Resources\TR1\Audio\sfx.json"));

        // We don't want to store all SFX WAV data in JSON, so instead we reference the source level
        // and extract the details from there using the same format for model transport.
        Dictionary<string, TR1Level> levels = new();
        TR1LevelControl reader = new();
        foreach (TR1SFXDefinition definition in _soundEffects)
        {
            if (!levels.ContainsKey(definition.SourceLevel))
            {
                levels[definition.SourceLevel] = reader.Read(Path.Combine(backupPath, definition.SourceLevel));
            }

            TR1Level level = levels[definition.SourceLevel];
            definition.SoundEffect = level.SoundEffects[definition.InternalIndex];
        }

        // PS uzis need some manual setup. Make a copy of the standard uzi definition
        // then replace the sound data from the external wav file.
        TR1Level caves = levels[TR1LevelNames.CAVES];
        _psUziDefinition = new TR1SFXDefinition
        {
            SoundEffect = caves.SoundEffects[TR1SFX.LaraUziFire]
        };
        _psUziDefinition.SoundEffect.Samples = new() { File.ReadAllBytes(@"Resources\TR1\Audio\ps_uzis.wav") };
    }

    public void RandomizeMusicTriggers(TR1Level level)
    {
        RandomizeFloorTracks(level.Rooms, level.FloorData);
        if (!Settings.RandomizeSecrets)
        {
            RandomizeSecretTracks(level.FloorData, _defaultSecretTrack);
        }
    }
}
