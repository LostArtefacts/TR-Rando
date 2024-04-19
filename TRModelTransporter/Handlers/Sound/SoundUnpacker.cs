using TRLevelControl.Model;
using TRModelTransporter.Model.Sound;

namespace TRModelTransporter.Handlers;

public class SoundUnpacker
{
    private static readonly int _maxTR3Sounds = 450; // TR3+

    private List<uint> _sampleIndices;
    private List<short> _soundMap;

    private Dictionary<uint, ushort> _sampleMap;
    private Dictionary<int, int> _soundDetailsMap;
    private Dictionary<int, int> _soundIndexMap;

    public IReadOnlyDictionary<int, int> SoundIndexMap => _soundIndexMap;

    public void Unpack(TR3PackedSound sound, TR3Level level, bool retainInternalIndices = false)
    {
        GenerateSampleMap(level.SampleIndices, sound.SampleIndices);
        GenerateSoundDetailsMap(level.SoundDetails, sound.SoundDetails);
        GenerateSoundIndexMap(level, retainInternalIndices, sound.SoundMapIndices);

        level.SampleIndices.Clear();
        level.SampleIndices.AddRange(_sampleIndices);
    }

    private void GenerateSampleMap(List<uint> sampleIndices, Dictionary<ushort, uint[]> packedIndices, Dictionary<uint, uint> sampleKeyMap = null)
    {
        // Insert each required SampleIndex value, which are the values that point into
        // MAIN.SFX (or Samples[] in TR1). Note the first inserted index for updating
        // the relevant SoundDetails.Sample.
        _sampleIndices = sampleIndices.ToList();
        _sampleMap = new Dictionary<uint, ushort>();

        foreach (ushort sampleIndex in packedIndices.Keys)
        {
            uint[] sampleValues = packedIndices[sampleIndex];
            if (sampleValues.Length > 0)
            {
                // Store each value as long as it does not already exist. Keep a reference to
                // the first index so we can update the SoundDetails pointer.
                uint firstSample = 0;
                for (int i = 0; i < sampleValues.Length; i++)
                {
                    uint sampleKey = sampleValues[i];
                    // We may have remapped the sample key (for TR1) so get the new sample pointer
                    if (sampleKeyMap != null && sampleKeyMap.ContainsKey(sampleValues[i]))
                    {
                        sampleKey = sampleKeyMap[sampleValues[i]];
                    }

                    if (!_sampleIndices.Contains(sampleKey))
                    {
                        _sampleIndices.Add(sampleKey);
                    }

                    if (i == 0)
                    {
                        firstSample = sampleKey;
                    }
                }

                // Store the new index of the first sample
                _sampleMap[sampleIndex] = (ushort)_sampleIndices.IndexOf(firstSample);
            }
        }
    }

    // TR3+
    private void GenerateSoundDetailsMap(List<TR3SoundDetails> soundDetails, Dictionary<int, TR3SoundDetails> packedSoundDetails)
    {
        _soundDetailsMap = new Dictionary<int, int>();

        foreach (int soundDetailsIndex in packedSoundDetails.Keys)
        {
            TR3SoundDetails details = packedSoundDetails[soundDetailsIndex];
            details.Sample = _sampleMap[details.Sample];

            int currentIndex = FindSoundDetailsIndex(details, soundDetails);
            if (currentIndex == -1)
            {
                _soundDetailsMap[soundDetailsIndex] = soundDetails.Count;
                soundDetails.Add(details);
            }
            else
            {
                _soundDetailsMap[soundDetailsIndex] = currentIndex;
            }
        }
    }

    private static int FindSoundDetailsIndex(TR3SoundDetails details, List<TR3SoundDetails> allDetails)
    {
        return allDetails.FindIndex
        (
            d =>
                d.Chance == details.Chance &&
                d.Characteristics == details.Characteristics &&
                d.Range == details.Range &&
                d.Sample == details.Sample &&
                d.Volume == details.Volume
        );
    }

    private void GenerateSoundIndexMap(TR3Level level, bool retainInternalIndices, Dictionary<int, short> packedSoundMapIndices)
    {
        _soundIndexMap = new Dictionary<int, int>();
        if (retainInternalIndices)
        {
            // This covers instances where internal sound indices are hard-coded in the game so
            // we have to retain those indices, but simply point them to the new SoundDetails
            foreach (int soundMapIndex in packedSoundMapIndices.Keys)
            {
                level.SoundMap[soundMapIndex] = (short)_soundDetailsMap[packedSoundMapIndices[soundMapIndex]];
                _soundIndexMap[soundMapIndex] = level.SoundMap[soundMapIndex];
            }
        }
        else
        {
            _soundMap = level.SoundMap.ToList();
            foreach (int soundMapIndex in packedSoundMapIndices.Keys)
            {
                // Sanity check
                if (soundMapIndex < 0 || soundMapIndex > _maxTR3Sounds - 1)
                {
                    continue;
                }

                short currentSoundDetailsIndex = packedSoundMapIndices[soundMapIndex];
                // For null indices, just point to the first existing null in the level
                if (currentSoundDetailsIndex == -1)
                {
                    _soundIndexMap[soundMapIndex] = _soundMap.FindIndex(i => i == -1);
                }
                else
                {
                    // Only insert the new SoundDetails index provided there isn't something there already
                    if (_soundMap[soundMapIndex] == -1)
                    {
                        short newSoundDetailsIndex = (short)_soundDetailsMap[currentSoundDetailsIndex];
                        level.SoundMap[soundMapIndex] = _soundMap[soundMapIndex] = newSoundDetailsIndex;
                    }
                    _soundIndexMap[soundMapIndex] = soundMapIndex;
                }
            }
        }
    }
}
