using TRLevelControl.Model;
using TRModelTransporter.Model.Sound;

namespace TRModelTransporter.Handlers;

public static class SoundUtilities
{
    public static void ImportLevelSound(TR1Level baseLevel, TR1Level sourceLevel, short[] soundIDs)
    {
        TR1PackedSound sound = BuildPackedSound(sourceLevel.SoundMap, sourceLevel.SoundDetails, sourceLevel.SampleIndices, sourceLevel.Samples, soundIDs);
        new SoundUnpacker().Unpack(sound, baseLevel);
        SoundUtilities.ResortSoundIndices(baseLevel);
    }

    public static TR1PackedSound BuildPackedSound(short[] soundMap, List<TRSoundDetails> soundDetails, List<uint> sampleIndices, List<byte> wavSamples, short[] hardcodedSounds)
    {
        if (hardcodedSounds == null || hardcodedSounds.Length == 0)
        {
            return null;
        }

        TR1PackedSound packedSound = new();

        short id = -2;
        foreach (int index in hardcodedSounds)
        {
            packedSound.SoundMapIndices[index] = id--;
        }

        int offset = 2;
        foreach (int index in packedSound.SoundMapIndices.Keys)
        {
            short val = packedSound.SoundMapIndices[index];
            TRSoundDetails details = soundDetails[soundMap[index]];

            uint[] samples = new uint[details.NumSounds];
            for (int i = 0; i < details.NumSounds; i++)
            {
                ushort sampleIndex = (ushort)(details.Sample + i);
                samples[i] = sampleIndices[sampleIndex];

                uint nextIndex = sampleIndex == sampleIndices.Count - 1 ? (uint)wavSamples.Count : sampleIndices[sampleIndex + 1];
                packedSound.Samples[samples[i]] = AnimationUtilities.GetSample(samples[i], nextIndex, wavSamples);
            }

            packedSound.SampleIndices[(ushort)(ushort.MaxValue - offset)] = samples;

            packedSound.SoundDetails[val] = new TRSoundDetails
            {
                Chance = details.Chance,
                Characteristics = details.Characteristics,
                Sample = (ushort)(ushort.MaxValue - offset),
                Volume = details.Volume
            };
            offset++;
        }

        return packedSound;
    }

    public static TR2PackedSound BuildPackedSound(short[] soundMap, List<TRSoundDetails> soundDetails, List<uint> sampleIndices, short[] hardcodedSounds)
    {
        if (hardcodedSounds == null || hardcodedSounds.Length == 0)
        {
            return null;
        }

        TR2PackedSound packedSound = new();

        short id = -2;
        foreach (int index in hardcodedSounds)
        {
            packedSound.SoundMapIndices[index] = id--;
        }

        int offset = 2;
        foreach (int index in packedSound.SoundMapIndices.Keys)
        {
            short val = packedSound.SoundMapIndices[index];
            TRSoundDetails details = soundDetails[soundMap[index]];

            uint[] samples = new uint[details.NumSounds];
            for (int i = 0; i < details.NumSounds; i++)
            {
                samples[i] = sampleIndices[(ushort)(details.Sample + i)];
            }

            packedSound.SampleIndices[(ushort)(ushort.MaxValue - offset)] = samples;

            packedSound.SoundDetails[val] = new TRSoundDetails
            {
                Chance = details.Chance,
                Characteristics = details.Characteristics,
                Sample = (ushort)(ushort.MaxValue - offset),
                Volume = details.Volume
            };
            offset++;
        }

        return packedSound;
    }

    public static TR3PackedSound BuildPackedSound(short[] soundMap, List<TR3SoundDetails> soundDetails, List<uint> sampleIndices, short[] hardcodedSounds)
    {
        if (hardcodedSounds == null || hardcodedSounds.Length == 0)
        {
            return null;
        }

        TR3PackedSound packedSound = new();

        short id = -2;
        foreach (int index in hardcodedSounds)
        {
            packedSound.SoundMapIndices[index] = id--;
        }

        int offset = 2;
        foreach (int index in packedSound.SoundMapIndices.Keys)
        {
            short val = packedSound.SoundMapIndices[index];
            TR3SoundDetails details = soundDetails[soundMap[index]];

            uint[] samples = new uint[details.NumSounds];
            for (int i = 0; i < details.NumSounds; i++)
            {
                samples[i] = sampleIndices[(ushort)(details.Sample + i)];
            }

            packedSound.SampleIndices[(ushort)(ushort.MaxValue - offset)] = samples;

            packedSound.SoundDetails[val] = new TR3SoundDetails
            {
                Chance = details.Chance,
                Characteristics = details.Characteristics,
                Sample = (ushort)(ushort.MaxValue - offset),
                Volume = details.Volume,
                Range = details.Range
            };
            offset++;
        }

        return packedSound;
    }

