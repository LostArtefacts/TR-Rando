using System.Text;

namespace TRLevelControl.Model;

public class TRModel
{
    public uint ID { get; set; }

    public ushort NumMeshes { get; set; }

    public ushort StartingMesh { get; set; }

    public uint MeshTree { get; set; }

    public uint FrameOffset { get; set; }

    public ushort Animation { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new(base.ToString());

        sb.Append(" ID: " + ID);
        sb.Append(" NumMeshes: " + NumMeshes);
        sb.Append(" StartingMesh: " + StartingMesh);
        sb.Append(" MeshTree: " + MeshTree);
        sb.Append(" FrameOffset: " + FrameOffset);
        sb.Append(" Animation: " + Animation);

        return sb.ToString();
    }
}
