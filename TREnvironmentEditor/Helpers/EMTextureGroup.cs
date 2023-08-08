using System.ComponentModel;
using TREnvironmentEditor.Model.Types;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Helpers;

public class EMTextureGroup
{
    public ushort Floor { get; set; }
    public ushort Ceiling { get; set; }
    public Direction WallAlignment { get; set; }
    // 64x64
    [DefaultValue(ushort.MaxValue)]
    public ushort Wall4 { get; set; }
    // 64x48
    [DefaultValue(ushort.MaxValue)]
    public ushort Wall3 { get; set; }
    // 64x32
    [DefaultValue(ushort.MaxValue)]
    public ushort Wall2 { get; set; }
    // 64x16
    [DefaultValue(ushort.MaxValue)]
    public ushort Wall1 { get; set; }

    public int RandomRotationSeed { get; set; }

    private Random _generator;

    public EMTextureGroup()
    {
        Wall4 = Wall3 = Wall2 = Wall1 = ushort.MaxValue;
        WallAlignment = Direction.Up;
    }

    public ushort GetWall(int height)
    {
        List<ushort> temp = new() { Wall1, Wall2, Wall3, Wall4 };
        ushort result = ushort.MaxValue;
        int clicks = Math.Min(height / 256, 4);
        
        for (int i = 0; i < temp.Count; i++)
        {
            if (clicks == i + 1)
            {
                if (temp[i] != ushort.MaxValue)
                {
                    result = temp[i];
                    break;
                }
                else
                {
                    clicks++;
                }
            }
        }

        return result == ushort.MaxValue ? Floor : result;
    }

    public void RandomizeRotation(TRFace4 face, int height)
    {
        if (RandomRotationSeed <= 0)
        {
            return;
        }

        if (_generator == null)
        {
            _generator = new Random(RandomRotationSeed);
        }

        Dictionary<int, int> remap = null;
        switch (_generator.Next(0, 4))
        {
            case 1:
                remap = new Dictionary<int, int>
                {
                    [0] = 1,
                    [1] = 2,
                    [2] = 3,
                    [3] = 0
                };
                break;
            case 2:
                remap = new Dictionary<int, int>
                {
                    [0] = 2,
                    [1] = 3,
                    [2] = 0,
                    [3] = 1
                };
                break;
            case 3:
                remap = new Dictionary<int, int>
                {
                    [0] = 3,
                    [1] = 0,
                    [2] = 1,
                    [3] = 2
                };
                break;
            default:
                // Leave it as-is
                break;
        }

        if (remap != null && height == 1024)
        {
            face.Vertices = EMModifyFaceFunction.RotateVertices(face.Vertices, new EMFaceRotation
            {
                VertexRemap = remap
            });
        }
    }
}
