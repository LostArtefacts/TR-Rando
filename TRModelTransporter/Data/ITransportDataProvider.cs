using System;
using System.Collections.Generic;

namespace TRModelTransporter.Data
{
    public interface ITransportDataProvider<E> where E : Enum
    {
        int TextureTileLimit { get; set; }
        int TextureObjectLimit { get; set; }

        /// <summary>
        /// Return all other model types on which the given type depends.
        /// </summary>
        IEnumerable<E> GetModelDependencies(E entity);

        /// <summary>
        /// Null meshes that determine if the main entity can be removed from a level.
        /// </summary>
        IEnumerable<E> GetRemovalExclusions(E entity);

        /// <summary>
        /// Return model types that have a cyclic depencency on the given type.
        /// </summary>
        IEnumerable<E> GetCyclicDependencies(E entity);

        /// <summary>
        /// Return any sprite types on which the given type depends.
        /// </summary>
        IEnumerable<E> GetSpriteDependencies(E entity);
        
        /// <summary>
        /// Determines which alias has the priority in a family during import if another alias already exists.
        /// </summary>
        Dictionary<E, E> AliasPriority { get; set; }

        /// <summary>
        /// Return model types for which cinematic frames should be exported.
        /// </summary>
        IEnumerable<E> GetCinematicEntities();

        /// <summary>
        /// Return models that are dependent on Lara.
        /// </summary>
        IEnumerable<E> GetLaraDependants();

        /// <summary>
        /// Whether or not the given entity is an alias of another.
        /// </summary>
        bool IsAlias(E entity);

        /// <summary>
        /// Whether or not the given entity has aliases.
        /// </summary>
        bool HasAliases(E entity);

        /// <summary>
        /// Convert the given alias into its normal type.
        /// </summary>
        E TranslateAlias(E entity);

        /// <summary>
        /// Returns all possible aliases for the given entity.
        /// </summary>
        IEnumerable<E> GetAliases(E entity);

        /// <summary>
        /// Return the specific alias for an alias type given a particular level ID.
        /// </summary>
        E GetLevelAlias(string level, E entity);

        /// <summary>
        /// Duplicates on import will throw a TransportException unless they are permitted to replace existing models.
        /// </summary>
        bool IsAliasDuplicatePermitted(E entity);

        /// <summary>
        /// Similar to alias duplicates, but where we want to replace a non-aliased model with another.
        /// </summary>
        bool IsOverridePermitted(E entity);

        /// <summary>
        /// Models that cannot be replaced in the Model array and instead should be remapped to imported models.
        /// </summary>
        IEnumerable<E> GetUnsafeModelReplacements();

        /// <summary>
        /// Determine if the given type's graphics should be ignored on import.
        /// </summary>
        bool IsNonGraphicsDependency(E entity);

        /// <summary>
        /// Determine if the the given type's sound alone should be imported.
        /// </summary>
        bool IsSoundOnlyDependency(E entity);

        /// <summary>
        /// Returns any hardcoded internal sound IDs for the given entity.
        /// </summary>
        short[] GetHardcodedSounds(E entity);

        /// <summary>
        /// Returns a list of texture indices that should be ignored for a given entity.
        /// An emtpy list is translated as meaning all indices should be ignored. Null
        /// indicates that no indices should be ignored.
        /// </summary>
        IEnumerable<int> GetIgnorableTextureIndices(E entity, string level);
    }
}