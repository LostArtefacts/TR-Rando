using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRDataControl;

public class TR1DataProvider : IDataProvider<TR1Type, TR1SFX>
{
    public int TextureTileLimit { get; set; } = 32;
    public int TextureObjectLimit { get; set; } = 8192;

    public Dictionary<TR1Type, TR1Type> AliasPriority { get; set; }

    public TRBlobType GetBlobType(TR1Type type)
    {
        if (_spriteTypes.Contains(type))
        {
            return TRBlobType.Sprite;
        }
        if (type > TR1Type.HarpoonProjectile_H && type < TR1Type.M16_S_P)
        {
            return TRBlobType.StaticMesh;
        }
        return TRBlobType.Model;
    }

    public IEnumerable<TR1Type> GetDependencies(TR1Type type)
    {
        if (_typeDependencies.ContainsKey(type))
        {
            return _typeDependencies[type];
        }

        if (IsAlias(type))
        {
            return GetDependencies(TranslateAlias(type));
        }

        return _emptyTypes;
    }

    public IEnumerable<TR1Type> GetRemovalExclusions(TR1Type type)
    {
        return _removalExclusions.ContainsKey(type) ? _removalExclusions[type] : _emptyTypes;
    }

    public IEnumerable<TR1Type> GetCyclicDependencies(TR1Type type)
    {
        return _cyclicDependencies.ContainsKey(type) ? _cyclicDependencies[type] : _emptyTypes;
    }

    public IEnumerable<TR1Type> GetCinematicTypes()
    {
        return _cinematicTypes;
    }

    public bool IsAlias(TR1Type type)
    {
        foreach (List<TR1Type> aliases in _typeAliases.Values)
        {
            if (aliases.Contains(type))
            {
                return true;
            }
        }

        return false;
    }

    public bool HasAliases(TR1Type type)
    {
        return _typeAliases.ContainsKey(type);
    }

    public TR1Type TranslateAlias(TR1Type type)
    {
        foreach (TR1Type root in _typeAliases.Keys)
        {
            if (_typeAliases[root].Contains(type))
            {
                return root;
            }
        }

        return type;
    }

    public IEnumerable<TR1Type> GetAliases(TR1Type type)
    {
        return _typeAliases.ContainsKey(type) ? _typeAliases[type] : _emptyTypes;
    }

    public TR1Type GetLevelAlias(string level, TR1Type type)
    {
        return TR1TypeUtilities.GetAliasForLevel(level, type);
    }

    public bool IsAliasDuplicatePermitted(TR1Type type)
    {
        return _permittedAliasDuplicates.Contains(type);
    }

    public bool IsOverridePermitted(TR1Type type)
    {
        return _permittedOverrides.Contains(type);
    }

    public bool IsNonGraphicsDependency(TR1Type type)
    {
        return false;
    }

    public IEnumerable<TR1SFX> GetHardcodedSounds(TR1Type type)
    {
        return _hardcodedSFX.ContainsKey(type)
            ? _hardcodedSFX[type]
            : _emptySFX;
    }

    #region Data

    private static readonly List<TR1Type> _emptyTypes = new();
    private static readonly List<TR1SFX> _emptySFX = new();

