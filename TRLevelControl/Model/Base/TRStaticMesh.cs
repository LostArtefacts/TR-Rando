using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TRStaticMesh : ISerializableCompact
{
    public uint ID { get; set; }

    public ushort Mesh { get; set; }

    public TRBoundingBox VisibilityBox { get; set; }

    public TRBoundingBox CollisionBox { get; set; }

    public ushort Flags { get; set; }

    public bool NonCollidable
    {
        get => (Flags & 1) > 0;
        set
        {
            if (value)
            {
                Flags |= 1;
            }
            else
            {
                Flags ^= 1;
            }
        }
    }

    public bool Visible
    {
        get => (Flags & 2) > 0;
        set
        {
            if (value)
            {
                Flags |= 2;
            }
            else
            {
                Flags ^= 2;
            }
        }
    }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write(ID);
            writer.Write(Mesh);
            writer.Write(VisibilityBox.Serialize());
            writer.Write(CollisionBox.Serialize());
            writer.Write(Flags);
        }

        return stream.ToArray();
    }

    public override string ToString()
    {
        StringBuilder sb = new(base.ToString());

        sb.Append(" ID: " + ID);
        sb.Append(" Mesh: " + Mesh);
        sb.Append(" VisibilityBox: {" + VisibilityBox.ToString() + "} ");
        sb.Append(" CollisionBox: {" + CollisionBox.ToString() + "} ");
        sb.Append(" Flags: " + Flags.ToString("X4"));

        return sb.ToString();
    }
}
