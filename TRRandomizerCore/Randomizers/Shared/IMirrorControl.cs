namespace TRRandomizerCore.Randomizers;

public interface IMirrorControl
{
    void AllocateMirroredLevels(int seed);
    bool IsMirrored(string levelName);
    void SetIsMirrored(string levelName, bool mirrored);
}
