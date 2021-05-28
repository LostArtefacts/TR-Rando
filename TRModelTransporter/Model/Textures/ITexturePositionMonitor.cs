using System.Collections.Generic;
using TRLevelReader.Model.Enums;

namespace TRModelTransporter.Model.Textures
{
    // This allows external callers to monitor specific textures for specific entities by providing a list
    // of texture indices it wants to observe. When the import completes, a map of entity to a list of
    // PositionedTextures will be returned to it for processing as necessary.
    public interface ITexturePositionMonitor
    {
        Dictionary<TR2Entities, List<int>> GetMonitoredTextureIndices();
        void MonitoredTexturesPositioned(Dictionary<TR2Entities, List<PositionedTexture>> texturePositions);
        void EntityTexturesRemoved(List<TR2Entities> entities);
    }
}