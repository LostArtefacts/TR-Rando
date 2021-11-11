using Newtonsoft.Json;
using System.Collections.Generic;
using TREnvironmentEditor.Model;
using TRLevelReader.Model;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Secrets
{
    public class TRSecretRoom<E> where E : class
    {
        public List<Location> RewardPositions { get; set; }
        public List<E> Doors { get; set; }
        public List<TRCamera> Cameras { get; set; }
        public EMEditorSet Room { get; set; }

        [JsonIgnore]
        public List<int> DoorIndices { get; set; }
        [JsonIgnore]
        public List<int> CameraIndices { get; set; }

        public bool HasCameras => CameraIndices != null && CameraIndices.Count > 0;
    }
}