    private static readonly Dictionary<TR1Type, List<TR1Type>> _typeDependencies = new()
    {
        [TR1Type.LaraFlareAnim_H] = new()
        {
            TR1Type.FlareSparks_M_H, TR1Type.Flares_M_H, TR1Type.Flare_H, TR1Type.Flares_S_P
        },
        [TR1Type.LaraM16Anim_H] = new()
        {
            TR1Type.M16Gunflare_H, TR1Type.M16_M_H, TR1Type.M16Ammo_M_H, TR1Type.M16_S_P, TR1Type.M16Ammo_S_P,
        },
        [TR1Type.LaraGrenadeAnim_H] = new()
        {
            TR1Type.GrenadeProjectile_H, TR1Type.GrenadeLauncher_M_H, TR1Type.Grenades_M_H, TR1Type.GrenadeLauncher_S_P, TR1Type.Grenades_S_P,
        },
        [TR1Type.LaraHarpoonAnim_H] = new()
        {
            TR1Type.HarpoonProjectile_H, TR1Type.Harpoon_M_H, TR1Type.HarpoonAmmo_M_H, TR1Type.Harpoon_S_P, TR1Type.HarpoonAmmo_S_P,
        },
        [TR1Type.Adam]
            = new() { TR1Type.LaraMiscAnim_H_Pyramid, TR1Type.Explosion1_S_H },
        [TR1Type.AtlanteanLava]
            = new() { TR1Type.Flame_S_H },
        [TR1Type.BandagedAtlantean]
            = new() { TR1Type.BandagedFlyer },
        [TR1Type.BandagedFlyer]
            = new() { TR1Type.Missile2_H, TR1Type.Missile3_H, TR1Type.Explosion1_S_H },
        [TR1Type.Centaur]
            = new() { TR1Type.Missile3_H, TR1Type.Explosion1_S_H },
        [TR1Type.CentaurStatue]
            = new() { TR1Type.Centaur },
        [TR1Type.CrocodileLand]
            = new() { TR1Type.CrocodileWater },
        [TR1Type.CrocodileWater]
            = new() { TR1Type.CrocodileLand },
        [TR1Type.DartEmitter]
            = new() { TR1Type.Dart_H },
        [TR1Type.FlameEmitter_N]
            = new() { TR1Type.Flame_S_H },
        [TR1Type.Key1_M_H]
            = new() { TR1Type.Key1_S_P },
        [TR1Type.LavaEmitter_N]
            = new() { TR1Type.LavaParticles_S_H },
        [TR1Type.MeatyAtlantean]
            = new() { TR1Type.MeatyFlyer },
        [TR1Type.MeatyFlyer]
            = new() { TR1Type.Missile2_H, TR1Type.Missile3_H, TR1Type.Explosion1_S_H },
        [TR1Type.MidasHand_N]
            = new() { TR1Type.LaraMiscAnim_H_Midas, TR1Type.Sparkles_S_H },
        [TR1Type.Missile3_H]
            = new() { TR1Type.Explosion1_S_H },
        [TR1Type.Natla]
            = new() { TR1Type.Missile2_H, TR1Type.Missile3_H, TR1Type.Explosion1_S_H },
        [TR1Type.Pierre]
            = new() { TR1Type.Key1_M_H, TR1Type.ScionPiece_M_H },
        [TR1Type.RatLand]
            = new() { TR1Type.RatWater },
        [TR1Type.RatWater]
            = new() { TR1Type.RatLand },
        [TR1Type.ScionPiece_M_H]
            = new() { TR1Type.ScionPiece2_S_P },
        [TR1Type.SecretAnkh_M_H]
            = new() { TR1Type.SecretAnkh_S_P },
        [TR1Type.SecretGoldBar_M_H]
            = new() { TR1Type.SecretGoldBar_S_P },
        [TR1Type.SecretGoldIdol_M_H]
            = new() { TR1Type.SecretGoldIdol_S_P },
        [TR1Type.SecretLeadBar_M_H]
            = new() { TR1Type.SecretLeadBar_S_P },
        [TR1Type.SecretScion_M_H]
            = new() { TR1Type.SecretScion_S_P },
        [TR1Type.ShootingAtlantean_N]
            = new() { TR1Type.MeatyFlyer },
        [TR1Type.SkateboardKid]
            = new() { TR1Type.Skateboard },
        [TR1Type.ThorHammerHandle]
            = new() { TR1Type.ThorHammerBlock },
        [TR1Type.TRex]
            = new() { TR1Type.LaraMiscAnim_H_Valley },
    };

    private static readonly Dictionary<TR1Type, List<TR1Type>> _cyclicDependencies = new()
    {
        [TR1Type.CrocodileLand]
            = new() { TR1Type.CrocodileWater },
        [TR1Type.CrocodileWater]
            = new() { TR1Type.CrocodileLand },
        [TR1Type.RatLand]
            = new() { TR1Type.RatWater },
        [TR1Type.RatWater]
            = new() { TR1Type.RatLand },
        [TR1Type.Pierre]
            = new() { TR1Type.ScionPiece_M_H },
    };

    private static readonly Dictionary<TR1Type, List<TR1Type>> _removalExclusions = new()
    {
        [TR1Type.FlyingAtlantean]
            = new() { TR1Type.NonShootingAtlantean_N, TR1Type.ShootingAtlantean_N }
    };

    private static readonly List<TR1Type> _cinematicTypes = new()
    {
        TR1Type.MidasHand_N
    };

