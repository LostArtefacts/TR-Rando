using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRModelTransporter.Data;

public class TR3DefaultDataProvider : ITransportDataProvider<TR3Type>
{
    public int TextureTileLimit { get; set; } = 32;
    public int TextureObjectLimit { get; set; } = 4096;

    public Dictionary<TR3Type, TR3Type> AliasPriority { get; set; }

    public IEnumerable<TR3Type> GetModelDependencies(TR3Type entity)
    {
        return _entityDependencies.ContainsKey(entity) ? _entityDependencies[entity] : _emptyEntities;
    }

    public IEnumerable<TR3Type> GetRemovalExclusions(TR3Type entity)
    {
        return _emptyEntities;
    }

    public IEnumerable<TR3Type> GetCyclicDependencies(TR3Type entity)
    {
        return _emptyEntities;
    }

    public IEnumerable<TR3Type> GetSpriteDependencies(TR3Type entity)
    {
        return _spriteDependencies.ContainsKey(entity) ? _spriteDependencies[entity] : _emptyEntities;
    }

    public IEnumerable<TR3Type> GetCinematicEntities()
    {
        return _cinematicEntities;
    }

    public IEnumerable<TR3Type> GetLaraDependants()
    {
        return _laraDependentModels;
    }

    public bool IsAlias(TR3Type entity)
    {
        foreach (List<TR3Type> aliases in _entityAliases.Values)
        {
            if (aliases.Contains(entity))
            {
                return true;
            }
        }

        return false;
    }

    public bool HasAliases(TR3Type entity)
    {
        return _entityAliases.ContainsKey(entity);
    }

    public TR3Type TranslateAlias(TR3Type entity)
    {
        foreach (TR3Type root in _entityAliases.Keys)
        {
            if (_entityAliases[root].Contains(entity))
            {
                return root;
            }
        }

        return entity;
    }

    public IEnumerable<TR3Type> GetAliases(TR3Type entity)
    {
        return _entityAliases.ContainsKey(entity) ? _entityAliases[entity] : _emptyEntities;
    }

    public TR3Type GetLevelAlias(string level, TR3Type entity)
    {
        return TR3TypeUtilities.GetAliasForLevel(level, entity);
    }

    public bool IsAliasDuplicatePermitted(TR3Type entity)
    {
        return _permittedAliasDuplicates.Contains(entity);
    }

    public bool IsOverridePermitted(TR3Type entity)
    {
        return _permittedOverrides.Contains(entity);
    }

    public IEnumerable<TR3Type> GetUnsafeModelReplacements()
    {
        return _unsafeModelReplacements;
    }

    public bool IsNonGraphicsDependency(TR3Type entity)
    {
        return _nonGraphicsDependencies.Contains(entity);
    }

    public bool IsSoundOnlyDependency(TR3Type entity)
    {
        return _soundOnlyDependencies.Contains(entity);
    }

    public short[] GetHardcodedSounds(TR3Type entity)
    {
        return _hardcodedSoundIndices.ContainsKey(entity) ? _hardcodedSoundIndices[entity] : null;
    }

    public IEnumerable<int> GetIgnorableTextureIndices(TR3Type entity, string level)
    {
        return _ignoreEntityTextures.ContainsKey(entity) ? _ignoreEntityTextures[entity] : null;
    }

    #region Data

    private static readonly IEnumerable<TR3Type> _emptyEntities = new List<TR3Type>();

