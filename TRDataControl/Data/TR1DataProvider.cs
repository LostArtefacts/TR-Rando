using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRModelTransporter.Data;

public class TR1DataProvider : IDataProvider<TR1Type>
{
    public int TextureTileLimit { get; set; } = 16;
    public int TextureObjectLimit { get; set; } = 2048;

    public Dictionary<TR1Type, TR1Type> AliasPriority { get; set; }

    public IEnumerable<TR1Type> GetModelDependencies(TR1Type entity)
    {
        return _entityDependencies.ContainsKey(entity) ? _entityDependencies[entity] : _emptyEntities;
    }

    public IEnumerable<TR1Type> GetRemovalExclusions(TR1Type entity)
    {
        return _removalExclusions.ContainsKey(entity) ? _removalExclusions[entity] : _emptyEntities;
    }

    public IEnumerable<TR1Type> GetCyclicDependencies(TR1Type entity)
    {
        return _cyclicDependencies.ContainsKey(entity) ? _cyclicDependencies[entity] : _emptyEntities;
    }

    public IEnumerable<TR1Type> GetSpriteDependencies(TR1Type entity)
    {
        return _spriteDependencies.ContainsKey(entity) ? _spriteDependencies[entity] : _emptyEntities;
    }

    public IEnumerable<TR1Type> GetCinematicEntities()
    {
        return _cinematicEntities;
    }

    public IEnumerable<TR1Type> GetLaraDependants()
    {
        return _laraDependentModels;
    }

    public bool IsAlias(TR1Type entity)
    {
        foreach (List<TR1Type> aliases in _entityAliases.Values)
        {
            if (aliases.Contains(entity))
            {
                return true;
            }
        }

        return false;
    }

    public bool HasAliases(TR1Type entity)
    {
        return _entityAliases.ContainsKey(entity);
    }

    public TR1Type TranslateAlias(TR1Type entity)
    {
        foreach (TR1Type root in _entityAliases.Keys)
        {
            if (_entityAliases[root].Contains(entity))
            {
                return root;
            }
        }

        return entity;
    }

    public IEnumerable<TR1Type> GetAliases(TR1Type entity)
    {
        return _entityAliases.ContainsKey(entity) ? _entityAliases[entity] : _emptyEntities;
    }

    public TR1Type GetLevelAlias(string level, TR1Type entity)
    {
        return TR1TypeUtilities.GetAliasForLevel(level, entity);
    }

    public bool IsAliasDuplicatePermitted(TR1Type entity)
    {
        return _permittedAliasDuplicates.Contains(entity);
    }

    public bool IsOverridePermitted(TR1Type entity)
    {
        return _permittedOverrides.Contains(entity);
    }

    public IEnumerable<TR1Type> GetUnsafeModelReplacements()
    {
        return _unsafeModelReplacements;
    }

    public bool IsNonGraphicsDependency(TR1Type entity)
    {
        return _nonGraphicsDependencies.Contains(entity);
    }

    public bool IsSoundOnlyDependency(TR1Type entity)
    {
        return _soundOnlyDependencies.Contains(entity);
    }

    public short[] GetHardcodedSounds(TR1Type entity)
    {
        return _hardcodedSoundIndices.ContainsKey(entity) ? _hardcodedSoundIndices[entity] : null;
    }

    public IEnumerable<int> GetIgnorableTextureIndices(TR1Type entity, string level)
    {
        if (entity == TR1Type.LaraMiscAnim_H && level == TR1LevelNames.VALLEY)
        {
            // Mesh swap when Lara is killed by T-Rex
            return null;
        }
        return _ignoreEntityTextures.ContainsKey(entity) ? _ignoreEntityTextures[entity] : null;
    }

    #region Data

    private static readonly IEnumerable<TR1Type> _emptyEntities = new List<TR1Type>();

