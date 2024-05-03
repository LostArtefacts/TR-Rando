using TRLevelControl.Model;

namespace TRLevelControl.Build;

public class TRBoxBuilder
{
    private readonly TRGameVersion _version;
    private readonly ITRLevelObserver _observer;

    public TRBoxBuilder(TRGameVersion version, ITRLevelObserver observer = null)
    {
        _version = version;
        _observer = observer;
    }

    public List<TRBox> ReadBoxes(TRLevelReader reader)
    {
        List<TRBox> boxes = new();

        uint numBoxes = reader.ReadUInt32();
        for (int i = 0; i < numBoxes; i++)
        {
            TRBox box = reader.ReadBox(_version);
            box.OverlapIndex = reader.ReadUInt16();
            boxes.Add(box);
        }

        return boxes;
    }

    public void WriteBoxes(TRLevelWriter writer, List<TRBox> boxes)
    {
        writer.Write((uint)boxes.Count);
        foreach (TRBox box in boxes)
        {
            writer.Write(box, box.OverlapIndex, _version);
        }
    }
}