    private static readonly Dictionary<TR3Type, TR3Type[]> _entityDependencies = new()
    {
        [TR3Type.LaraIndia]
            = new TR3Type[] { TR3Type.LaraSkin_H_India, TR3Type.LaraPistolAnimation_H_India, TR3Type.LaraDeagleAnimation_H_India, TR3Type.LaraUziAnimation_H_India },

        [TR3Type.LaraCoastal]
            = new TR3Type[] { TR3Type.LaraSkin_H_Coastal, TR3Type.LaraPistolAnimation_H_Coastal, TR3Type.LaraDeagleAnimation_H_Coastal, TR3Type.LaraUziAnimation_H_Coastal },

        [TR3Type.LaraLondon]
            = new TR3Type[] { TR3Type.LaraSkin_H_London, TR3Type.LaraPistolAnimation_H_London, TR3Type.LaraDeagleAnimation_H_London, TR3Type.LaraUziAnimation_H_London },

        [TR3Type.LaraNevada]
            = new TR3Type[] { TR3Type.LaraSkin_H_Nevada, TR3Type.LaraPistolAnimation_H_Nevada, TR3Type.LaraDeagleAnimation_H_Nevada, TR3Type.LaraUziAnimation_H_Nevada },

        [TR3Type.LaraAntarc]
            = new TR3Type[] { TR3Type.LaraSkin_H_Antarc, TR3Type.LaraPistolAnimation_H_Antarc, TR3Type.LaraDeagleAnimation_H_Antarc, TR3Type.LaraUziAnimation_H_Antarc },

        [TR3Type.LaraHome]
            = new TR3Type[] { TR3Type.LaraSkin_H_Home, TR3Type.LaraPistolAnimation_H_Home/*, TR3Entities.LaraDeagleAnimation_H_Home, TR3Entities.LaraUziAnimation_H_Home*/ },

        [TR3Type.Monkey]
            = new TR3Type[] { TR3Type.MonkeyMedMeshswap, TR3Type.MonkeyKeyMeshswap },

        [TR3Type.Shiva]
            = new TR3Type[] { TR3Type.ShivaStatue, TR3Type.LaraExtraAnimation_H, TR3Type.Monkey },

        [TR3Type.Quad]
            = new TR3Type[] { TR3Type.LaraVehicleAnimation_H_Quad },

        [TR3Type.Kayak]
            = new TR3Type[] { TR3Type.LaraVehicleAnimation_H_Kayak },

        [TR3Type.UPV]
            = new TR3Type[] { TR3Type.LaraVehicleAnimation_H_UPV },

        [TR3Type.Boat]
            = new TR3Type[] { TR3Type.LaraVehicleAnimation_H_Boat },

        [TR3Type.Tyrannosaur]
             = new TR3Type[] { TR3Type.LaraExtraAnimation_H },

        [TR3Type.Willie]
             = new TR3Type[] { TR3Type.LaraExtraAnimation_H, TR3Type.AIPath_N },

        [TR3Type.Infada_P]
             = new TR3Type[] { TR3Type.Infada_M_H },
        [TR3Type.OraDagger_P]
             = new TR3Type[] { TR3Type.OraDagger_M_H },
        [TR3Type.EyeOfIsis_P]
             = new TR3Type[] { TR3Type.EyeOfIsis_M_H },
        [TR3Type.Element115_P]
             = new TR3Type[] { TR3Type.Element115_M_H },

        [TR3Type.Quest1_P]
            = new TR3Type[] { TR3Type.Quest1_M_H },
        [TR3Type.Quest2_P]
            = new TR3Type[] { TR3Type.Quest2_M_H },

        [TR3Type.MP5_P]
            = new TR3Type[] { TR3Type.GunflareMP5_H },
        [TR3Type.RocketLauncher_P]
            = new TR3Type[] { TR3Type.RocketSingle },
        [TR3Type.GrenadeLauncher_P]
            = new TR3Type[] { TR3Type.GrenadeSingle },
        [TR3Type.Harpoon_P]
            = new TR3Type[] { TR3Type.HarpoonSingle2 }
    };

    private static readonly Dictionary<TR3Type, List<TR3Type>> _spriteDependencies = new()
    {
        
    };

    private static readonly List<TR3Type> _cinematicEntities = new()
    {
        
    };

    private static readonly List<TR3Type> _laraDependentModels = new()
    {
        
    };

