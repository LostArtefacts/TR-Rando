using System.Collections.Generic;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TRModelTransporter.Handlers
{
    public class CinematicTransportHandler : AbstractTransportHandler
    {
        private static readonly List<TR2Entities> _entityTypes = new List<TR2Entities> { TR2Entities.DragonExplosionEmitter_N };

        public override void Export()
        {
            List<TRCinematicFrame> frames = new List<TRCinematicFrame>();
            if (_entityTypes.Contains(Definition.Entity))
            {
                frames.AddRange(Level.CinematicFrames);
            }

            Definition.CinematicFrames = frames.ToArray();
        }

        public override void Import()
        {
            // We only import frames if the level doesn't have any already. This creates a bit of a strange effect
            // with the dragon death as it inherits the frames already in the level (e.g. Rig, HSH start animations).
            // The alternative is replacing those with the one for the dragon death, but that itself will cause those
            // animations to behave strangely - plus it would also provide a spoiler that here be dragons...
            if (Level.NumCinematicFrames > 0)
            {
                return;
            }

            Level.CinematicFrames = Definition.CinematicFrames;
            Level.NumCinematicFrames = (ushort)Definition.CinematicFrames.Length;
        }
    }
}