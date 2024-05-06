using TRLevelControl.Model;

namespace TRDataControl;

public class TR4DataProvider : IDataProvider<TR4Type, TR4SFX>
{
    public int TextureTileLimit { get; set; } = short.MaxValue;
    public int TextureObjectLimit { get; set; } = short.MaxValue;

    public Dictionary<TR4Type, TR4Type> AliasPriority { get; set; }

    public TRBlobType GetBlobType(TR4Type type)
    {
        if (_spriteTypes.Contains(type))
        {
            return TRBlobType.Sprite;
        }
        if (type >= TR4Type.SceneryBase)
        {
            return TRBlobType.StaticMesh;
        }
        return TRBlobType.Model;
    }

    public IEnumerable<TR4Type> GetDependencies(TR4Type type)
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

    public IEnumerable<TR4Type> GetRemovalExclusions(TR4Type type)
    {
        return _emptyTypes;
    }

    public IEnumerable<TR4Type> GetCyclicDependencies(TR4Type type)
    {
        return _emptyTypes;
    }

    public IEnumerable<TR4Type> GetCinematicTypes()
    {
        return _emptyTypes;
    }

    public bool IsAlias(TR4Type type)
    {
        foreach (List<TR4Type> aliases in _typeAliases.Values)
        {
            if (aliases.Contains(type))
            {
                return true;
            }
        }

        return false;
    }

    public bool HasAliases(TR4Type type)
    {
        return _typeAliases.ContainsKey(type);
    }

    public TR4Type TranslateAlias(TR4Type type)
    {
        foreach (TR4Type root in _typeAliases.Keys)
        {
            if (_typeAliases[root].Contains(type))
            {
                return root;
            }
        }

        return type;
    }

    public IEnumerable<TR4Type> GetAliases(TR4Type type)
    {
        return _typeAliases.ContainsKey(type) ? _typeAliases[type] : _emptyTypes;
    }

    public TR4Type GetLevelAlias(string level, TR4Type type)
    {
        return level switch { _ => type };
    }

    public bool IsAliasDuplicatePermitted(TR4Type type)
    {
        return _permittedAliasDuplicates.Contains(type);
    }

    public bool IsOverridePermitted(TR4Type type)
    {
        return _permittedOverrides.Contains(type);
    }

    public bool IsNonGraphicsDependency(TR4Type type)
    {
        return _nonGraphicsDependencies.Contains(type);
    }

    public IEnumerable<TR4SFX> GetHardcodedSounds(TR4Type type)
    {
        return _hardcodedSFX.ContainsKey(type)
            ? _hardcodedSFX[type]
            : _emptySFX;
    }

    #region Data

    private static readonly List<TR4Type> _emptyTypes = new();
    private static readonly List<TR4SFX> _emptySFX = new();

    private static readonly Dictionary<TR4Type, List<TR4Type>> _typeDependencies = new()
    {

    };

    private static readonly Dictionary<TR4Type, List<TR4Type>> _typeAliases = new()
    {

    };

    private static readonly List<TR4Type> _permittedAliasDuplicates = new()
    {

    };

    private static readonly List<TR4Type> _permittedOverrides = new()
    {

    };

    private static readonly List<TR4Type> _nonGraphicsDependencies = new()
    {

    };

    private static readonly Dictionary<TR4Type, List<TR4SFX>> _hardcodedSFX = new()
    {

    };

    private static readonly List<TR4Type> _spriteTypes = new()
    {

    };

    #endregion
}
