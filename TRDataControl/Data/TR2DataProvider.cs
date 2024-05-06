using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRDataControl;

public class TR2DataProvider : IDataProvider<TR2Type, TR2SFX>
{
    public int TextureTileLimit { get; set; } = 16;
    public int TextureObjectLimit { get; set; } = 2048;

    public Dictionary<TR2Type, TR2Type> AliasPriority { get; set; }

    public TRBlobType GetBlobType(TR2Type type)
    {
        if (_spriteTypes.Contains(type))
        {
            return TRBlobType.Sprite;
        }
        if (type >= TR2Type.SceneryBase && type < TR2Type.BengalTiger)
        {
            return TRBlobType.StaticMesh;
        }
        return TRBlobType.Model;
    }

    public IEnumerable<TR2Type> GetDependencies(TR2Type type)
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

    public IEnumerable<TR2Type> GetRemovalExclusions(TR2Type type)
    {
        return _emptyTypes;
    }

    public IEnumerable<TR2Type> GetCyclicDependencies(TR2Type type)
    {
        return _emptyTypes;
    }

    public IEnumerable<TR2Type> GetCinematicTypes()
    {
        return _cinematicTypes;
    }

    public bool IsAlias(TR2Type type)
    {
        foreach (List<TR2Type> aliases in _typeAliases.Values)
        {
            if (aliases.Contains(type))
            {
                return true;
            }
        }

        return false;
    }

    public bool HasAliases(TR2Type type)
    {
        return _typeAliases.ContainsKey(type);
    }

    public TR2Type TranslateAlias(TR2Type type)
    {
        foreach (TR2Type root in _typeAliases.Keys)
        {
            if (_typeAliases[root].Contains(type))
            {
                return root;
            }
        }

        return type;
    }

    public IEnumerable<TR2Type> GetAliases(TR2Type type)
    {
        return _typeAliases.ContainsKey(type) ? _typeAliases[type] : _emptyTypes;
    }

    public TR2Type GetLevelAlias(string level, TR2Type type)
    {
        return TR2TypeUtilities.GetAliasForLevel(level, type);
    }

    public bool IsAliasDuplicatePermitted(TR2Type type)
    {
        return _permittedAliasDuplicates.Contains(type);
    }

    public bool IsOverridePermitted(TR2Type type)
    {
        return _permittedOverrides.Contains(type);
    }

    public bool IsNonGraphicsDependency(TR2Type type)
    {
        return _nonGraphicsDependencies.Contains(type);
    }

    public IEnumerable<TR2SFX> GetHardcodedSounds(TR2Type type)
    {
        return _hardcodedSFX.ContainsKey(type)
            ? _hardcodedSFX[type]
            : _emptySFX;
    }

    #region Data

    private static readonly List<TR2Type> _emptyTypes = new();
    private static readonly List<TR2SFX> _emptySFX = new();

