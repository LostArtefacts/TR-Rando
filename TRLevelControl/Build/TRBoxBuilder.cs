using System.Diagnostics;
using TRLevelControl.Model;

namespace TRLevelControl.Build;

public class TRBoxBuilder
{
    private static readonly ushort _tr1NoOverlap = ushort.MaxValue;
    private static readonly ushort _tr3NoOverlap = 2047;

    private readonly TRGameVersion _version;
    private readonly ITRLevelObserver _observer;
    private readonly ushort _noOverlap;

    private List<ushort> _overlaps;

    public TRBoxBuilder(TRGameVersion version, ITRLevelObserver observer = null)
    {
        _version = version;
        _observer = observer;
        _noOverlap = _version <= TRGameVersion.TR2 ? _tr1NoOverlap : _tr3NoOverlap;
    }

    public List<TRBox> ReadBoxes(TRLevelReader reader)
    {
        List<TRBox> boxes = new();
        _overlaps = new();

        uint numBoxes = reader.ReadUInt32();
        for (int i = 0; i < numBoxes; i++)
        {
            boxes.Add(reader.ReadBox(_version));
            _overlaps.Add(reader.ReadUInt16());
        }

        BuildOverlaps(reader, boxes);
        BuildZones(reader, boxes);

        return boxes;
    }

    private void BuildOverlaps(TRLevelReader reader, List<TRBox> boxes)
    {
        uint numOverlaps = reader.ReadUInt32();
        ushort[] overlaps = reader.ReadUInt16s(numOverlaps);

        if (_version > TRGameVersion.TR2
            && _observer != null
            && overlaps.Length >= _noOverlap
            && _overlaps.Contains(_noOverlap)
            && (overlaps[_noOverlap - 1] & 0x8000) > 0)
        {
            // A handful of levels have a bug that points a box to overlap entry 2047
            // but this is ignored by the game as it means "NOBOX". It's zeroed on
            // write, but observers can handle it if needed.
            _observer.OnBadOverlapRead(overlaps[_noOverlap]);
        }

        for (int i = 0; i < boxes.Count; i++)
        {
            TRBox box = boxes[i];
            ushort overlapIndex = _overlaps[i];
            box.Blockable = (overlapIndex & 0x8000) > 0;
            box.Blocked = (overlapIndex & 0x4000) > 0;

            box.Overlaps = new();
            if (overlapIndex == _noOverlap)
            {
                continue;
            }

            int index = overlapIndex & 0x3FFF;
            if (index >= overlaps.Length)
            {
                continue;
            }

            ushort boxNumber;
            bool done = false;
            do
            {
                boxNumber = overlaps[index++];
                if ((boxNumber & 0x8000) > 0)
                {
                    done = true;
                    boxNumber &= 0x7FFF;
                }
                box.Overlaps.Add(boxNumber);
            }
            while (!done);
        }
    }

    private void BuildZones(TRLevelReader reader, List<TRBox> boxes)
    {
        List<TRZoneType> zoneTypes = new()
        {
            TRZoneType.Zone1,
            TRZoneType.Zone2,
        };

        if (_version > TRGameVersion.TR1)
        {
            zoneTypes.Add(TRZoneType.Zone3);
            zoneTypes.Add(TRZoneType.Zone4);
        }

        int numZoneData = 2 * boxes.Count * (zoneTypes.Count + 1);
        long targetRead = reader.BaseStream.Position + numZoneData * sizeof(ushort);

        void Build(Func<TRZoneGroup, TRZone> zoneAction)
        {
            foreach (TRZoneType zoneType in zoneTypes)
            {
                foreach (TRBox box in boxes)
                {
                    zoneAction(box.Zone).Ground[zoneType] = reader.ReadUInt16();
                }
            }

            foreach (TRBox box in boxes)
            {
                zoneAction(box.Zone).Fly = reader.ReadUInt16();
            }
        }

        Build(zoneGroup => zoneGroup.FlipOffZone);
        Build(zoneGroup => zoneGroup.FlipOnZone);

        Debug.Assert(reader.BaseStream.Position == targetRead);
    }

    public void WriteBoxes(TRLevelWriter writer, List<TRBox> boxes)
    {
        _overlaps = new();

        writer.Write((uint)boxes.Count);
        foreach (TRBox box in boxes)
        {
            WriteBox(writer, box);
        }

        WriteOverlaps(writer);
        WriteZones(writer, boxes);
    }

    private void WriteBox(TRLevelWriter writer, TRBox box)
    {
        int overlapIndex;
        if (box.Overlaps.Count == 0)
        {
            overlapIndex = _noOverlap;
        }
        else
        {
            overlapIndex = _overlaps.Count;
            if (box.Blockable)
            {
                overlapIndex |= 0x8000;
            }
            if (box.Blocked)
            {
                overlapIndex |= 0x4000;
            }

            _overlaps.AddRange(box.Overlaps);
            _overlaps[^1] |= 0x8000;

            if (_overlaps.Count == _noOverlap)
            {
                // Ensure we don't have a box that has overlaps pointing to the "NOBOX" marker.
                _overlaps.Add(_observer?.GetBadOverlap() ?? 0);
            }
        }

        writer.Write(box, (ushort)overlapIndex, _version);
    }

    private void WriteOverlaps(TRLevelWriter writer)
    {
        writer.Write((uint)_overlaps.Count);
        writer.Write(_overlaps);
    }

    private void WriteZones(TRLevelWriter writer, List<TRBox> boxes)
    {
        List<TRZoneType> zoneTypes = new()
        {
            TRZoneType.Zone1,
            TRZoneType.Zone2,
        };

        if (_version > TRGameVersion.TR1)
        {
            zoneTypes.Add(TRZoneType.Zone3);
            zoneTypes.Add(TRZoneType.Zone4);
        }

        IEnumerable<TRZoneGroup> zoneGroups = boxes.Select(b => b.Zone);

        void Flatten(Func<TRZoneGroup, TRZone> zoneAction)
        {
            foreach (TRZoneType zoneType in zoneTypes)
            {
                foreach (TRZoneGroup zoneGroup in zoneGroups)
                {
                    writer.Write(zoneAction(zoneGroup).Ground[zoneType]);
                }
            }

            foreach (TRZoneGroup zoneGroup in zoneGroups)
            {
                writer.Write(zoneAction(zoneGroup).Fly);
            }
        }

        Flatten(zoneGroup => zoneGroup.FlipOffZone);
        Flatten(zoneGroup => zoneGroup.FlipOnZone);
    }
}
