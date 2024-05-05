namespace TRImageControl.Packing;

// This allows external callers to monitor specific textures for specific entities by providing a list
// of texture indices it wants to observe. When the import completes, a map of entity to a list of
// PositionedTextures will be returned to it for processing as necessary.
public interface ITexturePositionMonitor<E> where E : Enum
{
    Dictionary<E, List<int>> GetMonitoredTextureIndices();
    void MonitoredTexturesPositioned(Dictionary<E, List<PositionedTexture>> texturePositions);
    void EntityTexturesRemoved(List<E> entities);
}
