using TRLevelControl;
using TRLevelControl.Model;

namespace TRDataControl;

public abstract class BaseTRRDataCache<TKey, TAlias>
    where TKey : Enum
    where TAlias : Enum
{
    private const string _pdpExt = ".PDP";

    private readonly Dictionary<TKey, TRModel> _pdpCache = new();

    public string PDPFolder { get; set; }

    public void SetData(TRDictionary<TKey, TRModel> pdpData, Dictionary<TKey, TAlias> mapData, TKey sourceType, TKey destinationType = default)
    {
        if (EqualityComparer<TKey>.Default.Equals(destinationType, default))
        {
            destinationType = sourceType;
        }

        SetPDPData(pdpData, sourceType, destinationType);
        SetMapData(mapData, sourceType, destinationType);
    }

    public void SetPDPData(TRDictionary<TKey, TRModel> pdpData, TKey sourceType, TKey destinationType)
    {
        TKey translatedKey = TranslateKey(sourceType);
        if (!_pdpCache.ContainsKey(sourceType))
        {
            string sourceLevel = GetSourceLevel(sourceType)
                ?? throw new KeyNotFoundException($"Source PDP file for {sourceType} is undefined");

            TRPDPControlBase<TKey> control = GetPDPControl();
            TRDictionary<TKey, TRModel> models = control.Read(Path.Combine(PDPFolder, Path.GetFileNameWithoutExtension(sourceLevel) + _pdpExt));
            if (models.ContainsKey(translatedKey))
            {
                _pdpCache[sourceType] = models[translatedKey];
            }
            else if (models.ContainsKey(destinationType))
            {
                _pdpCache[sourceType] = models[destinationType];
            }
            else
            {
                throw new KeyNotFoundException($"Could not load cached PDP data for {sourceType}");
            }
        }

        translatedKey = TranslateKey(destinationType);
        pdpData[translatedKey] = _pdpCache[sourceType];
    }

    public void SetMapData(Dictionary<TKey, TAlias> mapData, TKey sourceType, TKey destinationType)
    {
        TAlias alias = GetAlias(sourceType);
        if (EqualityComparer<TAlias>.Default.Equals(alias, default))
        {
            return;
        }

        TKey translatedKey = TranslateKey(destinationType);
        mapData[translatedKey] = alias;
    }

    protected abstract TRPDPControlBase<TKey> GetPDPControl();
    public abstract string GetSourceLevel(TKey key);
    public abstract TKey TranslateKey(TKey key);
    public abstract TAlias GetAlias(TKey key);
}
