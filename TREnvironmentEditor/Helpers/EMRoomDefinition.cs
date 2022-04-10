using System.Collections.Generic;
using TRFDControl;

namespace TREnvironmentEditor.Helpers
{
    public class EMRoomDefinition<R> where R : class
    {
        public R Room { get; set; }
        public Dictionary<int, List<FDEntry>> FloorData { get; set; }
    }
}