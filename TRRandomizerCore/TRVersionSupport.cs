using System.Collections.Generic;
using TRGE.Core;

namespace TRRandomizerCore
{
    internal class TRVersionSupport
    {
        private static readonly List<TRRandomizerType> _tr1Types = new List<TRRandomizerType>
        {
            TRRandomizerType.AtlanteanEggBehaviour,
            TRRandomizerType.Audio,
            TRRandomizerType.DynamicTextures,
            TRRandomizerType.Enemy,
            TRRandomizerType.Environment,
            TRRandomizerType.ExtraPickups,
            TRRandomizerType.GymOutfit,
            TRRandomizerType.GlitchedSecrets,
            TRRandomizerType.HardSecrets,
            TRRandomizerType.HiddenEnemies,
            TRRandomizerType.Item,
            TRRandomizerType.ItemSprite,
            TRRandomizerType.KeyItems,
            TRRandomizerType.KeyItemTextures,
            TRRandomizerType.LarsonBehaviour,
            TRRandomizerType.MeshSwaps,
            TRRandomizerType.NightMode,
            TRRandomizerType.Outfit,
            TRRandomizerType.RewardRooms,
            TRRandomizerType.Secret,
            TRRandomizerType.SecretModels,
            TRRandomizerType.SecretReward,
            TRRandomizerType.SecretTextures,
            TRRandomizerType.SFX,
            TRRandomizerType.StartPosition,
            TRRandomizerType.Texture
        };

        private static readonly List<TRRandomizerType> _tr1MainTypes = new List<TRRandomizerType>
        {
            TRRandomizerType.AmbientTracks,
            TRRandomizerType.Ammoless,
            TRRandomizerType.Braid,
            TRRandomizerType.DisableDemos,
            TRRandomizerType.Health,
            TRRandomizerType.LevelSequence,
            TRRandomizerType.Mediless,
            TRRandomizerType.SecretCount,
            TRRandomizerType.Unarmed,
            TRRandomizerType.WaterColour
        };

        private static readonly List<TRRandomizerType> _tr2Types = new List<TRRandomizerType>
        {
            TRRandomizerType.AmbientTracks,
            TRRandomizerType.Ammoless,
            TRRandomizerType.Audio,
            TRRandomizerType.BirdMonsterBehaviour,
            TRRandomizerType.Braid,
            TRRandomizerType.DisableDemos,
            TRRandomizerType.DragonSpawn,
            TRRandomizerType.DynamicTextures,
            TRRandomizerType.Enemy,
            TRRandomizerType.Environment,
            TRRandomizerType.GlitchedSecrets,
            TRRandomizerType.HardSecrets,
            TRRandomizerType.Item,
            TRRandomizerType.KeyItems,
            TRRandomizerType.KeyItemTextures,
            TRRandomizerType.Ladders,
            TRRandomizerType.LevelSequence,
            TRRandomizerType.MeshSwaps,
            TRRandomizerType.NightMode,
            TRRandomizerType.Outfit,
            TRRandomizerType.OutfitDagger,
            TRRandomizerType.Secret,
            TRRandomizerType.SecretAudio,
            TRRandomizerType.SecretReward,
            TRRandomizerType.SecretTextures,
            TRRandomizerType.SFX,
            TRRandomizerType.StartPosition,
            TRRandomizerType.Sunset,
            TRRandomizerType.Text,
            TRRandomizerType.Texture,
            TRRandomizerType.Unarmed,
            TRRandomizerType.ItemSprite
        };

        private static readonly List<TRRandomizerType> _tr3Types = new List<TRRandomizerType>
        {
            TRRandomizerType.AmbientTracks,
            TRRandomizerType.Ammoless,
            TRRandomizerType.Audio,
            TRRandomizerType.Braid,
            TRRandomizerType.Enemy,
            TRRandomizerType.Environment,
            TRRandomizerType.GlitchedSecrets,
            TRRandomizerType.GlobeDisplay,
            TRRandomizerType.HardSecrets,
            TRRandomizerType.Item,
            TRRandomizerType.KeyItems,
            TRRandomizerType.Ladders,
            TRRandomizerType.LevelSequence,
            TRRandomizerType.NightMode,
            TRRandomizerType.Outfit,
            TRRandomizerType.RewardRooms,
            TRRandomizerType.Secret,
            TRRandomizerType.SecretAudio,
            TRRandomizerType.SecretReward,
            TRRandomizerType.SecretTextures,
            TRRandomizerType.SFX,
            TRRandomizerType.StartPosition,
            TRRandomizerType.Text,
            TRRandomizerType.Texture,
            TRRandomizerType.Unarmed,
            TRRandomizerType.VFX
        };

        private static readonly List<TRRandomizerType> _tr3MainTypes = new List<TRRandomizerType>
        {
            //TRRandomizerType.Weather
        };

        private static readonly Dictionary<TRVersion, TRVersionSupportGroup> _supportedTypes = new Dictionary<TRVersion, TRVersionSupportGroup>
        {
            [TRVersion.TR1] = new TRVersionSupportGroup
            {
                DefaultSupport = _tr1Types,
                PatchSupport = _tr1MainTypes
            },
            [TRVersion.TR2] = new TRVersionSupportGroup
            {
                DefaultSupport = _tr2Types
            },
            [TRVersion.TR3] = new TRVersionSupportGroup
            {
                DefaultSupport = _tr3Types,
                PatchSupport = _tr3MainTypes
            }
        };

        private static readonly Dictionary<TRVersion, List<string>> _versionExes = new Dictionary<TRVersion, List<string>>
        {
            [TRVersion.TR1] = new List<string> { "Tomb1Main.exe", "tombati.exe" },
            [TRVersion.TR2] = new List<string> { "Tomb2.exe" },
            [TRVersion.TR3] = new List<string> { "Tomb3.exe" }
        };

        public bool IsRandomizationSupported(TREdition edition)
        {
            return _supportedTypes.ContainsKey(edition.Version);
        }

        public bool IsRandomizationSupported(TREdition edition, TRRandomizerType randomizerType)
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

            return supported;
        }

        public List<string> GetExecutables(TREdition edition)
        {
            List<string> exes = new List<string>();
            if (_versionExes.ContainsKey(edition.Version))
            {
                exes.AddRange(_versionExes[edition.Version]);
            }
            return exes;
        }
    }

    internal class TRVersionSupportGroup
    {
        internal List<TRRandomizerType> DefaultSupport { get; set; }
        internal List<TRRandomizerType> PatchSupport { get; set; }
        internal bool HasPatchSupport => PatchSupport != null;
    }
}