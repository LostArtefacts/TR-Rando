using TRLevelControl.Model;

namespace TRDataControl;

public class TR5DataProvider : IDataProvider<TR5Type, TR5SFX>
{
    public int TextureTileLimit { get; set; } = short.MaxValue;
    public int TextureObjectLimit { get; set; } = short.MaxValue;

    public Dictionary<TR5Type, TR5Type> AliasPriority { get; set; }

    public TRBlobType GetBlobType(TR5Type type)
    {
        if (_spriteTypes.Contains(type))
        {
            return TRBlobType.Sprite;
        }
        if (type >= TR5Type.SceneryBase)
        {
            return TRBlobType.StaticMesh;
        }
        return TRBlobType.Model;
    }

    public IEnumerable<TR5Type> GetDependencies(TR5Type type)
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

    public IEnumerable<TR5Type> GetRemovalExclusions(TR5Type type)
    {
        return _emptyTypes;
    }

    public IEnumerable<TR5Type> GetCyclicDependencies(TR5Type type)
    {
        return _emptyTypes;
    }

    public IEnumerable<TR5Type> GetCinematicTypes()
    {
        return _emptyTypes;
    }

    public bool IsAlias(TR5Type type)
    {
        foreach (List<TR5Type> aliases in _typeAliases.Values)
        {
            if (aliases.Contains(type))
            {
                return true;
            }
        }

        return false;
    }

    public bool HasAliases(TR5Type type)
    {
        return _typeAliases.ContainsKey(type);
    }

    public TR5Type TranslateAlias(TR5Type type)
    {
        foreach (TR5Type root in _typeAliases.Keys)
        {
            if (_typeAliases[root].Contains(type))
            {
                return root;
            }
        }

        return type;
    }

    public IEnumerable<TR5Type> GetAliases(TR5Type type)
    {
        return _typeAliases.ContainsKey(type) ? _typeAliases[type] : _emptyTypes;
    }

    public TR5Type GetLevelAlias(string level, TR5Type type)
    {
        return level switch { _ => type };
    }

    public bool IsAliasDuplicatePermitted(TR5Type type)
    {
        return _permittedAliasDuplicates.Contains(type);
    }

    public bool IsOverridePermitted(TR5Type type)
    {
        return _permittedOverrides.Contains(type);
    }

    public bool IsNonGraphicsDependency(TR5Type type)
    {
        return _nonGraphicsDependencies.Contains(type);
    }

    public IEnumerable<TR5SFX> GetHardcodedSounds(TR5Type type)
    {
        return _hardcodedSFX.ContainsKey(type)
            ? _hardcodedSFX[type]
            : _emptySFX;
    }

    #region Data

    private static readonly List<TR5Type> _emptyTypes = new();
    private static readonly List<TR5SFX> _emptySFX = new();

    private static readonly Dictionary<TR5Type, List<TR5Type>> _typeDependencies = new()
    {

    };

    private static readonly Dictionary<TR5Type, List<TR5Type>> _typeAliases = new()
    {

    };

    private static readonly List<TR5Type> _permittedAliasDuplicates = new()
    {

    };

    private static readonly List<TR5Type> _permittedOverrides = new()
    {

    };

    private static readonly List<TR5Type> _nonGraphicsDependencies = new()
    {

    };

    private static readonly Dictionary<TR5Type, List<TR5SFX>> _hardcodedSFX = new()
    {

    };

    private static readonly List<TR5Type> _spriteTypes = new()
    {

    };

    #endregion
}