    private static readonly Dictionary<TR1Type, TR1Type[]> _entityDependencies = new()
    {
        [TR1Type.Adam]
            = new TR1Type[] { TR1Type.LaraMiscAnim_H_Pyramid },
        [TR1Type.BandagedAtlantean]
            = new TR1Type[] { TR1Type.BandagedFlyer },
        [TR1Type.BandagedFlyer]
            = new TR1Type[] { TR1Type.Missile2_H, TR1Type.Missile3_H },
        [TR1Type.Centaur]
            = new TR1Type[] { TR1Type.Missile3_H },
        [TR1Type.CentaurStatue]
            = new TR1Type[] { TR1Type.Centaur },
        [TR1Type.CrocodileLand]
            = new TR1Type[] { TR1Type.CrocodileWater },
        [TR1Type.CrocodileWater]
            = new TR1Type[] { TR1Type.CrocodileLand },
        [TR1Type.DartEmitter]
            = new TR1Type[] { TR1Type.Dart_H },
        [TR1Type.MeatyAtlantean]
            = new TR1Type[] { TR1Type.MeatyFlyer },
        [TR1Type.MeatyFlyer]
            = new TR1Type[] { TR1Type.Missile2_H, TR1Type.Missile3_H },
        [TR1Type.MidasHand_N]
            = new TR1Type[] { TR1Type.LaraMiscAnim_H_Midas },
        [TR1Type.Natla]
            = new TR1Type[] { TR1Type.Missile2_H, TR1Type.Missile3_H },
        [TR1Type.Pierre]
            = new TR1Type[] { TR1Type.Key1_M_H, TR1Type.ScionPiece_M_H },
        [TR1Type.RatLand]
            = new TR1Type[] { TR1Type.RatWater },
        [TR1Type.RatWater]
            = new TR1Type[] { TR1Type.RatLand },
        [TR1Type.ShootingAtlantean_N]
            = new TR1Type[] { TR1Type.MeatyFlyer },
        [TR1Type.SkateboardKid]
            = new TR1Type[] { TR1Type.Skateboard },
        [TR1Type.ThorHammerHandle]
            = new TR1Type[] { TR1Type.ThorHammerBlock },
        [TR1Type.TRex]
            = new TR1Type[] { TR1Type.LaraMiscAnim_H_Valley }
    };

    private static readonly Dictionary<TR1Type, TR1Type[]> _cyclicDependencies = new()
    {
        [TR1Type.CrocodileLand]
            = new TR1Type[] { TR1Type.CrocodileWater },
        [TR1Type.CrocodileWater]
            = new TR1Type[] { TR1Type.CrocodileLand },
        [TR1Type.RatLand]
            = new TR1Type[] { TR1Type.RatWater },
        [TR1Type.RatWater]
            = new TR1Type[] { TR1Type.RatLand },
        [TR1Type.Pierre]
            = new TR1Type[] { TR1Type.ScionPiece_M_H },
    };

    private static readonly Dictionary<TR1Type, List<TR1Type>> _removalExclusions = new()
    {
        [TR1Type.FlyingAtlantean]
            = new List<TR1Type> { TR1Type.NonShootingAtlantean_N, TR1Type.ShootingAtlantean_N }
    };

    private static readonly Dictionary<TR1Type, List<TR1Type>> _spriteDependencies = new()
    {
        [TR1Type.SecretScion_M_H]
            = new List<TR1Type> { TR1Type.ScionPiece4_S_P },
        [TR1Type.SecretGoldIdol_M_H]
            = new List<TR1Type> { TR1Type.ScionPiece4_S_P },
        [TR1Type.SecretLeadBar_M_H]
            = new List<TR1Type> { TR1Type.ScionPiece4_S_P },
        [TR1Type.SecretGoldBar_M_H]
            = new List<TR1Type> { TR1Type.ScionPiece4_S_P },
        [TR1Type.SecretAnkh_M_H]
            = new List<TR1Type> { TR1Type.ScionPiece4_S_P },

        [TR1Type.Adam]
            = new List<TR1Type> { TR1Type.Explosion1_S_H },
        [TR1Type.Centaur]
            = new List<TR1Type> { TR1Type.Explosion1_S_H },
        [TR1Type.FlyingAtlantean]
            = new List<TR1Type> { TR1Type.Explosion1_S_H },
        [TR1Type.Key1_M_H]
            = new List<TR1Type> { TR1Type.Key1_S_P },
        [TR1Type.Natla]
            = new List<TR1Type> { TR1Type.Explosion1_S_H },
        [TR1Type.ScionPiece_M_H]
            = new List<TR1Type> { TR1Type.ScionPiece2_S_P },
        [TR1Type.ShootingAtlantean_N]
            = new List<TR1Type> { TR1Type.Explosion1_S_H },
        [TR1Type.Missile3_H]
            = new List<TR1Type> { TR1Type.Explosion1_S_H },
        [TR1Type.FlameEmitter_N]
            = new() { TR1Type.Flame_S_H },
        [TR1Type.MidasHand_N]
            = new() { TR1Type.Sparkles_S_H },
        [TR1Type.LavaEmitter_N]
            = new() { TR1Type.LavaParticles_S_H },
        [TR1Type.AtlanteanLava]
            = new() { TR1Type.Flame_S_H }
    };

    private static readonly List<TR1Type> _cinematicEntities = new()
    {
        TR1Type.MidasHand_N
    };

    // These are models that use Lara's hips as placeholders
    private static readonly List<TR1Type> _laraDependentModels = new()
    {
        TR1Type.NonShootingAtlantean_N, TR1Type.ShootingAtlantean_N, TR1Type.Earthquake_N, TR1Type.FlameEmitter_N,
        TR1Type.LavaEmitter_N, TR1Type.AtlanteanLava, TR1Type.MidasHand_N
    };