    private static readonly Dictionary<TR2Type, List<TR2Type>> _typeDependencies = new()
    {
        [TR2Type.Autos_M_H] =
            new() { TR2Type.LaraAutoAnim_H, TR2Type.Gunflare_H, TR2Type.Automags_S_P, TR2Type.AutoAmmo_M_H, TR2Type.AutoAmmo_S_P },
        [TR2Type.Boat] =
            new() { TR2Type.LaraBoatAnim_H, TR2Type.BoatWake_S_H },
        [TR2Type.BlackSnowmob] =
            new() { TR2Type.RedSnowmobile },
        [TR2Type.FlamethrowerGoon]
            = new() { TR2Type.Flame_S_H },
        [TR2Type.GrenadeLauncher_M_H] =
            new() { TR2Type.LaraGrenadeAnim_H, TR2Type.GrenadeProjectile_H, TR2Type.GrenadeLauncher_S_P, TR2Type.Grenades_M_H, TR2Type.Grenades_S_P, TR2Type.Explosion_S_H },
        [TR2Type.Harpoon_M_H] =
            new() { TR2Type.LaraHarpoonAnim_H, TR2Type.HarpoonProjectile_H, TR2Type.Harpoon_S_P, TR2Type.HarpoonAmmo_M_H, TR2Type.HarpoonAmmo_S_P },
        [TR2Type.Key2_M_H]
            = new() { TR2Type.Key2_S_P },
        [TR2Type.Knifethrower] =
            new() { TR2Type.KnifeProjectile_H },
        [TR2Type.LaraSun] =
            new() { TR2Type.LaraPistolAnim_H_Sun, TR2Type.LaraAutoAnim_H_Sun, TR2Type.LaraUziAnim_H_Sun },
        [TR2Type.LaraUnwater] =
            new() { TR2Type.LaraPistolAnim_H_Unwater, TR2Type.LaraAutoAnim_H_Unwater, TR2Type.LaraUziAnim_H_Unwater },
        [TR2Type.LaraSnow] =
            new() { TR2Type.LaraPistolAnim_H_Snow, TR2Type.LaraAutoAnim_H_Snow, TR2Type.LaraUziAnim_H_Snow },
        [TR2Type.LaraHome] =
            new() { TR2Type.LaraPistolAnim_H_Home, TR2Type.LaraAutoAnim_H_Home, TR2Type.LaraUziAnim_H_Home },
        [TR2Type.M16_M_H] =
            new() { TR2Type.LaraM16Anim_H, TR2Type.M16Gunflare_H, TR2Type.M16_S_P, TR2Type.M16Ammo_M_H, TR2Type.M16Ammo_S_P },
        [TR2Type.MarcoBartoli] =
            new()
            {
                TR2Type.DragonExplosionEmitter_N, TR2Type.DragonExplosion1_H, TR2Type.DragonExplosion2_H, TR2Type.DragonExplosion3_H,
                TR2Type.DragonFront_H, TR2Type.DragonBack_H, TR2Type.DragonBonesFront_H, TR2Type.DragonBonesBack_H, TR2Type.LaraMiscAnim_H_Xian,
                TR2Type.Puzzle2_M_H_Dagger, TR2Type.Flame_S_H
            },
        [TR2Type.MaskedGoon2] =
            new() { TR2Type.MaskedGoon1 },
        [TR2Type.MaskedGoon3] =
            new() { TR2Type.MaskedGoon1 },
        [TR2Type.Mercenary3] =
            new() { TR2Type.Mercenary2 },
        [TR2Type.MercSnowmobDriver] =
            new() { TR2Type.BlackSnowmob },
        [TR2Type.Pistols_M_H] =
            new() { TR2Type.LaraPistolAnim_H, TR2Type.Gunflare_H, TR2Type.Pistols_S_P },
        [TR2Type.RedSnowmobile] =
            new() { TR2Type.SnowmobileBelt, TR2Type.LaraSnowmobAnim_H, TR2Type.SnowmobileWake_S_H },
        [TR2Type.Shotgun_M_H] =
            new() { TR2Type.LaraShotgunAnim_H, TR2Type.Gunflare_H, TR2Type.Shotgun_S_P, TR2Type.ShotgunAmmo_M_H, TR2Type.ShotgunAmmo_S_P },
        [TR2Type.ScubaDiver] =
            new() { TR2Type.ScubaHarpoonProjectile_H },
        [TR2Type.Shark] =
            new() { TR2Type.LaraMiscAnim_H_Unwater },
        [TR2Type.StickWieldingGoon2] =
            new() { TR2Type.StickWieldingGoon1GreenVest },
        [TR2Type.TRex] =
            new() { TR2Type.LaraMiscAnim_H_Wall },
        [TR2Type.Uzi_M_H] =
            new() { TR2Type.LaraUziAnim_H, TR2Type.Gunflare_H, TR2Type.Uzi_S_P, TR2Type.UziAmmo_M_H, TR2Type.Uzi_S_P },
        [TR2Type.WaterfallMist_N]
            = new() { TR2Type.WaterRipples_S_H },
        [TR2Type.XianGuardSpear] =
            new() { TR2Type.LaraMiscAnim_H_Xian, TR2Type.XianGuardSpearStatue },
        [TR2Type.XianGuardSword] =
            new() { TR2Type.XianGuardSwordStatue, TR2Type.XianGuardSparkles_S_H },
        [TR2Type.Yeti] =
            new() { TR2Type.LaraMiscAnim_H_Ice },
        
    };

    private static readonly List<TR2Type> _cinematicTypes = new()
    {
        TR2Type.DragonExplosionEmitter_N
    };

