using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRDataControl;

public class TR3DataProvider : IDataProvider<TR3Type, TR3SFX>
{
    public int TextureTileLimit { get; set; } = 32;
    public int TextureObjectLimit { get; set; } = 4096;

    public Dictionary<TR3Type, TR3Type> AliasPriority { get; set; }

    public TRBlobType GetBlobType(TR3Type type)
    {
        if (_spriteTypes.Contains(type))
        {
            return TRBlobType.Sprite;
        }
        if (type >= TR3Type.SceneryBase && type < TR3Type.CobraIndia)
        {
            return TRBlobType.StaticMesh;
        }
        return TRBlobType.Model;
    }

    public IEnumerable<TR3Type> GetDependencies(TR3Type type)
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

    public IEnumerable<TR3Type> GetRemovalExclusions(TR3Type type)
    {
        return _emptyTypes;
    }

    public IEnumerable<TR3Type> GetCyclicDependencies(TR3Type type)
    {
        return _emptyTypes;
    }

    public IEnumerable<TR3Type> GetCinematicTypes()
    {
        return _emptyTypes;
    }

    public bool IsAlias(TR3Type type)
    {
        foreach (List<TR3Type> aliases in _typeAliases.Values)
        {
            if (aliases.Contains(type))
            {
                return true;
            }
        }

        return false;
    }

    public bool HasAliases(TR3Type type)
    {
        return _typeAliases.ContainsKey(type);
    }

    public TR3Type TranslateAlias(TR3Type type)
    {
        foreach (TR3Type root in _typeAliases.Keys)
        {
            if (_typeAliases[root].Contains(type))
            {
                return root;
            }
        }

        return type;
    }

    public IEnumerable<TR3Type> GetAliases(TR3Type type)
    {
        return _typeAliases.ContainsKey(type) ? _typeAliases[type] : _emptyTypes;
    }

    public TR3Type GetLevelAlias(string level, TR3Type type)
    {
        return TR3TypeUtilities.GetAliasForLevel(level, type);
    }

    public bool IsAliasDuplicatePermitted(TR3Type type)
    {
        return _permittedAliasDuplicates.Contains(type);
    }

    public bool IsOverridePermitted(TR3Type type)
    {
        return _permittedOverrides.Contains(type);
    }

    public bool IsNonGraphicsDependency(TR3Type type)
    {
        return _nonGraphicsDependencies.Contains(type);
    }

    public IEnumerable<TR3SFX> GetHardcodedSounds(TR3Type type)
    {
        return _hardcodedSFX.ContainsKey(type)
            ? _hardcodedSFX[type]
            : _emptySFX;
    }

    #region Data

    private static readonly List<TR3Type> _emptyTypes = new();
    private static readonly List<TR3SFX> _emptySFX = new();

