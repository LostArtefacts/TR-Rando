using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Model;
using TRModelTransporter.Model;

namespace TRModelTransporter.Helpers
{
    public class SoundUnpacker
    {
        private PackedSound _sound;
        private TR2Level _level;

        private List<uint> _sampleIndices;
        private List<TRSoundDetails> _soundDetails;
        private List<short> _soundMap;

        private Dictionary<uint, ushort> _sampleMap;
        private Dictionary<int, int> _soundDetailsMap;
        private Dictionary<int, int> _soundIndexMap;

        public IReadOnlyDictionary<int, int> SoundIndexMap => _soundIndexMap;

        public void Unpack(PackedSound sound, TR2Level level, bool retainInternalIndices = false)
        {
            _sound = sound;
            _level = level;

            GenerateSampleMap();
            GenerateSoundDetailsMap();
            GenerateSoundIndexMap(retainInternalIndices);

            _level.SampleIndices = _sampleIndices.ToArray();
            _level.NumSampleIndices = (uint)_sampleIndices.Count;

            _level.SoundDetails = _soundDetails.ToArray();
            _level.NumSoundDetails = (uint)_soundDetails.Count;
        }

        private void GenerateSampleMap()
        {
            // Insert each required SampleIndex value, which are the values
            // that point into MAIN.SFX. Note the first inserted index for updating
            // the relevant SoundDetails.Sample.
            _sampleIndices = _level.SampleIndices.ToList();
            _sampleMap = new Dictionary<uint, ushort>();

            foreach (ushort sampleIndex in _sound.SampleIndices.Keys)
            {
                uint[] sampleValues = _sound.SampleIndices[sampleIndex];
                if (sampleValues.Length > 0)
                {
                    // Store each value as long as it does not already exist
                    for (int i = 0; i < sampleValues.Length; i++)
                    {
                        if (!_sampleIndices.Contains(sampleValues[i]))
                        {
                            _sampleIndices.Add(sampleValues[i]);
                        }
                    }

                    // Store the "new" index of the first sample
                    _sampleMap[sampleIndex] = (ushort)_sampleIndices.IndexOf(sampleValues[0]);
                }
            }
        }

        private void GenerateSoundDetailsMap()
        {
            // Update each SoundDetails.Sample with the new SampleIndex values. Store
            // a map of old SoundsDetails indices to new.
            _soundDetails = _level.SoundDetails.ToList();
            _soundDetailsMap = new Dictionary<int, int>();

            foreach (int soundDetailsIndex in _sound.SoundDetails.Keys)
            {
                TRSoundDetails details = _sound.SoundDetails[soundDetailsIndex];
                details.Sample = _sampleMap[details.Sample];
                _soundDetailsMap[soundDetailsIndex] = _soundDetails.Count;
                _soundDetails.Add(details);
            }
        }

        private void GenerateSoundIndexMap(bool retainInternalIndices)
        {
            _soundIndexMap = new Dictionary<int, int>();
            if (retainInternalIndices)
            {
                // This covers instances where internal sound indices are hard-coded in the game so
                // we have to retain those indices, but simply point them to the new SoundDetails
                foreach (int soundMapIndex in _sound.SoundMapIndices.Keys)
                {
                    _level.SoundMap[soundMapIndex] = (short)_soundDetailsMap[_sound.SoundMapIndices[soundMapIndex]];
                    _soundIndexMap[soundMapIndex] = _level.SoundMap[soundMapIndex];
                }
            }
            else
            {
                _soundMap = _level.SoundMap.ToList();
                foreach (int soundMapIndex in _sound.SoundMapIndices.Keys)
                {
                    // Sanity check
                    if (soundMapIndex < 0 || soundMapIndex > 369)
                    {
                        continue;
                    }

                    short currentSoundDetailsIndex = _sound.SoundMapIndices[soundMapIndex];
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
                            _level.SoundMap[soundMapIndex] = _soundMap[soundMapIndex] = newSoundDetailsIndex;
                        }
                        _soundIndexMap[soundMapIndex] = soundMapIndex;
                    }
                }
            }
        }
    }
}