using TRLevelControl.Model;
using TRLevelControl.Model.Enums;
using TRModelTransporter.Model.Definitions;

namespace TRModelTransporter.Handlers;

public class CinematicTransportHandler
{
    public static void Export(TR1Level level, TR1ModelDefinition definition, IEnumerable<TREntities> entityTypes)
    {
        List<TRCinematicFrame> frames = new();
        if (entityTypes != null && entityTypes.Contains(definition.Entity))
        {
            frames.AddRange(level.CinematicFrames);
        }

        definition.CinematicFrames = frames.ToArray();
    }

    public static void Export(TR2Level level, TR2ModelDefinition definition, IEnumerable<TR2Entities> entityTypes)
    {
        List<TRCinematicFrame> frames = new();
        if (entityTypes != null && entityTypes.Contains(definition.Entity))
        {
            frames.AddRange(level.CinematicFrames);
        }

        definition.CinematicFrames = frames.ToArray();
    }

    public static void Export(TR3Level level, TR3ModelDefinition definition, IEnumerable<TR3Entities> entityTypes)
    {
        List<TRCinematicFrame> frames = new();
        if (entityTypes != null && entityTypes.Contains(definition.Entity))
        {
            frames.AddRange(level.CinematicFrames);
        }

        definition.CinematicFrames = frames.ToArray();
    }

    public static void Import(TR1Level level, TR1ModelDefinition definition)
    {
        // We only import frames if the level doesn't have any already.
        if (level.NumCinematicFrames == 0)
        {
            level.CinematicFrames = definition.CinematicFrames;
            level.NumCinematicFrames = (ushort)definition.CinematicFrames.Length;
        }
    }

    public static void Import(TR2Level level, TR2ModelDefinition definition)
    {
        if (level.NumCinematicFrames == 0)
        {
            level.CinematicFrames = definition.CinematicFrames;
            level.NumCinematicFrames = (ushort)definition.CinematicFrames.Length;
        }
    }

    public static void Import(TR3Level level, TR3ModelDefinition definition)
    {
        if (level.NumCinematicFrames == 0)
        {
            level.CinematicFrames = definition.CinematicFrames;
            level.NumCinematicFrames = (ushort)definition.CinematicFrames.Length;
        }
    }
}