    private static readonly Dictionary<TR2Type, List<TR2Type>> _typeAliases = new()
    {
        [TR2Type.Lara] = new()
        {
            TR2Type.LaraSun, TR2Type.LaraUnwater, TR2Type.LaraSnow, TR2Type.LaraHome
        },

        [TR2Type.LaraPistolAnim_H] = new()
        {
            TR2Type.LaraPistolAnim_H_Sun, TR2Type.LaraPistolAnim_H_Unwater, TR2Type.LaraPistolAnim_H_Snow, TR2Type.LaraPistolAnim_H_Home
        },
        [TR2Type.LaraAutoAnim_H] = new()
        {
            TR2Type.LaraAutoAnim_H_Sun, TR2Type.LaraAutoAnim_H_Unwater, TR2Type.LaraAutoAnim_H_Snow, TR2Type.LaraAutoAnim_H_Home
        },
        [TR2Type.LaraUziAnim_H] = new()
        {
            TR2Type.LaraUziAnim_H_Sun, TR2Type.LaraUziAnim_H_Unwater, TR2Type.LaraUziAnim_H_Snow, TR2Type.LaraUziAnim_H_Home
        },

        [TR2Type.LaraMiscAnim_H] = new()
        {
            TR2Type.LaraMiscAnim_H_Wall, TR2Type.LaraMiscAnim_H_Unwater, TR2Type.LaraMiscAnim_H_Ice, TR2Type.LaraMiscAnim_H_Xian, TR2Type.LaraMiscAnim_H_HSH, TR2Type.LaraMiscAnim_H_Venice
        },

        [TR2Type.TigerOrSnowLeopard] = new()
        {
            TR2Type.BengalTiger, TR2Type.SnowLeopard, TR2Type.WhiteTiger
        },

        [TR2Type.StickWieldingGoon1] = new()
        {
            TR2Type.StickWieldingGoon1Bandana, TR2Type.StickWieldingGoon1BlackJacket, TR2Type.StickWieldingGoon1BodyWarmer, TR2Type.StickWieldingGoon1GreenVest, TR2Type.StickWieldingGoon1WhiteVest
        },

        [TR2Type.FlamethrowerGoon] = new()
        {
            TR2Type.FlamethrowerGoonOG, TR2Type.FlamethrowerGoonTopixtor
        },

        [TR2Type.Gunman1] = new()
        {
            TR2Type.Gunman1OG, TR2Type.Gunman1TopixtorORC, TR2Type.Gunman1TopixtorCAC
        },

        [TR2Type.Barracuda] = new()
        {
            TR2Type.BarracudaIce, TR2Type.BarracudaUnwater, TR2Type.BarracudaXian
        },

        [TR2Type.Puzzle1_M_H] = new()
        {
            TR2Type.Puzzle1_M_H_CircuitBoard, TR2Type.Puzzle1_M_H_CircuitBreaker, TR2Type.Puzzle1_M_H_Dagger, TR2Type.Puzzle1_M_H_DragonSeal, TR2Type.Puzzle1_M_H_MysticPlaque,
            TR2Type.Puzzle1_M_H_PrayerWheel, TR2Type.Puzzle1_M_H_RelayBox, TR2Type.Puzzle1_M_H_TibetanMask
        },
        [TR2Type.Puzzle2_M_H] = new()
        {
            TR2Type.Puzzle2_M_H_CircuitBoard, TR2Type.Puzzle2_M_H_Dagger, TR2Type.Puzzle2_M_H_GemStone, TR2Type.Puzzle2_M_H_MysticPlaque
        },
        [TR2Type.Puzzle4_M_H] = new()
        {
            TR2Type.Puzzle4_M_H_Seraph
        }
    };

    private static readonly List<TR2Type> _permittedAliasDuplicates = new()
    {
        TR2Type.LaraMiscAnim_H
    };

    private static readonly List<TR2Type> _permittedOverrides = new()
    {
        TR2Type.MarcoBartoli
    };

    private static readonly List<TR2Type> _nonGraphicsDependencies = new()
    {
        TR2Type.StickWieldingGoon1GreenVest, TR2Type.MaskedGoon1, TR2Type.Mercenary2
    };