    private static readonly Dictionary<TR1Type, List<TR1Type>> _typeAliases = new()
    {
        [TR1Type.FlyingAtlantean] = new()
        {
            TR1Type.BandagedFlyer, TR1Type.MeatyFlyer
        },
        [TR1Type.LaraMiscAnim_H] = new()
        {
            TR1Type.LaraMiscAnim_H_General, TR1Type.LaraMiscAnim_H_Valley, TR1Type.LaraMiscAnim_H_Qualopec, TR1Type.LaraMiscAnim_H_Midas,
            TR1Type.LaraMiscAnim_H_Sanctuary, TR1Type.LaraMiscAnim_H_Atlantis, TR1Type.LaraMiscAnim_H_Pyramid
        },
        [TR1Type.NonShootingAtlantean_N] = new()
        {
            TR1Type.BandagedAtlantean, TR1Type.MeatyAtlantean
        },
        [TR1Type.Cowboy] = new()
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

    private static readonly Dictionary<TR1Type, List<TR1SFX>> _hardcodedSFX = new()
    {
        [TR1Type.Adam] = new()
        {
            TR1SFX.TorsoHit,
        },
        [TR1Type.Bear] = new()
        {
            TR1SFX.BearFeet,
            TR1SFX.BearHurt,
        },
        [TR1Type.DamoclesSword] = new()
        {
            TR1SFX.DamoclesSword,
        },
        [TR1Type.DartEmitter] = new()
        {
            TR1SFX.Darts,
        },
        [TR1Type.Larson] = new()
        {
            TR1SFX.LarsonRicochet,
        },
        [TR1Type.Earthquake_N] = new()
        {
            TR1SFX.RollingBall,
            TR1SFX.TRexStomp,
        },
        [TR1Type.FlameEmitter_N] = new()
        {
            TR1SFX.Fire,
        },
        [TR1Type.Explosion1_S_H] = new()
        {
            TR1SFX.AtlanteanExplode,
        },
        [TR1Type.LavaEmitter_N] = new()
        {
            TR1SFX.LavaFountain,
        },
        [TR1Type.Lion] = new()
        {
            TR1SFX.LionHurt,
        },
        [TR1Type.Lioness] = new()
        {
            TR1SFX.LionHurt,
        },
        [TR1Type.Natla] = new()
        {
            TR1SFX.AtlanteanNeedle,
            TR1SFX.AtlanteanBall,
            TR1SFX.NatlaSpeech,
        },
        [TR1Type.Panther] = new()
        {
            TR1SFX.LionHurt,
        },
        [TR1Type.SkateboardKid] = new()
        {
            TR1SFX.SkateKidHit,
            TR1SFX.SkateKidSpeech,
        },
        [TR1Type.TeethSpikes] = new()
        {
            TR1SFX.LaraSpikeDeath,
        },
        [TR1Type.ThorLightning] = new()
        {
            TR1SFX.Thunder,
        },
        [TR1Type.UnderwaterSwitch] = new()
        {
            TR1SFX.UnderwaterSwitch,
        },
        [TR1Type.Wolf] = new()
        {
            TR1SFX.WolfHurt,
        },
    };

    private static readonly List<TR1Type> _spriteTypes = new()
    {
        TR1Type.Pistols_S_P,
        TR1Type.Shotgun_S_P,
        TR1Type.Magnums_S_P,
        TR1Type.Uzis_S_P,
        TR1Type.PistolAmmo_S_P,
        TR1Type.ShotgunAmmo_S_P,
        TR1Type.MagnumAmmo_S_P,
        TR1Type.UziAmmo_S_P,
        TR1Type.SmallMed_S_P,
        TR1Type.LargeMed_S_P,
        TR1Type.Puzzle1_S_P,
        TR1Type.Puzzle2_S_P,
        TR1Type.Puzzle3_S_P,
        TR1Type.Puzzle4_S_P,
        TR1Type.LeadBar_S_P,
        TR1Type.Key1_S_P,
        TR1Type.Key2_S_P,
        TR1Type.Key3_S_P,
        TR1Type.Key4_S_P,
        TR1Type.ScionPiece1_S_P,
        TR1Type.ScionPiece2_S_P,
        TR1Type.Explosion1_S_H,
        TR1Type.WaterRipples1_S_H,
        TR1Type.Bubbles1_S_H,
        TR1Type.Bubbles2_S_H,
        TR1Type.Blood1_S_H,
        TR1Type.DartEffect_S_H,
        TR1Type.Ricochet_S_H,
        TR1Type.Sparkles_S_H,
        TR1Type.LavaParticles_S_H,
        TR1Type.Flame_S_H,
        TR1Type.FontGraphics_S_H,
        TR1Type.PickupAid_S_H,

        TR1Type.SecretScion_S_P,
        TR1Type.SecretGoldIdol_S_P,
        TR1Type.SecretLeadBar_S_P,
        TR1Type.SecretGoldBar_S_P,
        TR1Type.SecretAnkh_S_P,

        TR1Type.Flares_S_P,
        TR1Type.M16_S_P,
        TR1Type.GrenadeLauncher_S_P,
        TR1Type.Harpoon_S_P,
        TR1Type.M16Ammo_S_P,
        TR1Type.Grenades_S_P,
        TR1Type.HarpoonAmmo_S_P,
    };

    #endregion
}
