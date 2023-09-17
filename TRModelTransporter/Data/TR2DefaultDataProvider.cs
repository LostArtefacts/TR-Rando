using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRModelTransporter.Data;

public class TR2DefaultDataProvider : ITransportDataProvider<TR2Type>
{
    public int TextureTileLimit { get; set; } = 16;
    public int TextureObjectLimit { get; set; } = 2048;

    public Dictionary<TR2Type, TR2Type> AliasPriority { get; set; }

    public IEnumerable<TR2Type> GetModelDependencies(TR2Type entity)
    {
        return _entityDependencies.ContainsKey(entity) ? _entityDependencies[entity] : _emptyEntities;
    }

    public IEnumerable<TR2Type> GetRemovalExclusions(TR2Type entity)
    {
        return _emptyEntities;
    }

    public IEnumerable<TR2Type> GetCyclicDependencies(TR2Type entity)
    {
        return _emptyEntities;
    }

    public IEnumerable<TR2Type> GetSpriteDependencies(TR2Type entity)
    {
        return _spriteDependencies.ContainsKey(entity) ? _spriteDependencies[entity] : _emptyEntities;
    }

    public IEnumerable<TR2Type> GetCinematicEntities()
    {
        return _cinematicEntities;
    }

    public IEnumerable<TR2Type> GetLaraDependants()
    {
        return _laraDependentModels;
    }

    public bool IsAlias(TR2Type entity)
    {
        foreach (List<TR2Type> aliases in _entityAliases.Values)
        {
            if (aliases.Contains(entity))
            {
                return true;
            }
        }

        return false;
    }

    public bool HasAliases(TR2Type entity)
    {
        return _entityAliases.ContainsKey(entity);
    }

    public TR2Type TranslateAlias(TR2Type entity)
    {
        foreach (TR2Type root in _entityAliases.Keys)
        {
            if (_entityAliases[root].Contains(entity))
            {
                return root;
            }
        }

        return entity;
    }

    public IEnumerable<TR2Type> GetAliases(TR2Type entity)
    {
        return _entityAliases.ContainsKey(entity) ? _entityAliases[entity] : _emptyEntities;
    }

    public TR2Type GetLevelAlias(string level, TR2Type entity)
    {
        return TR2EntityUtilities.GetAliasForLevel(level, entity);
    }

    public bool IsAliasDuplicatePermitted(TR2Type entity)
    {
        return _permittedAliasDuplicates.Contains(entity);
    }

    public bool IsOverridePermitted(TR2Type entity)
    {
        return _permittedOverrides.Contains(entity);
    }

    public IEnumerable<TR2Type> GetUnsafeModelReplacements()
    {
        return _unsafeModelReplacements;
    }

    public bool IsNonGraphicsDependency(TR2Type entity)
    {
        return _nonGraphicsDependencies.Contains(entity);
    }

    public bool IsSoundOnlyDependency(TR2Type entity)
    {
        return _soundOnlyDependencies.Contains(entity);
    }

    public short[] GetHardcodedSounds(TR2Type entity)
    {
        return _hardcodedSoundIndices.ContainsKey(entity) ? _hardcodedSoundIndices[entity] : null;
    }

    public IEnumerable<int> GetIgnorableTextureIndices(TR2Type entity, string level)
    {
        return _ignoreEntityTextures.ContainsKey(entity) ? _ignoreEntityTextures[entity] : null;
    }

    #region Data

    private static readonly IEnumerable<TR2Type> _emptyEntities = new List<TR2Type>();

