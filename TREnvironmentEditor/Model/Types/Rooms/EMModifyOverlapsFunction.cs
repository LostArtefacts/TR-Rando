using System.Collections.Generic;
using TRLevelReader.Helpers;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMModifyOverlapsFunction : BaseEMFunction
    {
        public Dictionary<ushort, List<ushort>> RemoveLinks { get; set; }
        public Dictionary<ushort, List<ushort>> AddLinks { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            if (RemoveLinks != null)
            {
                foreach (ushort boxIndex in RemoveLinks.Keys)
                {
                    TRBox box = level.Boxes[boxIndex];
                    List<ushort> overlaps = TR1BoxUtilities.GetOverlaps(level, box);
                    overlaps.RemoveAll(o => RemoveLinks[boxIndex].Contains(o));
                    TR1BoxUtilities.UpdateOverlaps(level, box, overlaps);
                }
            }

            if (AddLinks != null)
            {
                foreach (ushort boxIndex in AddLinks.Keys)
                {
                    TRBox box = level.Boxes[boxIndex];
                    List<ushort> overlaps = TR1BoxUtilities.GetOverlaps(level, box);
                    overlaps.AddRange(AddLinks[boxIndex]);
                    TR1BoxUtilities.UpdateOverlaps(level, box, overlaps);
                }
            }
        }

        public override void ApplyToLevel(TR2Level level)
        {
            if (RemoveLinks != null)
            {
                foreach (ushort boxIndex in RemoveLinks.Keys)
                {
                    TR2Box box = level.Boxes[boxIndex];
                    List<ushort> overlaps = TR2BoxUtilities.GetOverlaps(level, box);
                    overlaps.RemoveAll(o => RemoveLinks[boxIndex].Contains(o));
                    TR2BoxUtilities.UpdateOverlaps(level, box, overlaps);
                }
            }

            if (AddLinks != null)
            {
                foreach (ushort boxIndex in AddLinks.Keys)
                {
                    TR2Box box = level.Boxes[boxIndex];
                    List<ushort> overlaps = TR2BoxUtilities.GetOverlaps(level, box);
                    overlaps.AddRange(AddLinks[boxIndex]);
                    TR2BoxUtilities.UpdateOverlaps(level, box, overlaps);
                }
            }
        }

        public override void ApplyToLevel(TR3Level level)
        {
            if (RemoveLinks != null)
            {
                foreach (ushort boxIndex in RemoveLinks.Keys)
                {
                    TR2Box box = level.Boxes[boxIndex];
                    List<ushort> overlaps = TR2BoxUtilities.GetOverlaps(level, box);
                    overlaps.RemoveAll(o => RemoveLinks[boxIndex].Contains(o));
                    TR2BoxUtilities.UpdateOverlaps(level, box, overlaps);
                }
            }

            if (AddLinks != null)
            {
                foreach (ushort boxIndex in AddLinks.Keys)
                {
                    TR2Box box = level.Boxes[boxIndex];
                    List<ushort> overlaps = TR2BoxUtilities.GetOverlaps(level, box);
                    overlaps.AddRange(AddLinks[boxIndex]);
                    TR2BoxUtilities.UpdateOverlaps(level, box, overlaps);
                }
            }
        }
    }
}