using System.Collections.Generic;
using TRLevelReader.Model;

namespace TRModelTransporter.Model
{
    public class PackedSound
    {
        public Dictionary<ushort, uint[]> SampleIndices { get; set; }
        public Dictionary<int, TRSoundDetails> SoundDetails { get; set; }
        public Dictionary<int, short> SoundMapIndices { get; set; }

        public PackedSound()
        {
            SampleIndices = new Dictionary<ushort, uint[]>();
            SoundDetails = new Dictionary<int, TRSoundDetails>();
            SoundMapIndices = new Dictionary<int, short>();
        }
    }
}