    private static readonly Dictionary<TR2Type, TR2Type[]> _entityDependencies = new()
    {
        [TR2Type.LaraSun] =
            new TR2Type[] { TR2Type.LaraPistolAnim_H_Sun, TR2Type.LaraAutoAnim_H_Sun, TR2Type.LaraUziAnim_H_Sun },
        [TR2Type.LaraUnwater] =
            new TR2Type[] { TR2Type.LaraPistolAnim_H_Unwater, TR2Type.LaraAutoAnim_H_Unwater, TR2Type.LaraUziAnim_H_Unwater },
        [TR2Type.LaraSnow] =
            new TR2Type[] { TR2Type.LaraPistolAnim_H_Snow, TR2Type.LaraAutoAnim_H_Snow, TR2Type.LaraUziAnim_H_Snow },
        [TR2Type.LaraHome] =
            new TR2Type[] { TR2Type.LaraPistolAnim_H_Home, TR2Type.LaraAutoAnim_H_Home, TR2Type.LaraUziAnim_H_Home },

        [TR2Type.Pistols_M_H] =
            new TR2Type[] { TR2Type.LaraPistolAnim_H, TR2Type.Gunflare_H },
        [TR2Type.Shotgun_M_H] =
            new TR2Type[] { TR2Type.LaraShotgunAnim_H, TR2Type.Gunflare_H },
        [TR2Type.Autos_M_H] =
            new TR2Type[] { TR2Type.LaraAutoAnim_H, TR2Type.Gunflare_H },
        [TR2Type.Uzi_M_H] =
            new TR2Type[] { TR2Type.LaraUziAnim_H, TR2Type.Gunflare_H },
        [TR2Type.M16_M_H] =
            new TR2Type[] { TR2Type.LaraM16Anim_H, TR2Type.M16Gunflare_H },
        [TR2Type.Harpoon_M_H] =
            new TR2Type[] { TR2Type.LaraHarpoonAnim_H, TR2Type.HarpoonProjectile_H },
        [TR2Type.GrenadeLauncher_M_H] =
            new TR2Type[] { TR2Type.LaraGrenadeAnim_H, TR2Type.GrenadeProjectile_H },

        [TR2Type.TRex] =
            new TR2Type[] { TR2Type.LaraMiscAnim_H_Wall },
        [TR2Type.MaskedGoon2] =
            new TR2Type[] { TR2Type.MaskedGoon1 },
        [TR2Type.MaskedGoon3] =
            new TR2Type[] { TR2Type.MaskedGoon1 },
        [TR2Type.ScubaDiver] =
            new TR2Type[] { TR2Type.ScubaHarpoonProjectile_H },
        [TR2Type.Shark] =
            new TR2Type[] { TR2Type.LaraMiscAnim_H_Unwater },
        [TR2Type.StickWieldingGoon2] =
            new TR2Type[] { TR2Type.StickWieldingGoon1GreenVest },
        [TR2Type.MercSnowmobDriver] =
            new TR2Type[] { TR2Type.BlackSnowmob },
        [TR2Type.BlackSnowmob] =
            new TR2Type[] { TR2Type.RedSnowmobile },
        [TR2Type.RedSnowmobile] =
            new TR2Type[] { TR2Type.SnowmobileBelt, TR2Type.LaraSnowmobAnim_H },
        [TR2Type.Boat] =
            new TR2Type[] { TR2Type.LaraBoatAnim_H },
        [TR2Type.Mercenary3] =
            new TR2Type[] { TR2Type.Mercenary2 },
        [TR2Type.Yeti] =
            new TR2Type[] { TR2Type.LaraMiscAnim_H_Ice },
        [TR2Type.XianGuardSpear] =
            new TR2Type[] { TR2Type.LaraMiscAnim_H_Xian, TR2Type.XianGuardSpearStatue },
        [TR2Type.XianGuardSword] =
            new TR2Type[] { TR2Type.XianGuardSwordStatue },
        [TR2Type.Knifethrower] =
            new TR2Type[] { TR2Type.KnifeProjectile_H },
        [TR2Type.MarcoBartoli] =
            new TR2Type[]
            {
                TR2Type.DragonExplosionEmitter_N, TR2Type.DragonExplosion1_H, TR2Type.DragonExplosion2_H, TR2Type.DragonExplosion3_H,
                TR2Type.DragonFront_H, TR2Type.DragonBack_H, TR2Type.DragonBonesFront_H, TR2Type.DragonBonesBack_H, TR2Type.LaraMiscAnim_H_Xian,
                TR2Type.Puzzle2_M_H_Dagger
            }
    };

    private static readonly Dictionary<TR2Type, List<TR2Type>> _spriteDependencies = new()
    {
        [TR2Type.FlamethrowerGoon] 
            = new List<TR2Type> { TR2Type.Flame_S_H },
        [TR2Type.MarcoBartoli] 
            = new List<TR2Type> { TR2Type.Flame_S_H },
        [TR2Type.Boat] 
            = new List<TR2Type> { TR2Type.BoatWake_S_H },
        [TR2Type.RedSnowmobile] 
            = new List<TR2Type> { TR2Type.SnowmobileWake_S_H },
        [TR2Type.XianGuardSword] 
            = new List<TR2Type> { TR2Type.XianGuardSparkles_S_H },
        [TR2Type.WaterfallMist_N]
            = new List<TR2Type> { TR2Type.WaterRipples_S_H },
        [TR2Type.Key2_M_H]
            = new List<TR2Type> { TR2Type.Key2_S_P }
    };

