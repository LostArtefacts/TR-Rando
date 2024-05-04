using TRLevelControl.Model;
using TRModelTransporter.Model.Definitions;

namespace TRModelTransporter.Handlers;

public class SoundTransportHandler
{
    public static void Export(TR1Level level, TR1Blob definition, short[] hardcodedSounds)
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

    public static void Export(TR2Level level, TR2Blob definition, short[] hardcodedSounds)
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

    public static void Export(TR3Level level, TR3Blob definition, short[] hardcodedSounds)
    {
        if (hardcodedSounds == null || hardcodedSounds.Length == 0)
        {
            return;
        }

        definition.SoundEffects ??= new();
        foreach (short soundID in hardcodedSounds)
        {
            TR3SFX sfxID = (TR3SFX)soundID;
            if (level.SoundEffects.ContainsKey(sfxID))
            {
                definition.SoundEffects[sfxID] = level.SoundEffects[sfxID];
            }
        }
    }

    public static void Import(TR1Level level, IEnumerable<TR1Blob> definitions)
    {
        foreach (TR1Blob definition in definitions)
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

    public static void Import(TR2Level level, IEnumerable<TR2Blob> definitions)
    {
        foreach (TR2Blob definition in definitions)
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

    public static void Import(TR3Level level, IEnumerable<TR3Blob> definitions)
    {
        foreach (TR3Blob definition in definitions)
        {
            if (definition.SoundEffects == null)
            {
                continue;
            }

            foreach (TR3SFX sfxID in definition.SoundEffects.Keys)
            {
                level.SoundEffects[sfxID] = definition.SoundEffects[sfxID];
            }
        }
    }
}
