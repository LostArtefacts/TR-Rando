namespace TRDataControl;

public interface IDataProvider<T, S>
    where T : Enum
    where S : Enum
{
    int TextureTileLimit { get; set; }
    int TextureObjectLimit { get; set; }

    /// <summary>
    /// Get the blob type of a game type.
    /// </summary>
    TRBlobType GetBlobType(T type);

    /// <summary>
    /// Return all other model types on which the given type depends.
    /// </summary>
    IEnumerable<T> GetDependencies(T type);

    /// <summary>
    /// Null meshes that determine if the main type can be removed from a level.
    /// </summary>
    IEnumerable<T> GetRemovalExclusions(T type);

    /// <summary>
    /// Return model types that have a cyclic depencency on the given type.
    /// </summary>
    IEnumerable<T> GetCyclicDependencies(T type);

    /// <summary>
    /// Determines which alias has the priority in a family during import if another alias already exists.
    /// </summary>
    Dictionary<T, T> AliasPriority { get; set; }

    /// <summary>
    /// Return model types for which cinematic frames should be exported.
    /// </summary>
    IEnumerable<T> GetCinematicTypes();

    /// <summary>
    /// Whether or not the given type is an alias of another.
    /// </summary>
    bool IsAlias(T type);

    /// <summary>
    /// Whether or not the given type has aliases.
    /// </summary>
    bool HasAliases(T type);

    /// <summary>
    /// Convert the given alias into its normal type.
    /// </summary>
    T TranslateAlias(T type);

    /// <summary>
    /// Returns all possible aliases for the given type.
    /// </summary>
    IEnumerable<T> GetAliases(T type);

    /// <summary>
    /// Return the specific alias for an alias type given a particular level ID.
    /// </summary>
    T GetLevelAlias(string level, T type);

    /// <summary>
    /// Duplicates on import will throw a TransportException unless they are permitted to replace existing models.
    /// </summary>
    bool IsAliasDuplicatePermitted(T type);

    /// <summary>
    /// Similar to alias duplicates, but where we want to replace a non-aliased model with another.
    /// </summary>
    bool IsOverridePermitted(T type);

    /// <summary>
    /// Determine if the given type's graphics should be ignored on import.
    /// </summary>
    bool IsNonGraphicsDependency(T type);

    /// <summary>
    /// Returns any hardcoded internal sound IDs for the given type.
    /// </summary>
    IEnumerable<S> GetHardcodedSounds(T type);
}
