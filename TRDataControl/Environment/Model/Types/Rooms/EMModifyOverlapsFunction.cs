using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMModifyOverlapsFunction : BaseEMFunction
{
    public Dictionary<ushort, List<ushort>> RemoveLinks { get; set; }
    public Dictionary<ushort, List<ushort>> AddLinks { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        UpdateBoxes(level.Boxes);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        UpdateBoxes(level.Boxes);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        UpdateBoxes(level.Boxes);
    }

    private void UpdateBoxes(List<TRBox> boxes)
    {
        if (RemoveLinks != null)
        {
            foreach (ushort boxIndex in RemoveLinks.Keys)
            {
                TRBox box = boxes[boxIndex];
                box.Overlaps.RemoveAll(o => RemoveLinks[boxIndex].Contains(o));
            }
        }

        if (AddLinks != null)
        {
            foreach (ushort boxIndex in AddLinks.Keys)
            {
                TRBox box = boxes[boxIndex];
                box.Overlaps.AddRange(AddLinks[boxIndex]);
            }
        }
    }
}
