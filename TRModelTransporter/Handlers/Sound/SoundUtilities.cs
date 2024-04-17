using TRLevelControl.Model;
using TRModelTransporter.Model.Sound;

namespace TRModelTransporter.Handlers;

public static class SoundUtilities
{
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