    private static readonly Dictionary<TR3Type, List<TR3Type>> _entityAliases = new()
    {
        [TR3Type.Lara] = new List<TR3Type>
        {
            TR3Type.LaraIndia, TR3Type.LaraCoastal, TR3Type.LaraLondon, TR3Type.LaraNevada, TR3Type.LaraAntarc, TR3Type.LaraHome
        },
        [TR3Type.LaraSkin_H] = new List<TR3Type>
        {
            TR3Type.LaraSkin_H_India, TR3Type.LaraSkin_H_Coastal, TR3Type.LaraSkin_H_London, TR3Type.LaraSkin_H_Nevada, TR3Type.LaraSkin_H_Antarc, TR3Type.LaraSkin_H_Home
        },

        [TR3Type.LaraPistolAnimation_H] = new List<TR3Type>
        {
            TR3Type.LaraPistolAnimation_H_India, TR3Type.LaraPistolAnimation_H_Coastal, TR3Type.LaraPistolAnimation_H_London, TR3Type.LaraPistolAnimation_H_Nevada, TR3Type.LaraPistolAnimation_H_Antarc, TR3Type.LaraPistolAnimation_H_Home
        },
        [TR3Type.LaraDeagleAnimation_H] = new List<TR3Type>
        {
            TR3Type.LaraDeagleAnimation_H_India, TR3Type.LaraDeagleAnimation_H_Coastal, TR3Type.LaraDeagleAnimation_H_London, TR3Type.LaraDeagleAnimation_H_Nevada, TR3Type.LaraDeagleAnimation_H_Antarc, TR3Type.LaraDeagleAnimation_H_Home
        },
        [TR3Type.LaraUziAnimation_H] = new List<TR3Type>
        {
            TR3Type.LaraUziAnimation_H_India, TR3Type.LaraUziAnimation_H_Coastal, TR3Type.LaraUziAnimation_H_London, TR3Type.LaraUziAnimation_H_Nevada, TR3Type.LaraUziAnimation_H_Antarc, TR3Type.LaraUziAnimation_H_Home
        },

        [TR3Type.LaraVehicleAnimation_H] = new List<TR3Type>
        {
            TR3Type.LaraVehicleAnimation_H_Quad, TR3Type.LaraVehicleAnimation_H_BigGun, TR3Type.LaraVehicleAnimation_H_Kayak, TR3Type.LaraVehicleAnimation_H_UPV, TR3Type.LaraVehicleAnimation_H_Boat
        },

        [TR3Type.Cobra] = new List<TR3Type>
        {
            TR3Type.CobraIndia, TR3Type.CobraNevada
        },

        [TR3Type.Dog] = new List<TR3Type>
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

    private static readonly List<TR3Type> _unsafeModelReplacements = new()
    {
         TR3Type.Lara, TR3Type.LaraSkin_H, TR3Type.LaraPistolAnimation_H, TR3Type.LaraUziAnimation_H, TR3Type.LaraDeagleAnimation_H
    };

    private static readonly List<TR3Type> _nonGraphicsDependencies = new()
    {
        TR3Type.Monkey
    };

    // If these are imported into levels that already have another alias for them, only their hardcoded sounds will be imported
    protected static readonly List<TR3Type> _soundOnlyDependencies = new()
    {
        
    };

    private static readonly Dictionary<TR3Type, short[]> _hardcodedSoundIndices = new()
    {
        [TR3Type.Quad] = new short[]
        {
            152, // Starting
            153, // Idling
            154, // Switch off 1
            155, // High RPM,
            156  // Switch off 2
        },
        [TR3Type.TonyFirehands] = new short[]
        {
            76,  // Powering down
            234, // Dying
            235, // Dying
            236, // Laughing
            366, // Fireball1
            367, // Fireball2
            368  // Fireball3
        },
        [TR3Type.Puna] = new short[]
        {
            359  // Hoo-uh!
        },
        [TR3Type.LondonMerc] = new short[]
        {
            299  // Hey/Oi!
        },
        [TR3Type.LondonGuard] = new short[]
        {
            299, // Hey/Oi!
            305  // Gunshot
        },
        [TR3Type.Punk] = new short[]
        {
            299  // Hey/Oi!
        },
        [TR3Type.UPV] = new short[]
        {
            346, // Starting
            347, // Running
            348  // Stopping
        },
        [TR3Type.MPWithStick] = new short[]
        {
            300  // Hey!
        },
        [TR3Type.MPWithGun] = new short[]
        {
            300  // Hey!
        },
        [TR3Type.MPWithMP5] = new short[]
        {
            137, // Gunshot
            300  // Hey!
        },
        [TR3Type.DamGuard] = new short[]
        {
            300  // Hey!
        },
        [TR3Type.Prisoner] = new short[]
        {
            300  // Hey!
        },
        [TR3Type.RXRedBoi] = new short[]
        {
            300  // Hey!
        },
        [TR3Type.RXGunLad] = new short[]
        {
            300  // Hey!
        },
        [TR3Type.Boat] = new short[]
        {
            194, // Starting
            195, // Idling
            196, // Accelerating
            197, // High RPM
            198, // Stopping
            199  // Hitting something
        },
        [TR3Type.Winston] = new short[]
        {
            308, // Shuffle
            311, // Hit by shield
            314  // General grunt
        }
    };

    private static readonly Dictionary<TR3Type, List<int>> _ignoreEntityTextures = new()
    {
        [TR3Type.LaraVehicleAnimation_H]
            = new List<int>(), // empty list indicates to ignore everything
        [TR3Type.LaraExtraAnimation_H]
            = new List<int>()
    };

    #endregion
}