    private static readonly Dictionary<TR3Type, List<TR3Type>> _typeDependencies = new()
    {
        [TR3Type.LaraIndia]
            = new() { TR3Type.LaraSkin_H_India, TR3Type.LaraPistolAnimation_H_India, TR3Type.LaraDeagleAnimation_H_India, TR3Type.LaraUziAnimation_H_India },

        [TR3Type.LaraCoastal]
            = new() { TR3Type.LaraSkin_H_Coastal, TR3Type.LaraPistolAnimation_H_Coastal, TR3Type.LaraDeagleAnimation_H_Coastal, TR3Type.LaraUziAnimation_H_Coastal },

        [TR3Type.LaraLondon]
            = new() { TR3Type.LaraSkin_H_London, TR3Type.LaraPistolAnimation_H_London, TR3Type.LaraDeagleAnimation_H_London, TR3Type.LaraUziAnimation_H_London },

        [TR3Type.LaraNevada]
            = new() { TR3Type.LaraSkin_H_Nevada, TR3Type.LaraPistolAnimation_H_Nevada, TR3Type.LaraDeagleAnimation_H_Nevada, TR3Type.LaraUziAnimation_H_Nevada },

        [TR3Type.LaraAntarc]
            = new() { TR3Type.LaraSkin_H_Antarc, TR3Type.LaraPistolAnimation_H_Antarc, TR3Type.LaraDeagleAnimation_H_Antarc, TR3Type.LaraUziAnimation_H_Antarc },

        [TR3Type.LaraHome]
            = new() { TR3Type.LaraSkin_H_Home, TR3Type.LaraPistolAnimation_H_Home },

        [TR3Type.Monkey]
            = new() { TR3Type.MonkeyMedMeshswap, TR3Type.MonkeyKeyMeshswap },

        [TR3Type.Shiva]
            = new() { TR3Type.ShivaStatue, TR3Type.LaraExtraAnimation_H },

        [TR3Type.Quad]
            = new() { TR3Type.LaraVehicleAnimation_H_Quad },

        [TR3Type.Kayak]
            = new() { TR3Type.LaraVehicleAnimation_H_Kayak },

        [TR3Type.UPV]
            = new() { TR3Type.LaraVehicleAnimation_H_UPV },

        [TR3Type.Boat]
            = new() { TR3Type.LaraVehicleAnimation_H_Boat },

        [TR3Type.Tyrannosaur]
             = new() { TR3Type.LaraExtraAnimation_H },

        [TR3Type.Willie]
             = new() { TR3Type.LaraExtraAnimation_H, TR3Type.AIPath_N },

        [TR3Type.Infada_P]
             = new() { TR3Type.Infada_M_H },
        [TR3Type.OraDagger_P]
             = new() { TR3Type.OraDagger_M_H },
        [TR3Type.EyeOfIsis_P]
             = new() { TR3Type.EyeOfIsis_M_H },
        [TR3Type.Element115_P]
             = new() { TR3Type.Element115_M_H },

        [TR3Type.Quest1_P]
            = new() { TR3Type.Quest1_M_H },
        [TR3Type.Quest2_P]
            = new() { TR3Type.Quest2_M_H },

        [TR3Type.Pistols_P] =
            new() { TR3Type.LaraPistolAnimation_H, TR3Type.Gunflare_H, TR3Type.Pistols_M_H, TR3Type.PistolAmmo_P, TR3Type.PistolAmmo_M_H },
        [TR3Type.Shotgun_P] =
            new() { TR3Type.LaraShotgunAnimation_H, TR3Type.Gunflare_H, TR3Type.Shotgun_M_H, TR3Type.ShotgunAmmo_P, TR3Type.ShotgunAmmo_M_H },
        [TR3Type.Deagle_P] =
            new() { TR3Type.LaraDeagleAnimation_H, TR3Type.Gunflare_H, TR3Type.Deagle_M_H, TR3Type.DeagleAmmo_P, TR3Type.DeagleAmmo_M_H },
        [TR3Type.Uzis_P] =
            new() { TR3Type.LaraUziAnimation_H, TR3Type.Gunflare_H, TR3Type.Uzis_M_H, TR3Type.UziAmmo_P, TR3Type.UziAmmo_M_H },
        [TR3Type.Harpoon_P] =
            new() { TR3Type.LaraHarpoonAnimation_H, TR3Type.Harpoon_M_H, TR3Type.Harpoons_P, TR3Type.Harpoons_M_H, TR3Type.HarpoonSingle2 },
        [TR3Type.MP5_P] =
            new() { TR3Type.LaraMP5Animation_H, TR3Type.GunflareMP5_H, TR3Type.MP5_M_H, TR3Type.MP5Ammo_P, TR3Type.MP5Ammo_M_H },
        [TR3Type.RocketLauncher_P] =
            new() { TR3Type.LaraRocketAnimation_H, TR3Type.RocketLauncher_M_H, TR3Type.Rockets_P, TR3Type.Rockets_M_H, TR3Type.RocketSingle },
        [TR3Type.GrenadeLauncher_P] =
            new() { TR3Type.LaraGrenadeAnimation_H, TR3Type.GrenadeLauncher_M_H, TR3Type.Grenades_P, TR3Type.Grenades_M_H, TR3Type.GrenadeSingle },
    };

    private static readonly Dictionary<TR3Type, List<TR3Type>> _typeAliases = new()
    {
        [TR3Type.Lara] = new()
        {
            TR3Type.LaraIndia, TR3Type.LaraCoastal, TR3Type.LaraLondon, TR3Type.LaraNevada, TR3Type.LaraAntarc, TR3Type.LaraHome
        },
        [TR3Type.LaraSkin_H] = new()
        {
            TR3Type.LaraSkin_H_India, TR3Type.LaraSkin_H_Coastal, TR3Type.LaraSkin_H_London, TR3Type.LaraSkin_H_Nevada, TR3Type.LaraSkin_H_Antarc, TR3Type.LaraSkin_H_Home
        },

        [TR3Type.LaraPistolAnimation_H] = new()
        {
            TR3Type.LaraPistolAnimation_H_India, TR3Type.LaraPistolAnimation_H_Coastal, TR3Type.LaraPistolAnimation_H_London, TR3Type.LaraPistolAnimation_H_Nevada, TR3Type.LaraPistolAnimation_H_Antarc, TR3Type.LaraPistolAnimation_H_Home
        },
        [TR3Type.LaraDeagleAnimation_H] = new()
        {
            TR3Type.LaraDeagleAnimation_H_India, TR3Type.LaraDeagleAnimation_H_Coastal, TR3Type.LaraDeagleAnimation_H_London, TR3Type.LaraDeagleAnimation_H_Nevada, TR3Type.LaraDeagleAnimation_H_Antarc, TR3Type.LaraDeagleAnimation_H_Home
        },
        [TR3Type.LaraUziAnimation_H] = new()
        {
            TR3Type.LaraUziAnimation_H_India, TR3Type.LaraUziAnimation_H_Coastal, TR3Type.LaraUziAnimation_H_London, TR3Type.LaraUziAnimation_H_Nevada, TR3Type.LaraUziAnimation_H_Antarc, TR3Type.LaraUziAnimation_H_Home
        },

        [TR3Type.LaraVehicleAnimation_H] = new()
        {
            TR3Type.LaraVehicleAnimation_H_Quad, TR3Type.LaraVehicleAnimation_H_BigGun, TR3Type.LaraVehicleAnimation_H_Kayak, TR3Type.LaraVehicleAnimation_H_UPV, TR3Type.LaraVehicleAnimation_H_Boat
        },

        [TR3Type.Cobra] = new()
        {
            TR3Type.CobraIndia, TR3Type.CobraNevada
        },

        [TR3Type.Dog] = new()
        {
            TR3Type.DogLondon, TR3Type.DogNevada
        }
    };