    public static void ResortSoundIndices(TR1Level level)
    {
        List<short> newSoundMap = new();
        List<TRSoundDetails> newSoundDetails = new();
        List<uint> newSampleIndices = new();
        List<byte> newSamples = new();

        for (int soundID = 0; soundID < level.SoundMap.Length; soundID++)
        {
            if (level.SoundMap[soundID] == -1)
            {
                newSoundMap.Add(-1);
            }
            else
            {
                TRSoundDetails details = level.SoundDetails[level.SoundMap[soundID]];
                newSoundMap.Add((short)newSoundDetails.Count);
                newSoundDetails.Add(details);

                ushort oldSample = details.Sample;
                details.Sample = (ushort)newSampleIndices.Count;
                for (int i = 0; i < details.NumSounds; i++)
                {
                    ushort samplePointerIndex = (ushort)(oldSample + i);

                    uint oldSampleIndex = level.SampleIndices[oldSample + i];
                    uint nextSampleIndex = samplePointerIndex == level.SampleIndices.Count - 1 ? (uint)level.Samples.Count : level.SampleIndices[samplePointerIndex + 1];

                    newSampleIndices.Add((uint)newSamples.Count);
                    newSamples.AddRange(AnimationUtilities.GetSample(oldSampleIndex, nextSampleIndex, level.Samples));
                }
            }
        }

        level.SoundMap = newSoundMap.ToArray();

        level.SoundDetails.Clear();
        level.SampleIndices.Clear();
        level.Samples.Clear();

        level.SoundDetails.AddRange(newSoundDetails);
        level.SampleIndices.AddRange(newSampleIndices);
        level.Samples.AddRange(newSamples);
    }

    public static void ResortSoundIndices(TR2Level level)
    {
        List<TRSoundDetails> soundDetails = level.SoundDetails.ToList();
        List<short> soundMap = level.SoundMap.ToList();

        ResortSoundIndices(level.SampleIndices, soundDetails, soundMap);

        level.SoundDetails.Clear();
        level.SoundDetails.AddRange(soundDetails);
        level.SoundMap = soundMap.ToArray();
    }

    public static void ResortSoundIndices(TR3Level level)
    {
        List<TR3SoundDetails> soundDetails = level.SoundDetails.ToList();
        List<short> soundMap = level.SoundMap.ToList();

        ResortSoundIndices(level.SampleIndices, soundDetails, soundMap);

        level.SoundDetails.Clear();
        level.SoundDetails.AddRange(soundDetails);
        level.SoundMap = soundMap.ToArray();
    }

    private static void ResortSoundIndices(List<uint> sampleIndices, List<TRSoundDetails> soundDetails, List<short> soundMap)
    {
        // Store the values from SampleIndices against their current positions
        // in the list.             
        Dictionary<int, uint> indexMap = new();
        for (int i = 0; i < sampleIndices.Count; i++)
        {
            indexMap[i] = sampleIndices[i];
        }

        // Sort the indices to avoid the game crashing
        sampleIndices.Sort();

        // Remap each SoundDetail to use the new index of the sample it points to
        foreach (TRSoundDetails details in soundDetails)
        {
            details.Sample = (ushort)sampleIndices.IndexOf(indexMap[details.Sample]);
        }

        // Repeat for SoundMap -> SoundDetails
        Dictionary<int, TRSoundDetails> soundMapIndices = new();
        for (int i = 0; i < soundMap.Count; i++)
        {
            if (soundMap[i] != -1)
            {
                soundMapIndices[i] = soundDetails[soundMap[i]];
            }
        }

        soundDetails.Sort(delegate (TRSoundDetails d1, TRSoundDetails d2)
        {
            return d1.Sample.CompareTo(d2.Sample);
        });

        foreach (int mapIndex in soundMapIndices.Keys)
        {
            TRSoundDetails details = soundMapIndices[mapIndex];
            soundMap[mapIndex] = (short)soundDetails.IndexOf(details);
        }
    }

    private static void ResortSoundIndices(List<uint> sampleIndices, List<TR3SoundDetails> soundDetails, List<short> soundMap)
    {
        // Store the values from SampleIndices against their current positions
        // in the list.             
        Dictionary<int, uint> indexMap = new();
        for (int i = 0; i < sampleIndices.Count; i++)
        {
            indexMap[i] = sampleIndices[i];
        }

        // Sort the indices to avoid the game crashing
        sampleIndices.Sort();

        // Remap each SoundDetail to use the new index of the sample it points to
        foreach (TR3SoundDetails details in soundDetails)
        {
            details.Sample = (ushort)sampleIndices.IndexOf(indexMap[details.Sample]);
        }

        // Repeat for SoundMap -> SoundDetails
        Dictionary<int, TR3SoundDetails> soundMapIndices = new();
        for (int i = 0; i < soundMap.Count; i++)
        {
            if (soundMap[i] != -1)
            {
                soundMapIndices[i] = soundDetails[soundMap[i]];
            }
        }

        soundDetails.Sort(delegate (TR3SoundDetails d1, TR3SoundDetails d2)
        {
            return d1.Sample.CompareTo(d2.Sample);
        });

        foreach (int mapIndex in soundMapIndices.Keys)
        {
            TR3SoundDetails details = soundMapIndices[mapIndex];
            soundMap[mapIndex] = (short)soundDetails.IndexOf(details);
        }
    }
}
