using TRLevelControl.Model;

namespace TREnvironmentEditor.Model;

public class EMConditionalGroupedSet : ITextureModifier
{
    public BaseEMCondition Condition { get; set; }
    public EMEditorGroupedSet OnTrue { get; set; }
    public EMEditorGroupedSet OnFalse { get; set; }

    public EMEditorGroupedSet GetApplicableSet(TR1Level level)
    {
        return Condition.GetResult(level) ? OnTrue : OnFalse;
    }

    public EMEditorGroupedSet GetApplicableSet(TR2Level level)
    {
        return Condition.GetResult(level) ? OnTrue : OnFalse;
    }

    public EMEditorGroupedSet GetApplicableSet(TR3Level level)
    {
        return Condition.GetResult(level) ? OnTrue : OnFalse;
    }

    public void RemapTextures(Dictionary<ushort, ushort> indexMap)
    {
        OnTrue?.RemapTextures(indexMap);
        OnFalse?.RemapTextures(indexMap);
    }

    public void SetCommunityPatch(bool isCommunityPatch)
    {
        OnTrue?.SetCommunityPatch(isCommunityPatch);
        OnFalse?.SetCommunityPatch(isCommunityPatch);
    }
}
