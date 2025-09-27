using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMCloneTypeFunction : BaseEMFunction
{
    public Dictionary<uint, uint> ModelMap { get; set; }
    public Dictionary<uint, uint> SpriteMap { get; set; }

    public override void ApplyToLevel(TR1Level level)
        => Clone(level.Models, level.Sprites);

    public override void ApplyToLevel(TR2Level level)
        => Clone(level.Models, level.Sprites);

    public override void ApplyToLevel(TR3Level level)
        => Clone(level.Models, level.Sprites);

    private void Clone<T>(TRDictionary<T, TRModel> models, TRDictionary<T, TRSpriteSequence> sprites)
        where T : Enum
    {
        Clone(models, ModelMap);
        Clone(sprites, SpriteMap);
    }

    private static void Clone<T, V>(TRDictionary<T, V> levelMap, Dictionary<uint, uint> cloneMap)
        where T : Enum
        where V : class, ICloneable
    {
        if (cloneMap == null)
        {
            return;
        }

        foreach (var (baseId, targetId) in cloneMap)
        {
            var baseType = (T)(object)baseId;
            var targetType = (T)(object)targetId;
            if (levelMap.TryGetValue(baseType, out var value))
            {
                levelMap[targetType] = (V)value.Clone();
            }
        }
    }
}
