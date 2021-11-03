using System.Collections.Generic;
using TREnvironmentEditor.Model;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Secrets
{
    public class TRSecretRoom<E> where E : class
    {
        public List<Location> RewardPositions { get; set; }
        public List<E> Doors { get; set; }
        public EMEditorSet Room { get; set; }
    }
}