    private static readonly List<TR3Type> _permittedAliasDuplicates = new()
    {
        TR3Type.LaraVehicleAnimation_H
    };

    private static readonly List<TR3Type> _permittedOverrides = new()
    {
        TR3Type.Infada_M_H, TR3Type.EyeOfIsis_M_H, TR3Type.OraDagger_M_H, TR3Type.Element115_M_H
    };

    private static readonly List<TR3Type> _nonGraphicsDependencies = new()
    {
        TR3Type.Monkey
    };

    private static readonly Dictionary<TR3Type, List<TR3SFX>> _hardcodedSFX = new()
    {
        [TR3Type.Quad] = new()
        {
            TR3SFX.QuadStart,
            TR3SFX.QuadIdle,
            TR3SFX.QuadAccelerate,
            TR3SFX.QuadMove,
            TR3SFX.QuadStop,
        },
        [TR3Type.TonyFirehands] = new()
        {
            TR3SFX.BlastCircle,
            TR3SFX.TonyBossStoneDeath,
            TR3SFX.TonyBossNormalDeath,
        },
        [TR3Type.Puna] = new()
        {
            TR3SFX.BlastCircle,
            TR3SFX.TribossTurnChair,
        },
        [TR3Type.LondonMerc] = new()
        {
            TR3SFX.EnglishOi,
        },
        [TR3Type.LondonGuard] = new()
        {
            TR3SFX.EnglishOi,
        },
        [TR3Type.Punk] = new()
        {
            TR3SFX.EnglishOi,
        },
        [TR3Type.UPV] = new()
        {
            TR3SFX.UPVLoop,
            TR3SFX.UPVStart,
            TR3SFX.UPVStop,
        },
        [TR3Type.MPWithStick] = new()
        {
            TR3SFX.AmercanHey,
        },
        [TR3Type.MPWithGun] = new()
        {
            TR3SFX.AmercanHey,
        },
        [TR3Type.MPWithMP5] = new()
        {
            TR3SFX.AmercanHey,
        },
        [TR3Type.DamGuard] = new()
        {
            TR3SFX.AmercanHey,
        },
        [TR3Type.Prisoner] = new()
        {
            TR3SFX.AmercanHey,
        },
        [TR3Type.RXRedBoi] = new()
        {
            TR3SFX.AmercanHey,
        },
        [TR3Type.RXGunLad] = new()
        {
            TR3SFX.AmercanHey,
        },
        [TR3Type.Boat] = new()
        {
            TR3SFX.BoatStart,
            TR3SFX.BoatIdle,
            TR3SFX.BoatAccelerate,
            TR3SFX.BoatMoving,
            TR3SFX.BoatStop,
            TR3SFX.BoatSlowDown,
        },
        [TR3Type.Winston] = new()
        {
            TR3SFX.WinstonBrushOff,
            TR3SFX.WinstonBulletTray,
            TR3SFX.WinstonGetUp,
        },
    };

    private static readonly List<TR3Type> _spriteTypes = new()
    {
        TR3Type.UIFrame_S_H,
        TR3Type.ShadowSprite_S_H,
        TR3Type.MiscSprites_S_H,
        TR3Type.Bubble_S_H,
        TR3Type.Glow_S_H,
        TR3Type.Glow2_S_H,
        TR3Type.FontGraphics_S_H,
        TR3Type.TimerFontGraphics_S_H,
    };

    #endregion
}