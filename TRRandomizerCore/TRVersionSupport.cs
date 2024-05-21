using TRGE.Core;

namespace TRRandomizerCore;

internal class TRVersionSupport
{
    private static readonly List<TRRandomizerType> _tr1Types = new()
    {
        TRRandomizerType.AmbientTracks,
        TRRandomizerType.Ammoless,
        TRRandomizerType.AtlanteanEggBehaviour,
        TRRandomizerType.Audio,
        TRRandomizerType.Braid,
        TRRandomizerType.ChallengeRooms,
        TRRandomizerType.ClonedEnemies,
        TRRandomizerType.DisableDemos,
        TRRandomizerType.DynamicTextures,
        TRRandomizerType.DynamicEnemyTextures,
        TRRandomizerType.Enemy,
        TRRandomizerType.Environment,
        TRRandomizerType.ExtraPickups,
        TRRandomizerType.GameMode,
        TRRandomizerType.GeneralBugFixes,
        TRRandomizerType.GymOutfit,
        TRRandomizerType.GlitchedSecrets,
        TRRandomizerType.HardEnvironment,
        TRRandomizerType.HardSecrets,
        TRRandomizerType.Health,
        TRRandomizerType.HiddenEnemies,
        TRRandomizerType.Item,
        TRRandomizerType.ItemDrops,
        TRRandomizerType.ItemSprite,
        TRRandomizerType.KeyItems,
        TRRandomizerType.KeyItemTextures,
        TRRandomizerType.LarsonBehaviour,
        TRRandomizerType.LevelCount,
        TRRandomizerType.LevelSequence,
        TRRandomizerType.Mediless,
        TRRandomizerType.MeshSwaps,
        TRRandomizerType.NightMode,
        TRRandomizerType.Outfit,
        TRRandomizerType.ReturnPaths,
        TRRandomizerType.RewardRooms,
        TRRandomizerType.Secret,
        TRRandomizerType.SecretCount,
        TRRandomizerType.SecretModels,
        TRRandomizerType.SecretReward,
        TRRandomizerType.SecretTextures,
        TRRandomizerType.ShortcutFixes,
        TRRandomizerType.SFX,
        TRRandomizerType.StartPosition,
        TRRandomizerType.Text,
        TRRandomizerType.Texture,
        TRRandomizerType.Traps,
        TRRandomizerType.Unarmed,
        TRRandomizerType.WaterColour,
    };

    private static readonly List<TRRandomizerType> _tr1RTypes = new()
    {
        TRRandomizerType.AtlanteanEggBehaviour,
        TRRandomizerType.Audio,
        TRRandomizerType.Enemy,
        TRRandomizerType.GlitchedSecrets,
        TRRandomizerType.HardSecrets,
        TRRandomizerType.HiddenEnemies,
        TRRandomizerType.Item,
        TRRandomizerType.KeyItems,
        TRRandomizerType.Secret,
        TRRandomizerType.SecretAudio,
        TRRandomizerType.SecretReward,
        TRRandomizerType.StartPosition,
    };

    private static readonly List<TRRandomizerType> _tr2Types = new()
    {
        TRRandomizerType.AmbientTracks,
        TRRandomizerType.Ammoless,
        TRRandomizerType.Audio,
        TRRandomizerType.BirdMonsterBehaviour,
        TRRandomizerType.Braid,
        TRRandomizerType.DocileBirdMonster,
        TRRandomizerType.DisableDemos,
        TRRandomizerType.DragonSpawn,
        TRRandomizerType.DynamicEnemyTextures,
        TRRandomizerType.DynamicTextures,
        TRRandomizerType.Enemy,
        TRRandomizerType.Environment,
        TRRandomizerType.GeneralBugFixes,
        TRRandomizerType.GlitchedSecrets,
        TRRandomizerType.HardSecrets,
        TRRandomizerType.Item,
        TRRandomizerType.ItemDrops,
        TRRandomizerType.KeyContinuity,
        TRRandomizerType.KeyItems,
        TRRandomizerType.KeyItemTextures,
        TRRandomizerType.Ladders,
        TRRandomizerType.LevelCount,
        TRRandomizerType.LevelSequence,
        TRRandomizerType.MeshSwaps,
        TRRandomizerType.NightMode,
        TRRandomizerType.Outfit,
        TRRandomizerType.OutfitDagger,
        TRRandomizerType.ReturnPaths,
        TRRandomizerType.Secret,
        TRRandomizerType.SecretAudio,
        TRRandomizerType.SecretReward,
        TRRandomizerType.SecretTextures,
        TRRandomizerType.SFX,
        TRRandomizerType.ShortcutFixes,
        TRRandomizerType.StartPosition,
        TRRandomizerType.Sunset,
        TRRandomizerType.Text,
        TRRandomizerType.Texture,
        TRRandomizerType.Unarmed,
        TRRandomizerType.ItemSprite
    };

    private static readonly List<TRRandomizerType> _tr2RTypes = new()
    {
        TRRandomizerType.Audio,
        TRRandomizerType.BirdMonsterBehaviour,
        TRRandomizerType.Enemy,
        TRRandomizerType.GlitchedSecrets,
        TRRandomizerType.HardSecrets,
        TRRandomizerType.Item,
        TRRandomizerType.ItemDrops,
        TRRandomizerType.KeyItems,
        TRRandomizerType.Secret,
        TRRandomizerType.SecretAudio,
        TRRandomizerType.SFX,
        TRRandomizerType.StartPosition,
    };

