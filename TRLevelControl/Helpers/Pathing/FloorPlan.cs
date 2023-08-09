using System.Collections.Generic;
using System.Drawing;

namespace TRLevelControl.Helpers;

public class FloorPlan
{
    private readonly int _depth;
    private readonly List<Rectangle> _scans;

    public IReadOnlyList<Rectangle> Scans => _scans;

    public List<Box> Boxes { get; private set; }

    public FloorPlan(int depth)
    {
        _depth = depth;
        _scans = new List<Rectangle>();
    }

    public void AddFloor(int x, int z)
    {
        _scans.Add(MakeFloor(x, z));
    }

    private Rectangle MakeFloor(int x, int z)
    {
        return new Rectangle(x, _depth - z - 1, 1, 1);
    }

    public void GenerateBoxes(int boxIndexStart, sbyte height)
    {
        MergeFloorSpace();
        Boxes = new List<Box>();
        for (int i = 0; i < Scans.Count; i++)
        {
            Rectangle scan = Scans[i];
            // Z is flipped
            Boxes.Add(new Box
            {
                Index = boxIndexStart + i,
                MinX = scan.X,
                MaxX = scan.X + scan.Width,
                MinZ = _depth - (scan.Y + scan.Height),
                MaxZ = _depth - scan.Y,
                Floor = height
            });
        }
    }

    public Box GetBox(int x, int z)
    {
        Rectangle rect = MakeFloor(x, z);
        for (int i = 0; i < _scans.Count; i++)
        {
            if (_scans[i].Contains(rect))
            {
                return Boxes[i];
            }
        }
        return null;
    }

    private void MergeFloorSpace()
    {
        // Scan the floor using positive then negative lookup - the
        // method that produces the fewest boxes wins.
        List<Rectangle> positiveScans = new(_scans);
        List<Rectangle> negativeScans = new(_scans);
        MergeFloorSpace(positiveScans, true);
        MergeFloorSpace(negativeScans, false);
        _scans.Clear();
        _scans.AddRange(positiveScans.Count <= negativeScans.Count ? positiveScans : negativeScans);
    }

    private void MergeFloorSpace(List<Rectangle> scans, bool positiveLookup)
    {
        for (int i = scans.Count - 1; i >= 0; i--)
        {
            Rectangle scan = scans[i];
            for (int j = 0; j < scans.Count; j++)
            {
                if (i == j)
                    continue;
                Rectangle neighbour = scans[j];

                if (positiveLookup)
                {
                    // Can the tile to the right or below be merged with this one?
                    if (neighbour.X == scan.X && neighbour.Y == scan.Y + scan.Height && neighbour.Width == scan.Width)
                    {
                        scans[i] = new Rectangle(scan.X, scan.Y, scan.Width, scan.Height + neighbour.Height);
                        scans.RemoveAt(j);
                        break;
                    }
                    else if (neighbour.X == scan.X + scan.Width && neighbour.Y == scan.Y && neighbour.Height == scan.Height)
                    {
                        scans[i] = new Rectangle(scan.X, scan.Y, scan.Width + neighbour.Width, scan.Height);
                        scans.RemoveAt(j);
                        break;
                    }
                }
                else
                {
                    // Can the tile to the left or above be merged with this one?
                    if (neighbour.X == scan.X && neighbour.Y == scan.Y - neighbour.Height && neighbour.Width == scan.Width)
                    {
                        scans[i] = new Rectangle(scan.X, scan.Y - neighbour.Height, scan.Width, scan.Height + neighbour.Height);
                        scans.RemoveAt(j);
                        break;
                    }
                    else if (neighbour.X == scan.X - neighbour.Width && neighbour.Y == scan.Y && neighbour.Height == scan.Height)
                    {
                        scans[i] = new Rectangle(scan.X - neighbour.Width, scan.Y, scan.Width + neighbour.Width, scan.Height);
                        scans.RemoveAt(j);
                        break;
                    }
                }
            }
        }
    }
}
