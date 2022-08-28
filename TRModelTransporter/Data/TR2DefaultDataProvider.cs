using System.Collections.Generic;
using TRLevelReader.Helpers;
using TRLevelReader.Model.Enums;

namespace TRModelTransporter.Data
{
    public class TR2DefaultDataProvider : ITransportDataProvider<TR2Entities>
    {
        public int TextureTileLimit { get; set; } = 16;
        public int TextureObjectLimit { get; set; } = 2048;

        public Dictionary<TR2Entities, TR2Entities> AliasPriority { get; set; }

        public IEnumerable<TR2Entities> GetModelDependencies(TR2Entities entity)
        {
            return _entityDependencies.ContainsKey(entity) ? _entityDependencies[entity] : _emptyEntities;
        }

        public IEnumerable<TR2Entities> GetRemovalExclusions(TR2Entities entity)
        {
            return _emptyEntities;
        }

        public IEnumerable<TR2Entities> GetCyclicDependencies(TR2Entities entity)
        {
            return _emptyEntities;
        }

        public IEnumerable<TR2Entities> GetSpriteDependencies(TR2Entities entity)
        {
            return _spriteDependencies.ContainsKey(entity) ? _spriteDependencies[entity] : _emptyEntities;
        }

        public IEnumerable<TR2Entities> GetCinematicEntities()
        {
            return _cinematicEntities;
        }

        public IEnumerable<TR2Entities> GetLaraDependants()
        {
            return _laraDependentModels;
        }