    private static readonly List<TRRandomizerType> _tr3Types = new()
    {
        TRRandomizerType.AmbientTracks,
        TRRandomizerType.Ammoless,
        TRRandomizerType.Audio,
        TRRandomizerType.Braid,
        TRRandomizerType.DynamicEnemyTextures,
        TRRandomizerType.Enemy,
        TRRandomizerType.Environment,
        TRRandomizerType.GeneralBugFixes,
        TRRandomizerType.GlitchedSecrets,
        TRRandomizerType.GlobeDisplay,
        TRRandomizerType.HardSecrets,
        TRRandomizerType.Item,
        TRRandomizerType.ItemDrops,
        TRRandomizerType.KeyItems,
        TRRandomizerType.Ladders,
        TRRandomizerType.LevelCount,
        TRRandomizerType.LevelSequence,
        TRRandomizerType.NightMode,
        TRRandomizerType.Outfit,
        TRRandomizerType.ReturnPaths,
        TRRandomizerType.RewardRooms,
        TRRandomizerType.Secret,
        TRRandomizerType.SecretAudio,
        TRRandomizerType.SecretReward,
        TRRandomizerType.SecretTextures,
        TRRandomizerType.SFX,
        TRRandomizerType.ShortcutFixes,
        TRRandomizerType.StartPosition,
        TRRandomizerType.Text,
        TRRandomizerType.Texture,
        TRRandomizerType.Unarmed,
        TRRandomizerType.VFX
    };

    private static readonly List<TRRandomizerType> _tr3MainTypes = new()
    {
        TRRandomizerType.Weather
    };

    private static readonly List<TRRandomizerType> _tr3RTypes = new()
    {
        TRRandomizerType.Audio,
        TRRandomizerType.GlitchedSecrets,
        TRRandomizerType.HardSecrets,
        TRRandomizerType.Item,
        TRRandomizerType.ItemDrops,
        TRRandomizerType.KeyItems,
        TRRandomizerType.Secret,
        TRRandomizerType.SecretAudio,
        TRRandomizerType.SecretReward,
        TRRandomizerType.SFX,
        TRRandomizerType.StartPosition,
    };

    private static readonly Dictionary<TRVersion, TRVersionSupportGroup> _supportedTypes = new()
    {
        [TRVersion.TR1] = new()
        {
            DefaultSupport = _tr1Types,
            RemasterSupport = _tr1RTypes,
        },
        [TRVersion.TR2] = new()
        {
            DefaultSupport = _tr2Types,
            RemasterSupport = _tr2RTypes,
        },
        [TRVersion.TR3] = new()
        {
            DefaultSupport = _tr3Types,
            PatchSupport = _tr3MainTypes,
            RemasterSupport = _tr3RTypes,
        }
    };

    private static readonly string _trrExe = "tomb123.exe";
    private static readonly Dictionary<TRVersion, List<string>> _versionExes = new()
    {
        [TRVersion.TR1] = new() { "TR1X.exe" },
        [TRVersion.TR2] = new() { "Tomb2.exe" },
        [TRVersion.TR3] = new() { "Tomb3.exe" }
    };

    public static bool IsRandomizationSupported(TREdition edition)
    {
        return _supportedTypes.ContainsKey(edition.Version);
    }

    public static bool IsRandomizationSupported(TREdition edition, TRRandomizerType randomizerType)
    {
        if (!IsRandomizationSupported(edition))
        {
            return false;
        }

        TRVersion version = edition.Version;
        TRVersionSupportGroup supportGroup = _supportedTypes[version];
        bool supported = supportGroup.DefaultSupport.Contains(randomizerType);

        // If not supported but we're using a community patch, does that support it?
        if (!supported && edition.IsCommunityPatch && supportGroup.HasPatchSupport)
        {
            supported = supportGroup.PatchSupport.Contains(randomizerType);
        }

        // More limited in the Remasters
        if (edition.Remastered)
        {
            supported = supportGroup.HasRemasterSupport
                && supportGroup.RemasterSupport.Contains(randomizerType);
        }

        return supported;
    }

    public static List<string> GetExecutables(TREdition edition, string dataFolder)
    {
        List<string> exes = new();
        if (edition.Remastered)
        {
            exes.Add(Path.GetFullPath(Path.Combine(dataFolder, "../../", _trrExe)));
        }
        else if (_versionExes.ContainsKey(edition.Version))
        {
            exes.AddRange(_versionExes[edition.Version].Select(
                p => Path.GetFullPath(Path.Combine(dataFolder, "../", p))));
        }
        return exes;
    }
}

internal class TRVersionSupportGroup
{
    internal List<TRRandomizerType> DefaultSupport { get; set; }
    internal List<TRRandomizerType> PatchSupport { get; set; }
    internal List<TRRandomizerType> RemasterSupport { get; set; }
    internal bool HasPatchSupport => PatchSupport != null;
    internal bool HasRemasterSupport => RemasterSupport != null;
}