    private static readonly Dictionary<TR1Type, List<TR1Type>> _entityAliases = new()
    {
        [TR1Type.FlyingAtlantean] = new List<TR1Type>
        {
            TR1Type.BandagedFlyer, TR1Type.MeatyFlyer
        },
        [TR1Type.LaraMiscAnim_H] = new List<TR1Type>
        {
            TR1Type.LaraMiscAnim_H_General, TR1Type.LaraMiscAnim_H_Valley, TR1Type.LaraMiscAnim_H_Qualopec, TR1Type.LaraMiscAnim_H_Midas,
            TR1Type.LaraMiscAnim_H_Sanctuary, TR1Type.LaraMiscAnim_H_Atlantis, TR1Type.LaraMiscAnim_H_Pyramid
        },
        [TR1Type.NonShootingAtlantean_N] = new List<TR1Type>
        {
            TR1Type.BandagedAtlantean, TR1Type.MeatyAtlantean
        },
        [TR1Type.Cowboy] = new List<TR1Type>
        {
            TR1Type.CowboyOG, TR1Type.CowboyHeadless
        }
    };

    private static readonly List<TR1Type> _permittedAliasDuplicates = new()
    {
        TR1Type.LaraMiscAnim_H
    };

    private static readonly List<TR1Type> _permittedOverrides = new()
    {
        TR1Type.LaraPonytail_H_U, TR1Type.ScionPiece_M_H
    };

    private static readonly List<TR1Type> _unsafeModelReplacements = new()
    {
    };

    private static readonly List<TR1Type> _nonGraphicsDependencies = new()
    {
    };

    // If these are imported into levels that already have another alias for them, only their hardcoded sounds will be imported
    protected static readonly List<TR1Type> _soundOnlyDependencies = new()
    {
    };

    private static readonly Dictionary<TR1Type, short[]> _hardcodedSoundIndices = new()
    {
        [TR1Type.Adam] = new short[] { 104, 137, 138, 140, 141, 142 },
        [TR1Type.BandagedFlyer] = new short[] { 104 },
        [TR1Type.Bear] = new short[] { 12, 16 },
        [TR1Type.Centaur] = new short[] { 104 },
        [TR1Type.DamoclesSword] = new short[] { 103 },
        [TR1Type.DartEmitter] = new short[] { 151 },
        [TR1Type.Earthquake_N] = new short[] { 70, 147 },
        [TR1Type.FlameEmitter_N] = new short[] { 150 },
        [TR1Type.Gorilla] = new short[] { 90, 91, 101 },
        [TR1Type.Larson] = new short[] { 78 },
        [TR1Type.LavaEmitter_N] = new short[] { 149 },
        [TR1Type.Lion] = new short[] { 85, 86, 87 },
        [TR1Type.Lioness] = new short[] { 85, 86, 87 },
        [TR1Type.MeatyAtlantean] = new short[] { 104 },
        [TR1Type.MeatyFlyer] = new short[] { 104, 120, 121, 122, 123, 124, 125, 126 },
        [TR1Type.Missile3_H] = new short[] { 104 },
        [TR1Type.Natla] = new short[] { 104, 123, 124, 202 },
        [TR1Type.ShootingAtlantean_N] = new short[] { 104 },
        [TR1Type.SkateboardKid] = new short[] { 132, 204 },
        [TR1Type.TeethSpikes] = new short[] { 145 },
        [TR1Type.ThorHammerHandle] = new short[] { 70 },
        [TR1Type.ThorLightning] = new short[] { 98 },
        [TR1Type.UnderwaterSwitch] = new short[] { 61 },
        [TR1Type.Wolf] = new short[] { 20 }
    };

    private static readonly Dictionary<TR1Type, List<int>> _ignoreEntityTextures = new()
    {
        [TR1Type.LaraMiscAnim_H]
            = new(), // empty list indicates to ignore everything
        [TR1Type.Earthquake_N]
            = new(),
        [TR1Type.FlameEmitter_N]
            = new() { 179, 180, 181, 183, 184, 185, 188, 189, 194 },
        [TR1Type.MidasHand_N]
            = new() { 179, 180, 181, 183, 184, 185, 188, 189, 194 },
        [TR1Type.LavaEmitter_N]
            = new() { 184, 185, 188, 189, 190, 192, 193, 195, 197 },
        [TR1Type.NonShootingAtlantean_N]
            = new(),
        [TR1Type.ShootingAtlantean_N]
            = new(),
        [TR1Type.Mummy]
            = new() { 130, 131, 132, 133, 134, 135, 136, 137, 140, 141 },
        [TR1Type.ThorLightning]
            = new() { 150, 151, 154, 155, 156, 157, 158, 159, 160, 161 }
    };

    #endregion
}