    private static readonly Dictionary<TR2Type, List<TR2SFX>> _hardcodedSFX = new()
    {
        [TR2Type.Boat] = new()
        {
            TR2SFX.BoatIdle,
            TR2SFX.BoatMoving,
            TR2SFX.BoatEngine,
            TR2SFX.BoatIntoWater,
        },
        [TR2Type.DragonExplosion1_H] = new()
        {
            TR2SFX.SphereOfDoom,
        },
        [TR2Type.DragonFront_H] = new()
        {
            TR2SFX.DragonFire,
        },
        [TR2Type.Flame_S_H] = new()
        {
            TR2SFX.LoopForSmallFires,
        },
        [TR2Type.RedSnowmobile] = new()
        {
            TR2SFX.SkidooIdle,
            TR2SFX.SkidooMoving,
        },
        [TR2Type.StickWieldingGoon1BodyWarmer] = new()
        {
            TR2SFX.EnemyHit1,
            TR2SFX.EnemyHit2,
            TR2SFX.EnemyThump,
        },
        [TR2Type.StickWieldingGoon1WhiteVest] = new()
        {
            TR2SFX.EnemyHit1,
            TR2SFX.EnemyHit2,
            TR2SFX.EnemyThump,
        },
        [TR2Type.StickWieldingGoon1Bandana] = new()
        {
            TR2SFX.EnemyHit1,
            TR2SFX.EnemyHit2,
            TR2SFX.EnemyThump,
        },
        [TR2Type.StickWieldingGoon1GreenVest] = new()
        {
            TR2SFX.EnemyHit1,
            TR2SFX.EnemyHit2,
            TR2SFX.EnemyThump,
        },
        [TR2Type.StickWieldingGoon1BlackJacket] = new()
        {
            TR2SFX.EnemyHit1,
            TR2SFX.EnemyHit2,
            TR2SFX.EnemyThump,
        },
        [TR2Type.StickWieldingGoon2] = new()
        {
            TR2SFX.EnemyFeet,
            TR2SFX.EnemyGrunt,
            TR2SFX.EnemyHit1,
            TR2SFX.EnemyHit2,
            TR2SFX.EnemyBeltJingle,
            TR2SFX.EnemyWrench,
            TR2SFX.Footstep,
            TR2SFX.FootstepHit,
        },
        [TR2Type.XianGuardSword] = new()
        {
            TR2SFX.WarriorHover,
        },
        [TR2Type.Winston] = new()
        {
            TR2SFX.WinstonGrunt1,
            TR2SFX.WinstonGrunt2,
            TR2SFX.WinstonGrunt3,
            TR2SFX.WinstonCups,
        }
    };

    private static readonly List<TR2Type> _spriteTypes = new()
    {
        TR2Type.BoatWake_S_H,
        TR2Type.SnowmobileWake_S_H,
        TR2Type.UIFrame_H,
        TR2Type.Pistols_S_P,
        TR2Type.Shotgun_S_P,
        TR2Type.Automags_S_P,
        TR2Type.Uzi_S_P,
        TR2Type.Harpoon_S_P,
        TR2Type.M16_S_P,
        TR2Type.GrenadeLauncher_S_P,
        TR2Type.ShotgunAmmo_S_P,
        TR2Type.AutoAmmo_S_P,
        TR2Type.UziAmmo_S_P,
        TR2Type.HarpoonAmmo_S_P,
        TR2Type.M16Ammo_S_P,
        TR2Type.Grenades_S_P,
        TR2Type.SmallMed_S_P,
        TR2Type.LargeMed_S_P,
        TR2Type.Flares_S_P,
        TR2Type.Puzzle1_S_P,
        TR2Type.Puzzle2_S_P,
        TR2Type.Puzzle4_S_P,
        TR2Type.GoldSecret_S_P,
        TR2Type.JadeSecret_S_P,
        TR2Type.StoneSecret_S_P,
        TR2Type.Key1_S_P,
        TR2Type.Key2_S_P,
        TR2Type.Key3_S_P,
        TR2Type.Key4_S_P,
        TR2Type.Quest1_S_P,
        TR2Type.Quest2_S_P,
        TR2Type.ExtraFire_S_H,
        TR2Type.GrayDisk_S_H,
        TR2Type.Explosion_S_H,
        TR2Type.WaterRipples_S_H,
        TR2Type.Bubbles1_S_H,
        TR2Type.Blood_S_H,
        TR2Type.DartEffect_S_H,
        TR2Type.Glow_S_H,
        TR2Type.Ricochet_S_H,
        TR2Type.XianGuardSparkles_S_H,
        TR2Type.FireBlast_S_H,
        TR2Type.LavaParticles_S_H,
        TR2Type.Flame_S_H,
        TR2Type.FontGraphics_S_H,
        TR2Type.AssaultNumbers,
    };

    #endregion
}