        public bool IsAlias(TR2Entities entity)
        {
            foreach (List<TR2Entities> aliases in _entityAliases.Values)
            {
                if (aliases.Contains(entity))
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasAliases(TR2Entities entity)
        {
            return _entityAliases.ContainsKey(entity);
        }

        public TR2Entities TranslateAlias(TR2Entities entity)
        {
            foreach (TR2Entities root in _entityAliases.Keys)
            {
                if (_entityAliases[root].Contains(entity))
                {
                    return root;
                }
            }

            return entity;
        }

        public IEnumerable<TR2Entities> GetAliases(TR2Entities entity)
        {
            return _entityAliases.ContainsKey(entity) ? _entityAliases[entity] : _emptyEntities;
        }

        public TR2Entities GetLevelAlias(string level, TR2Entities entity)
        {
            return TR2EntityUtilities.GetAliasForLevel(level, entity);
        }

        public bool IsAliasDuplicatePermitted(TR2Entities entity)
        {
            return _permittedAliasDuplicates.Contains(entity);
        }

        public bool IsOverridePermitted(TR2Entities entity)
        {
            return _permittedOverrides.Contains(entity);
        }

        public IEnumerable<TR2Entities> GetUnsafeModelReplacements()
        {
            return _unsafeModelReplacements;
        }

        public bool IsNonGraphicsDependency(TR2Entities entity)
        {
            return _nonGraphicsDependencies.Contains(entity);
        }

        public bool IsSoundOnlyDependency(TR2Entities entity)
        {
            return _soundOnlyDependencies.Contains(entity);
        }

        public short[] GetHardcodedSounds(TR2Entities entity)
        {
            return _hardcodedSoundIndices.ContainsKey(entity) ? _hardcodedSoundIndices[entity] : null;
        }

        public IEnumerable<int> GetIgnorableTextureIndices(TR2Entities entity, string level)
        {
            return _ignoreEntityTextures.ContainsKey(entity) ? _ignoreEntityTextures[entity] : null;
        }

        #region Data

        private static readonly IEnumerable<TR2Entities> _emptyEntities = new List<TR2Entities>();

        private static readonly Dictionary<TR2Entities, TR2Entities[]> _entityDependencies = new Dictionary<TR2Entities, TR2Entities[]>
        {
            [TR2Entities.LaraSun] =
                new TR2Entities[] { TR2Entities.LaraPistolAnim_H_Sun, TR2Entities.LaraAutoAnim_H_Sun, TR2Entities.LaraUziAnim_H_Sun },
            [TR2Entities.LaraUnwater] =
                new TR2Entities[] { TR2Entities.LaraPistolAnim_H_Unwater, TR2Entities.LaraAutoAnim_H_Unwater, TR2Entities.LaraUziAnim_H_Unwater },
            [TR2Entities.LaraSnow] =
                new TR2Entities[] { TR2Entities.LaraPistolAnim_H_Snow, TR2Entities.LaraAutoAnim_H_Snow, TR2Entities.LaraUziAnim_H_Snow },
            [TR2Entities.LaraHome] =
                new TR2Entities[] { TR2Entities.LaraPistolAnim_H_Home, TR2Entities.LaraAutoAnim_H_Home, TR2Entities.LaraUziAnim_H_Home },

            [TR2Entities.Pistols_M_H] =
                new TR2Entities[] { TR2Entities.LaraPistolAnim_H, TR2Entities.Gunflare_H },
            [TR2Entities.Shotgun_M_H] =
                new TR2Entities[] { TR2Entities.LaraShotgunAnim_H, TR2Entities.Gunflare_H },
            [TR2Entities.Autos_M_H] =
                new TR2Entities[] { TR2Entities.LaraAutoAnim_H, TR2Entities.Gunflare_H },
            [TR2Entities.Uzi_M_H] =
                new TR2Entities[] { TR2Entities.LaraUziAnim_H, TR2Entities.Gunflare_H },
            [TR2Entities.M16_M_H] =
                new TR2Entities[] { TR2Entities.LaraM16Anim_H, TR2Entities.M16Gunflare_H },
            [TR2Entities.Harpoon_M_H] =
                new TR2Entities[] { TR2Entities.LaraHarpoonAnim_H, TR2Entities.HarpoonProjectile_H },
            [TR2Entities.GrenadeLauncher_M_H] =
                new TR2Entities[] { TR2Entities.LaraGrenadeAnim_H, TR2Entities.GrenadeProjectile_H },

            [TR2Entities.TRex] =
                new TR2Entities[] { TR2Entities.LaraMiscAnim_H_Wall },
            [TR2Entities.MaskedGoon2] =
                new TR2Entities[] { TR2Entities.MaskedGoon1 },
            [TR2Entities.MaskedGoon3] =
                new TR2Entities[] { TR2Entities.MaskedGoon1 },
            [TR2Entities.ScubaDiver] =
                new TR2Entities[] { TR2Entities.ScubaHarpoonProjectile_H },
            [TR2Entities.Shark] =
                new TR2Entities[] { TR2Entities.LaraMiscAnim_H_Unwater },
            [TR2Entities.StickWieldingGoon2] =
                new TR2Entities[] { TR2Entities.StickWieldingGoon1GreenVest },
            [TR2Entities.MercSnowmobDriver] =
                new TR2Entities[] { TR2Entities.BlackSnowmob },
            [TR2Entities.BlackSnowmob] =
                new TR2Entities[] { TR2Entities.RedSnowmobile },
            [TR2Entities.RedSnowmobile] =
                new TR2Entities[] { TR2Entities.SnowmobileBelt, TR2Entities.LaraSnowmobAnim_H },
            [TR2Entities.Boat] =
                new TR2Entities[] { TR2Entities.LaraBoatAnim_H },
            [TR2Entities.Mercenary3] =
                new TR2Entities[] { TR2Entities.Mercenary2 },
            [TR2Entities.Yeti] =
                new TR2Entities[] { TR2Entities.LaraMiscAnim_H_Ice },
            [TR2Entities.XianGuardSpear] =
                new TR2Entities[] { TR2Entities.LaraMiscAnim_H_Xian, TR2Entities.XianGuardSpearStatue },
            [TR2Entities.XianGuardSword] =
                new TR2Entities[] { TR2Entities.XianGuardSwordStatue },
            [TR2Entities.Knifethrower] =
                new TR2Entities[] { TR2Entities.KnifeProjectile_H },
            [TR2Entities.MarcoBartoli] =
                new TR2Entities[]
                {
                    TR2Entities.DragonExplosionEmitter_N, TR2Entities.DragonExplosion1_H, TR2Entities.DragonExplosion2_H, TR2Entities.DragonExplosion3_H,
                    TR2Entities.DragonFront_H, TR2Entities.DragonBack_H, TR2Entities.DragonBonesFront_H, TR2Entities.DragonBonesBack_H, TR2Entities.LaraMiscAnim_H_Xian,
                    TR2Entities.Puzzle2_M_H_Dagger
                }
        };

        private static readonly Dictionary<TR2Entities, List<TR2Entities>> _spriteDependencies = new Dictionary<TR2Entities, List<TR2Entities>>
        {
            [TR2Entities.FlamethrowerGoon] 
                = new List<TR2Entities> { TR2Entities.Flame_S_H },
            [TR2Entities.MarcoBartoli] 
                = new List<TR2Entities> { TR2Entities.Flame_S_H },
            [TR2Entities.Boat] 
                = new List<TR2Entities> { TR2Entities.BoatWake_S_H },
            [TR2Entities.RedSnowmobile] 
                = new List<TR2Entities> { TR2Entities.SnowmobileWake_S_H },
            [TR2Entities.XianGuardSword] 
                = new List<TR2Entities> { TR2Entities.XianGuardSparkles_S_H },
            [TR2Entities.WaterfallMist_N]
                = new List<TR2Entities> { TR2Entities.WaterRipples_S_H },
            [TR2Entities.Key2_M_H]
                = new List<TR2Entities> { TR2Entities.Key2_S_P }
        };

        private static readonly List<TR2Entities> _cinematicEntities = new List<TR2Entities>
        {
            TR2Entities.DragonExplosionEmitter_N
        };

        // These are models that use Lara's hips as placeholders
        private static readonly List<TR2Entities> _laraDependentModels = new List<TR2Entities>
        {
            TR2Entities.CameraTarget_N, TR2Entities.FlameEmitter_N, TR2Entities.LaraCutscenePlacement_N,
            TR2Entities.DragonExplosionEmitter_N, TR2Entities.BartoliHideoutClock_N, TR2Entities.SingingBirds_N,
            TR2Entities.WaterfallMist_N, TR2Entities.DrippingWater_N, TR2Entities.LavaAirParticleEmitter_N,
            TR2Entities.AlarmBell_N, TR2Entities.DoorBell_N
        };

        private static readonly Dictionary<TR2Entities, List<TR2Entities>> _entityAliases = new Dictionary<TR2Entities, List<TR2Entities>>
        {
            [TR2Entities.Lara] = new List<TR2Entities>
            {
                TR2Entities.LaraSun, TR2Entities.LaraUnwater, TR2Entities.LaraSnow, TR2Entities.LaraHome
            },

            [TR2Entities.LaraPistolAnim_H] = new List<TR2Entities>
            {
                TR2Entities.LaraPistolAnim_H_Sun, TR2Entities.LaraPistolAnim_H_Unwater, TR2Entities.LaraPistolAnim_H_Snow, TR2Entities.LaraPistolAnim_H_Home
            },
            [TR2Entities.LaraAutoAnim_H] = new List<TR2Entities>
            {
                TR2Entities.LaraAutoAnim_H_Sun, TR2Entities.LaraAutoAnim_H_Unwater, TR2Entities.LaraAutoAnim_H_Snow, TR2Entities.LaraAutoAnim_H_Home
            },
            [TR2Entities.LaraUziAnim_H] = new List<TR2Entities>
            {
                TR2Entities.LaraUziAnim_H_Sun, TR2Entities.LaraUziAnim_H_Unwater, TR2Entities.LaraUziAnim_H_Snow, TR2Entities.LaraUziAnim_H_Home
            },

            [TR2Entities.LaraMiscAnim_H] = new List<TR2Entities>
            {
                TR2Entities.LaraMiscAnim_H_Wall, TR2Entities.LaraMiscAnim_H_Unwater, TR2Entities.LaraMiscAnim_H_Ice, TR2Entities.LaraMiscAnim_H_Xian, TR2Entities.LaraMiscAnim_H_HSH, TR2Entities.LaraMiscAnim_H_Venice
            },

            [TR2Entities.TigerOrSnowLeopard] = new List<TR2Entities>
            {
                TR2Entities.BengalTiger, TR2Entities.SnowLeopard, TR2Entities.WhiteTiger
            },

            [TR2Entities.StickWieldingGoon1] = new List<TR2Entities>
            {
                TR2Entities.StickWieldingGoon1Bandana, TR2Entities.StickWieldingGoon1BlackJacket, TR2Entities.StickWieldingGoon1BodyWarmer, TR2Entities.StickWieldingGoon1GreenVest, TR2Entities.StickWieldingGoon1WhiteVest
            },

            [TR2Entities.Barracuda] = new List<TR2Entities>
            {
                TR2Entities.BarracudaIce, TR2Entities.BarracudaUnwater, TR2Entities.BarracudaXian
            },

            [TR2Entities.Puzzle1_M_H] = new List<TR2Entities>
            {
                TR2Entities.Puzzle1_M_H_CircuitBoard, TR2Entities.Puzzle1_M_H_CircuitBreaker, TR2Entities.Puzzle1_M_H_Dagger, TR2Entities.Puzzle1_M_H_DragonSeal, TR2Entities.Puzzle1_M_H_MysticPlaque,
                TR2Entities.Puzzle1_M_H_PrayerWheel, TR2Entities.Puzzle1_M_H_RelayBox, TR2Entities.Puzzle1_M_H_TibetanMask
            },
            [TR2Entities.Puzzle2_M_H] = new List<TR2Entities>
            {
                TR2Entities.Puzzle2_M_H_CircuitBoard, TR2Entities.Puzzle2_M_H_Dagger, TR2Entities.Puzzle2_M_H_GemStone, TR2Entities.Puzzle2_M_H_MysticPlaque
            },
            [TR2Entities.Puzzle4_M_H] = new List<TR2Entities>
            {
                TR2Entities.Puzzle4_M_H_Seraph
            }
        };

        private static readonly List<TR2Entities> _permittedAliasDuplicates = new List<TR2Entities>
        {
            TR2Entities.LaraMiscAnim_H
        };

        private static readonly List<TR2Entities> _permittedOverrides = new List<TR2Entities>
        {
            TR2Entities.MarcoBartoli
        };

        private static readonly List<TR2Entities> _unsafeModelReplacements = new List<TR2Entities>
        {
        };

        private static readonly List<TR2Entities> _nonGraphicsDependencies = new List<TR2Entities>
        {
            TR2Entities.StickWieldingGoon1GreenVest, TR2Entities.MaskedGoon1, TR2Entities.Mercenary2
        };

        // If these are imported into levels that already have another alias for them, only their hardcoded sounds will be imported
        protected static readonly List<TR2Entities> _soundOnlyDependencies = new List<TR2Entities>
        {
            TR2Entities.StickWieldingGoon1GreenVest
        };

        private static readonly Dictionary<TR2Entities, short[]> _hardcodedSoundIndices = new Dictionary<TR2Entities, short[]>
        {
            [TR2Entities.DragonExplosionEmitter_N] = new short[]
            {
                341  // Explosion when dragon spawns
            },
            [TR2Entities.DragonFront_H] = new short[]
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
            [TR2Entities.Boat] = new short[]
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
            [TR2Entities.LaraSnowmobAnim_H] = new short[]
            {
                153, // Snowmobile idling
                155  // Snowmobile accelerating
            },
            [TR2Entities.StickWieldingGoon1Bandana] = new short[]
            {
                71,  // Thump 1
                72,  // Thump 2
            },
            [TR2Entities.StickWieldingGoon1BlackJacket] = new short[]
            {
                71,  // Thump 1
                72,  // Thump 2
            },
            [TR2Entities.StickWieldingGoon1BodyWarmer] = new short[]
            {
                69,  // Footstep
                70,  // Grunt
                71,  // Thump 1
                72,  // Thump 2
                121  // Thump 3
            },
            [TR2Entities.StickWieldingGoon1GreenVest] = new short[]
            {
                71,  // Thump 1
                72,  // Thump 2
            },
            [TR2Entities.StickWieldingGoon1WhiteVest] = new short[]
            {
                71,  // Thump 1
                72,  // Thump 2,
                121  // Thump 3
            },
            [TR2Entities.StickWieldingGoon2] = new short[]
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
            [TR2Entities.XianGuardSword] = new short[]
            {
                312  // Hovering
            },
            [TR2Entities.Winston] = new short[]
            {
                344, // Scared
                345, // Huff
                346, // Bumped
                347  // Cups clattering
            }
        };

        private static readonly Dictionary<TR2Entities, List<int>> _ignoreEntityTextures = new Dictionary<TR2Entities, List<int>>
        {
            [TR2Entities.LaraMiscAnim_H] 
                = new List<int>(), // empty list indicates to ignore everything
            [TR2Entities.WaterfallMist_N]
                = new List<int> { 0, 1, 2, 3, 4, 5, 6, 11, 15, 20, 22 },
            [TR2Entities.LaraSnowmobAnim_H] 
                = new List<int> { 0, 1, 2, 4, 5, 6, 7, 8, 9, 10, 20, 21, 23 },
            [TR2Entities.SnowmobileBelt] 
                = new List<int> { 0, 1, 2, 4, 5, 6, 7, 8, 9, 10, 20, 21, 23 },
            [TR2Entities.DragonExplosionEmitter_N] 
                = new List<int> { 0, 1, 2, 3, 4, 5, 6, 8, 13, 14, 16, 17, 19 }
        };

        #endregion
    }
}