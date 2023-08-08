using System.Collections.Generic;
using TRLevelControl.Model;

namespace TRModelTransporter.Model.Sound;

public class TR3PackedSound
{
    public Dictionary<ushort, uint[]> SampleIndices { get; set; }
    public Dictionary<int, TR3SoundDetails> SoundDetails { get; set; }
    public Dictionary<int, short> SoundMapIndices { get; set; }

    public TR3PackedSound()
    {
        SampleIndices = new Dictionary<ushort, uint[]>();
        SoundDetails = new Dictionary<int, TR3SoundDetails>();
        SoundMapIndices = new Dictionary<int, short>();
    }
}
