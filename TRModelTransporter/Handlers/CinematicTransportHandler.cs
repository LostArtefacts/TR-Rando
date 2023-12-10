using TRLevelControl.Model;
using TRModelTransporter.Model.Definitions;

namespace TRModelTransporter.Handlers;

public class CinematicTransportHandler
{
    public static void Export(TR1Level level, TR1ModelDefinition definition, IEnumerable<TR1Type> entityTypes)
    {
        List<TRCinematicFrame> frames = new();
        if (entityTypes != null && entityTypes.Contains(definition.Entity))
        {
            frames.AddRange(level.CinematicFrames);
        }

        definition.CinematicFrames = frames.ToArray();
    }

    public static void Export(TR2Level level, TR2ModelDefinition definition, IEnumerable<TR2Type> entityTypes)
    {
        List<TRCinematicFrame> frames = new();
        if (entityTypes != null && entityTypes.Contains(definition.Entity))
        {
            frames.AddRange(level.CinematicFrames);
        }

        definition.CinematicFrames = frames.ToArray();
    }

    public static void Export(TR3Level level, TR3ModelDefinition definition, IEnumerable<TR3Type> entityTypes)
    {
        List<TRCinematicFrame> frames = new();
        if (entityTypes != null && entityTypes.Contains(definition.Entity))
        {
            frames.AddRange(level.CinematicFrames);
        }

        definition.CinematicFrames = frames.ToArray();
    }

    public static void Import(TR1Level level, TR1ModelDefinition definition, bool forceOverwrite)
    {
        // We only import frames if the level doesn't have any already.
        if (level.NumCinematicFrames == 0 || forceOverwrite)
        {
            level.CinematicFrames = definition.CinematicFrames;
            level.NumCinematicFrames = (ushort)definition.CinematicFrames.Length;
        }
    }

    public static void Import(TR2Level level, TR2ModelDefinition definition, bool forceOverwrite)
    {
        if (level.NumCinematicFrames == 0 || forceOverwrite)
        {
            level.CinematicFrames = definition.CinematicFrames;
            level.NumCinematicFrames = (ushort)definition.CinematicFrames.Length;
        }
    }

    public static void Import(TR3Level level, TR3ModelDefinition definition, bool forceOverwrite)
    {
        if (level.NumCinematicFrames == 0 || forceOverwrite)
        {
            level.CinematicFrames = definition.CinematicFrames;
            level.NumCinematicFrames = (ushort)definition.CinematicFrames.Length;
        }
    }
}
