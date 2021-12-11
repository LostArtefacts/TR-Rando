using System.Collections.Generic;
using TRLevelReader.Model.Enums;

namespace TRModelTransporter.Data
{
    public class TR1DefaultDataProvider : ITransportDataProvider<TREntities>
    {
        public Dictionary<TREntities, TREntities> AliasPriority { get; set; }

        public IEnumerable<TREntities> GetModelDependencies(TREntities entity)
        {
            return _entityDependencies.ContainsKey(entity) ? _entityDependencies[entity] : _emptyEntities;
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
            return entity; // No aliases in TR1
        }

        public bool IsAliasDuplicatePermitted(TREntities entity)
        {
            return _permittedAliasDuplicates.Contains(entity);
        }

        public bool IsOverridePermitted(TREntities entity)
        {
            return false;
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

        public IEnumerable<int> GetIgnorableTextureIndices(TREntities entity)
        {
            return _ignoreEntityTextures.ContainsKey(entity) ? _ignoreEntityTextures[entity] : null;
        }

        #region Data

        private static readonly IEnumerable<TREntities> _emptyEntities = new List<TREntities>();

        private static readonly Dictionary<TREntities, TREntities[]> _entityDependencies = new Dictionary<TREntities, TREntities[]>
        {
        };

        private static readonly Dictionary<TREntities, List<TREntities>> _spriteDependencies = new Dictionary<TREntities, List<TREntities>>
        {
            [TREntities.ScionPiece_M_H]
                = new List<TREntities> { TREntities.ScionPiece_P }
        };

        private static readonly List<TREntities> _cinematicEntities = new List<TREntities>
        {
        };

        // These are models that use Lara's hips as placeholders
        private static readonly List<TREntities> _laraDependentModels = new List<TREntities>
        {
        };

        private static readonly Dictionary<TREntities, List<TREntities>> _entityAliases = new Dictionary<TREntities, List<TREntities>>
        {
        };

        private static readonly List<TREntities> _permittedAliasDuplicates = new List<TREntities>
        {
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
        };

        private static readonly Dictionary<TREntities, List<int>> _ignoreEntityTextures = new Dictionary<TREntities, List<int>>
        {
        };

        #endregion
    }
}