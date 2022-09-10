using System.Collections.Generic;
using TRLevelReader.Helpers;
using TRLevelReader.Model.Enums;

namespace TRModelTransporter.Data
{
    public class TR1DefaultDataProvider : ITransportDataProvider<TREntities>
    {
        public int TextureTileLimit { get; set; } = 16;
        public int TextureObjectLimit { get; set; } = 2048;

        public Dictionary<TREntities, TREntities> AliasPriority { get; set; }

        public IEnumerable<TREntities> GetModelDependencies(TREntities entity)
        {
            return _entityDependencies.ContainsKey(entity) ? _entityDependencies[entity] : _emptyEntities;
        }

        public IEnumerable<TREntities> GetRemovalExclusions(TREntities entity)
        {
            return _removalExclusions.ContainsKey(entity) ? _removalExclusions[entity] : _emptyEntities;
        }

        public IEnumerable<TREntities> GetCyclicDependencies(TREntities entity)
        {
            return _cyclicDependencies.ContainsKey(entity) ? _cyclicDependencies[entity] : _emptyEntities;
        }

        public IEnumerable<TREntities> GetSpriteDependencies(TREntities entity)
        {
            return _spriteDependencies.ContainsKey(entity) ? _spriteDependencies[entity] : _emptyEntities;
        }

        public IEnumerable<TREntities> GetCinematicEntities()
        {
            return _cinematicEntities;
        }

        public IEnumerable<TREntities> GetLaraDependants()
        {
            return _laraDependentModels;
        }