    private static readonly List<TR2Type> _cinematicEntities = new()
    {
        TR2Type.DragonExplosionEmitter_N
    };

    // These are models that use Lara's hips as placeholders
    private static readonly List<TR2Type> _laraDependentModels = new()
    {
        TR2Type.CameraTarget_N, TR2Type.FlameEmitter_N, TR2Type.LaraCutscenePlacement_N,
        TR2Type.DragonExplosionEmitter_N, TR2Type.BartoliHideoutClock_N, TR2Type.SingingBirds_N,
        TR2Type.WaterfallMist_N, TR2Type.DrippingWater_N, TR2Type.LavaAirParticleEmitter_N,
        TR2Type.AlarmBell_N, TR2Type.DoorBell_N
    };

    private static readonly Dictionary<TR2Type, List<TR2Type>> _entityAliases = new()
    {
        [TR2Type.Lara] = new List<TR2Type>
        {
            TR2Type.LaraSun, TR2Type.LaraUnwater, TR2Type.LaraSnow, TR2Type.LaraHome
        },

        [TR2Type.LaraPistolAnim_H] = new List<TR2Type>
        {
            TR2Type.LaraPistolAnim_H_Sun, TR2Type.LaraPistolAnim_H_Unwater, TR2Type.LaraPistolAnim_H_Snow, TR2Type.LaraPistolAnim_H_Home
        },
        [TR2Type.LaraAutoAnim_H] = new List<TR2Type>
        {
            TR2Type.LaraAutoAnim_H_Sun, TR2Type.LaraAutoAnim_H_Unwater, TR2Type.LaraAutoAnim_H_Snow, TR2Type.LaraAutoAnim_H_Home
        },
        [TR2Type.LaraUziAnim_H] = new List<TR2Type>
        {
            TR2Type.LaraUziAnim_H_Sun, TR2Type.LaraUziAnim_H_Unwater, TR2Type.LaraUziAnim_H_Snow, TR2Type.LaraUziAnim_H_Home
        },

        [TR2Type.LaraMiscAnim_H] = new List<TR2Type>
        {
            TR2Type.LaraMiscAnim_H_Wall, TR2Type.LaraMiscAnim_H_Unwater, TR2Type.LaraMiscAnim_H_Ice, TR2Type.LaraMiscAnim_H_Xian, TR2Type.LaraMiscAnim_H_HSH, TR2Type.LaraMiscAnim_H_Venice
        },

        [TR2Type.TigerOrSnowLeopard] = new List<TR2Type>
        {
            TR2Type.BengalTiger, TR2Type.SnowLeopard, TR2Type.WhiteTiger
        },

        [TR2Type.StickWieldingGoon1] = new List<TR2Type>
        {
            TR2Type.StickWieldingGoon1Bandana, TR2Type.StickWieldingGoon1BlackJacket, TR2Type.StickWieldingGoon1BodyWarmer, TR2Type.StickWieldingGoon1GreenVest, TR2Type.StickWieldingGoon1WhiteVest
        },

        [TR2Type.FlamethrowerGoon] = new List<TR2Type>
        {
            TR2Type.FlamethrowerGoonOG, TR2Type.FlamethrowerGoonTopixtor
        },

        [TR2Type.Gunman1] = new List<TR2Type>
        {
            TR2Type.Gunman1OG, TR2Type.Gunman1TopixtorORC, TR2Type.Gunman1TopixtorCAC
        },

        [TR2Type.Barracuda] = new List<TR2Type>
        {
            TR2Type.BarracudaIce, TR2Type.BarracudaUnwater, TR2Type.BarracudaXian
        },

        [TR2Type.Puzzle1_M_H] = new List<TR2Type>
        {
            TR2Type.Puzzle1_M_H_CircuitBoard, TR2Type.Puzzle1_M_H_CircuitBreaker, TR2Type.Puzzle1_M_H_Dagger, TR2Type.Puzzle1_M_H_DragonSeal, TR2Type.Puzzle1_M_H_MysticPlaque,
            TR2Type.Puzzle1_M_H_PrayerWheel, TR2Type.Puzzle1_M_H_RelayBox, TR2Type.Puzzle1_M_H_TibetanMask
        },
        [TR2Type.Puzzle2_M_H] = new List<TR2Type>
        {
            TR2Type.Puzzle2_M_H_CircuitBoard, TR2Type.Puzzle2_M_H_Dagger, TR2Type.Puzzle2_M_H_GemStone, TR2Type.Puzzle2_M_H_MysticPlaque
        },
        [TR2Type.Puzzle4_M_H] = new List<TR2Type>
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

    private static readonly List<TR2Type> _unsafeModelReplacements = new()
    {
    };

    private static readonly List<TR2Type> _nonGraphicsDependencies = new()
    {
        TR2Type.StickWieldingGoon1GreenVest, TR2Type.MaskedGoon1, TR2Type.Mercenary2
    };

    // If these are imported into levels that already have another alias for them, only their hardcoded sounds will be imported
    protected static readonly List<TR2Type> _soundOnlyDependencies = new()
    {
        TR2Type.StickWieldingGoon1GreenVest
    };

    private static readonly Dictionary<TR2Type, short[]> _hardcodedSoundIndices = new()
    {
        [TR2Type.DragonExplosionEmitter_N] = new short[]
        {
            341  // Explosion when dragon spawns
        },
        [TR2Type.DragonFront_H] = new short[]
        {
            298, // Footstep
            299, // Growl 1
            300, // Growl 2
            301, // Body falling
            302, // Dying breath
            303, // Growl 3
            304, // Grunt
            305, // Fire-breathing
            306, // Leg lift
            307  // Leg hit
        },
        [TR2Type.Boat] = new short[]
        {
            194, // Start
            195, // Idling
            196, // Accelerating
            197, // High RPM
            198, // Shut off
            199, // Engine hit
            200, // Body hit
            336  // Dry land
        },
        [TR2Type.LaraSnowmobAnim_H] = new short[]
        {
            153, // Snowmobile idling
            155  // Snowmobile accelerating
        },
        [TR2Type.StickWieldingGoon1Bandana] = new short[]
        {
            71,  // Thump 1
            72,  // Thump 2
        },
        [TR2Type.StickWieldingGoon1BlackJacket] = new short[]
        {
            71,  // Thump 1
            72,  // Thump 2
        },
        [TR2Type.StickWieldingGoon1BodyWarmer] = new short[]
        {
            69,  // Footstep
            70,  // Grunt
            71,  // Thump 1
            72,  // Thump 2
            121  // Thump 3
        },
        [TR2Type.StickWieldingGoon1GreenVest] = new short[]
        {
            71,  // Thump 1
            72,  // Thump 2
        },
        [TR2Type.StickWieldingGoon1WhiteVest] = new short[]
        {
            71,  // Thump 1
            72,  // Thump 2,
            121  // Thump 3
        },
        [TR2Type.StickWieldingGoon2] = new short[]
        {
            69,  // Footstep
            70,  // Grunt
            71,  // Thump 1
            72,  // Thump 2
            180, // Chains
            181, // Chains
            182, // Footstep?
            183  // Another thump?
        },
        [TR2Type.XianGuardSword] = new short[]
        {
            312  // Hovering
        },
        [TR2Type.Winston] = new short[]
        {
            344, // Scared
            345, // Huff
            346, // Bumped
            347  // Cups clattering
        }
    };

    private static readonly Dictionary<TR2Type, List<int>> _ignoreEntityTextures = new()
    {
        [TR2Type.LaraMiscAnim_H] 
            = new List<int>(), // empty list indicates to ignore everything
        [TR2Type.WaterfallMist_N]
            = new List<int> { 0, 1, 2, 3, 4, 5, 6, 11, 15, 20, 22 },
        [TR2Type.LaraSnowmobAnim_H] 
            = new List<int> { 0, 1, 2, 4, 5, 6, 7, 8, 9, 10, 20, 21, 23 },
        [TR2Type.SnowmobileBelt] 
            = new List<int> { 0, 1, 2, 4, 5, 6, 7, 8, 9, 10, 20, 21, 23 },
        [TR2Type.DragonExplosionEmitter_N] 
            = new List<int> { 0, 1, 2, 3, 4, 5, 6, 8, 13, 14, 16, 17, 19 }
    };

    #endregion
}
