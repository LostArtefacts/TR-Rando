using System.Collections.Generic;
using TRLevelControl.Model;

namespace TRModelTransporter.Model.Sound
{
    public class TR2PackedSound
    {
        public Dictionary<ushort, uint[]> SampleIndices { get; set; }
        public Dictionary<int, TRSoundDetails> SoundDetails { get; set; }
        public Dictionary<int, short> SoundMapIndices { get; set; }

        public TR2PackedSound()
        {
            SampleIndices = new Dictionary<ushort, uint[]>();
            SoundDetails = new Dictionary<int, TRSoundDetails>();
            SoundMapIndices = new Dictionary<int, short>();
        }
    }
}