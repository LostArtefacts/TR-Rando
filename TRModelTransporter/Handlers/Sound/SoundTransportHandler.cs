using System.Collections.Generic;
using TRLevelControl.Model;
using TRModelTransporter.Model.Definitions;

namespace TRModelTransporter.Handlers;

public class SoundTransportHandler
{
    public void Export(TR1Level level, TR1ModelDefinition definition, short[] hardcodedSounds)
    {
        definition.HardcodedSound = SoundUtilities.BuildPackedSound(level.SoundMap, level.SoundDetails, level.SampleIndices, level.Samples, hardcodedSounds);
    }

    public void Export(TR2Level level, TR2ModelDefinition definition, short[] hardcodedSounds)
    {
        definition.HardcodedSound = SoundUtilities.BuildPackedSound(level.SoundMap, level.SoundDetails, level.SampleIndices, hardcodedSounds);
    }

    public void Export(TR3Level level, TR3ModelDefinition definition, short[] hardcodedSounds)
    {
        definition.HardcodedSound = SoundUtilities.BuildPackedSound(level.SoundMap, level.SoundDetails, level.SampleIndices, hardcodedSounds);
    }

    public void Import(TR1Level level, IEnumerable<TR1ModelDefinition> definitions)
    {
        SoundUnpacker unpacker = new SoundUnpacker();
        foreach (TR1ModelDefinition definition in definitions)
        {
            if (definition.HardcodedSound != null)
            {
                unpacker.Unpack(definition.HardcodedSound, level, true);
            }
        }
    }

    public void Import(TR2Level level, IEnumerable<TR2ModelDefinition> definitions)
    {
        SoundUnpacker unpacker = new SoundUnpacker();
        foreach (TR2ModelDefinition definition in definitions)
        {
            if (definition.HardcodedSound != null)
            {
                unpacker.Unpack(definition.HardcodedSound, level, true);
            }
        }
    }

    public void Import(TR3Level level, IEnumerable<TR3ModelDefinition> definitions)
    {
        SoundUnpacker unpacker = new SoundUnpacker();
        foreach (TR3ModelDefinition definition in definitions)
        {
            if (definition.HardcodedSound != null)
            {
                unpacker.Unpack(definition.HardcodedSound, level, true);
            }
        }
    }
}
