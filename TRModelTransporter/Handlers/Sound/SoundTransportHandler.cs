using TRLevelControl.Model;
using TRModelTransporter.Model.Definitions;

namespace TRModelTransporter.Handlers;

public class SoundTransportHandler
{
    public static void Export(TR1Level level, TR1ModelDefinition definition, short[] hardcodedSounds)
    {
        if (hardcodedSounds == null || hardcodedSounds.Length == 0)
        {
            return;
        }

        definition.SoundEffects ??= new();
        foreach (short soundID in hardcodedSounds)
        {
            TR1SFX sfxID = (TR1SFX)soundID;
            if (level.SoundEffects.ContainsKey(sfxID))
            {
                definition.SoundEffects[sfxID] = level.SoundEffects[sfxID];
            }
        }
    }

    public static void Export(TR2Level level, TR2ModelDefinition definition, short[] hardcodedSounds)
    {
        if (hardcodedSounds == null || hardcodedSounds.Length == 0)
        {
            return;
        }

        definition.SoundEffects ??= new();
        foreach (short soundID in hardcodedSounds)
        {
            TR2SFX sfxID = (TR2SFX)soundID;
            if (level.SoundEffects.ContainsKey(sfxID))
            {
                definition.SoundEffects[sfxID] = level.SoundEffects[sfxID];
            }
        }
    }

    public static void Export(TR3Level level, TR3ModelDefinition definition, short[] hardcodedSounds)
    {
        definition.HardcodedSound = SoundUtilities.BuildPackedSound(level.SoundMap, level.SoundDetails, level.SampleIndices, hardcodedSounds);
    }

    public static void Import(TR1Level level, IEnumerable<TR1ModelDefinition> definitions)
    {
        foreach (TR1ModelDefinition definition in definitions)
        {
            if (definition.SoundEffects == null)
            {
                continue;
            }

            foreach (TR1SFX sfxID in definition.SoundEffects.Keys)
            {
                level.SoundEffects[sfxID] = definition.SoundEffects[sfxID];
            }
        }
    }

    public static void Import(TR2Level level, IEnumerable<TR2ModelDefinition> definitions)
    {
        foreach (TR2ModelDefinition definition in definitions)
        {
            if (definition.SoundEffects == null)
            {
                continue;
            }

            foreach (TR2SFX sfxID in definition.SoundEffects.Keys)
            {
                level.SoundEffects[sfxID] = definition.SoundEffects[sfxID];
            }
        }
    }

    public static void Import(TR3Level level, IEnumerable<TR3ModelDefinition> definitions)
    {
        SoundUnpacker unpacker = new();
        foreach (TR3ModelDefinition definition in definitions)
        {
            if (definition.HardcodedSound != null)
            {
                unpacker.Unpack(definition.HardcodedSound, level, true);
            }
        }
    }
}