        public bool IsAlias(TREntities entity)
        {
            foreach (List<TREntities> aliases in _entityAliases.Values)
            {
                if (aliases.Contains(entity))
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasAliases(TREntities entity)
        {
            return _entityAliases.ContainsKey(entity);
        }

        public TREntities TranslateAlias(TREntities entity)
        {
            foreach (TREntities root in _entityAliases.Keys)
            {
                if (_entityAliases[root].Contains(entity))
                {
                    return root;
                }
            }

            return entity;
        }

        public IEnumerable<TREntities> GetAliases(TREntities entity)
        {
            return _entityAliases.ContainsKey(entity) ? _entityAliases[entity] : _emptyEntities;
        }

        public TREntities GetLevelAlias(string level, TREntities entity)
        {
            return TR1EntityUtilities.GetAliasForLevel(level, entity);
        }

        public bool IsAliasDuplicatePermitted(TREntities entity)
        {
            return _permittedAliasDuplicates.Contains(entity);
        }

        public bool IsOverridePermitted(TREntities entity)
        {
            return _permittedOverrides.Contains(entity);
        }

        public IEnumerable<TREntities> GetUnsafeModelReplacements()
        {
            return _unsafeModelReplacements;
        }

        public bool IsNonGraphicsDependency(TREntities entity)
        {
            return _nonGraphicsDependencies.Contains(entity);
        }

        public bool IsSoundOnlyDependency(TREntities entity)
        {
            return _soundOnlyDependencies.Contains(entity);
        }

        public short[] GetHardcodedSounds(TREntities entity)
        {
            return _hardcodedSoundIndices.ContainsKey(entity) ? _hardcodedSoundIndices[entity] : null;
        }

        public IEnumerable<int> GetIgnorableTextureIndices(TREntities entity, string level)
        {
            if (entity == TREntities.LaraMiscAnim_H && level == TRLevelNames.VALLEY)
            {
                // Mesh swap when Lara is killed by T-Rex
                return null;
            }
            return _ignoreEntityTextures.ContainsKey(entity) ? _ignoreEntityTextures[entity] : null;
        }

        #region Data

        private static readonly IEnumerable<TREntities> _emptyEntities = new List<TREntities>();

        private static readonly Dictionary<TREntities, TREntities[]> _entityDependencies = new Dictionary<TREntities, TREntities[]>
        {
            [TREntities.Adam]
                = new TREntities[] { TREntities.LaraMiscAnim_H_Pyramid },
            [TREntities.BandagedAtlantean]
                = new TREntities[] { TREntities.BandagedFlyer },
            [TREntities.BandagedFlyer]
                = new TREntities[] { TREntities.Missile2_H, TREntities.Missile3_H },
            [TREntities.Centaur]
                = new TREntities[] { TREntities.Missile3_H },
            [TREntities.CentaurStatue]
                = new TREntities[] { TREntities.Centaur },
            [TREntities.CrocodileLand]
                = new TREntities[] { TREntities.CrocodileWater },
            [TREntities.CrocodileWater]
                = new TREntities[] { TREntities.CrocodileLand },
            [TREntities.CentaurStatue]
                = new TREntities[] { TREntities.Centaur },
            [TREntities.MeatyAtlantean]
                = new TREntities[] { TREntities.MeatyFlyer },
            [TREntities.MeatyFlyer]
                = new TREntities[] { TREntities.Missile2_H, TREntities.Missile3_H },
            [TREntities.Natla]
                = new TREntities[] { TREntities.Missile2_H, TREntities.Missile3_H },
            [TREntities.Pierre]
                = new TREntities[] { TREntities.Key1_M_H, TREntities.ScionPiece_M_H },
            [TREntities.RatLand]
                = new TREntities[] { TREntities.RatWater },
            [TREntities.RatWater]
                = new TREntities[] { TREntities.RatLand },
            [TREntities.ShootingAtlantean_N]
                = new TREntities[] { TREntities.MeatyFlyer },
            [TREntities.SkateboardKid]
                = new TREntities[] { TREntities.Skateboard },
            [TREntities.TRex]
                = new TREntities[] { TREntities.LaraMiscAnim_H_Valley }
        };

        private static readonly Dictionary<TREntities, TREntities[]> _cyclicDependencies = new Dictionary<TREntities, TREntities[]>
        {
            [TREntities.CrocodileLand]
                = new TREntities[] { TREntities.CrocodileWater },
            [TREntities.CrocodileWater]
                = new TREntities[] { TREntities.CrocodileLand },
            [TREntities.RatLand]
                = new TREntities[] { TREntities.RatWater },
            [TREntities.RatWater]
                = new TREntities[] { TREntities.RatLand },
            [TREntities.Pierre]
                = new TREntities[] { TREntities.ScionPiece_M_H },
        };

        private static readonly Dictionary<TREntities, List<TREntities>> _removalExclusions = new Dictionary<TREntities, List<TREntities>>
        {
            [TREntities.FlyingAtlantean]
                = new List<TREntities> { TREntities.NonShootingAtlantean_N, TREntities.ShootingAtlantean_N }
        };

        private static readonly Dictionary<TREntities, List<TREntities>> _spriteDependencies = new Dictionary<TREntities, List<TREntities>>
        {
            [TREntities.SecretScion_M_H]
                = new List<TREntities> { TREntities.ScionPiece4_S_P },
            [TREntities.SecretGoldIdol_M_H]
                = new List<TREntities> { TREntities.ScionPiece4_S_P },
            [TREntities.SecretLeadBar_M_H]
                = new List<TREntities> { TREntities.ScionPiece4_S_P },
            [TREntities.SecretGoldBar_M_H]
                = new List<TREntities> { TREntities.ScionPiece4_S_P },
            [TREntities.SecretAnkh_M_H]
                = new List<TREntities> { TREntities.ScionPiece4_S_P },

            [TREntities.Adam]
                = new List<TREntities> { TREntities.Explosion1_S_H },
            [TREntities.Centaur]
                = new List<TREntities> { TREntities.Explosion1_S_H },
            [TREntities.FlyingAtlantean]
                = new List<TREntities> { TREntities.Explosion1_S_H },
            [TREntities.Key1_M_H]
                = new List<TREntities> { TREntities.Key1_S_P },
            [TREntities.Natla]
                = new List<TREntities> { TREntities.Explosion1_S_H },
            [TREntities.ScionPiece_M_H]
                = new List<TREntities> { TREntities.ScionPiece2_S_P },
            [TREntities.ShootingAtlantean_N]
                = new List<TREntities> { TREntities.Explosion1_S_H },
            [TREntities.Missile3_H]
                = new List<TREntities> { TREntities.Explosion1_S_H },
        };

        private static readonly List<TREntities> _cinematicEntities = new List<TREntities>
        {
        };

        // These are models that use Lara's hips as placeholders
        private static readonly List<TREntities> _laraDependentModels = new List<TREntities>
        {
            TREntities.NonShootingAtlantean_N, TREntities.ShootingAtlantean_N
        };

        private static readonly Dictionary<TREntities, List<TREntities>> _entityAliases = new Dictionary<TREntities, List<TREntities>>
        {
            [TREntities.FlyingAtlantean] = new List<TREntities>
            {
                TREntities.BandagedFlyer, TREntities.MeatyFlyer
            },
            [TREntities.LaraMiscAnim_H] = new List<TREntities>
            {
                TREntities.LaraMiscAnim_H_General, TREntities.LaraMiscAnim_H_Valley, TREntities.LaraMiscAnim_H_Qualopec, TREntities.LaraMiscAnim_H_Midas,
                TREntities.LaraMiscAnim_H_Sanctuary, TREntities.LaraMiscAnim_H_Atlantis, TREntities.LaraMiscAnim_H_Pyramid
            },
            [TREntities.NonShootingAtlantean_N] = new List<TREntities>
            {
                TREntities.BandagedAtlantean, TREntities.MeatyAtlantean
            }
        };

        private static readonly List<TREntities> _permittedAliasDuplicates = new List<TREntities>
        {
            TREntities.LaraMiscAnim_H
        };

        private static readonly List<TREntities> _permittedOverrides = new List<TREntities>
        {
            TREntities.LaraPonytail_H_U, TREntities.ScionPiece_M_H
        };

        private static readonly List<TREntities> _unsafeModelReplacements = new List<TREntities>
        {
        };

        private static readonly List<TREntities> _nonGraphicsDependencies = new List<TREntities>
        {
        };

        // If these are imported into levels that already have another alias for them, only their hardcoded sounds will be imported
        protected static readonly List<TREntities> _soundOnlyDependencies = new List<TREntities>
        {
        };

        private static readonly Dictionary<TREntities, short[]> _hardcodedSoundIndices = new Dictionary<TREntities, short[]>
        {
            [TREntities.Adam] = new short[] { 104, 137, 138, 140, 141, 142 },
            [TREntities.BandagedFlyer] = new short[] { 104 },
            [TREntities.Bear] = new short[] { 12, 16 },
            [TREntities.Centaur] = new short[] { 104 },
            [TREntities.Gorilla] = new short[] { 90, 91, 101 },
            [TREntities.Larson] = new short[] { 78 },
            [TREntities.Lion] = new short[] { 85, 86, 87 },
            [TREntities.Lioness] = new short[] { 85, 86, 87 },
            [TREntities.MeatyAtlantean] = new short[] { 104 },
            [TREntities.MeatyFlyer] = new short[] { 104, 120, 121, 122, 123, 124, 125, 126 },
            [TREntities.Missile3_H] = new short[] { 104 },
            [TREntities.Natla] = new short[] { 104, 123, 124 },
            [TREntities.ShootingAtlantean_N] = new short[] { 104 },
            [TREntities.SkateboardKid] = new short[] { 132 },
            [TREntities.Wolf] = new short[] { 20 }
        };

        private static readonly Dictionary<TREntities, List<int>> _ignoreEntityTextures = new Dictionary<TREntities, List<int>>
        {
            [TREntities.LaraMiscAnim_H]
                = new List<int>(), // empty list indicates to ignore everything
            [TREntities.NonShootingAtlantean_N]
                = new List<int>(),
            [TREntities.ShootingAtlantean_N]
                = new List<int>(),
            [TREntities.Mummy]
                = new List<int> { 130, 131, 132, 133, 134, 135, 136, 137, 140, 141 }
        };

        #endregion
    }
}