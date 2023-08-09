using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR5RoomLayer : ISerializableCompact
{
    public uint NumLayerVertices { get; set; }

    public ushort UnknownL1 { get; set; }

    public ushort NumLayerRectangles { get; set; }

    public ushort NumLayerTriangles { get; set; }

    public ushort UnknownL2 { get; set; }

    public ushort Filler { get; set; }

    public ushort Filler2 { get; set; }

    public float LayerBoundingBoxX1 { get; set; }

    public float LayerBoundingBoxY1 { get; set; }

    public float LayerBoundingBoxZ1 { get; set; }

    public float LayerBoundingBoxX2 { get; set; }

    public float LayerBoundingBoxY2 { get; set; }

    public float LayerBoundingBoxZ2 { get; set; }

    public uint Filler3 { get; set; }

    public uint UnknownL6 { get; set; }

    public uint UnknownL7 { get; set; }

    public uint UnknownL8 { get; set; }

    public byte[] Serialize()
    {
        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(NumLayerVertices);
                writer.Write(UnknownL1);
                writer.Write(NumLayerRectangles);
                writer.Write(NumLayerTriangles);
                writer.Write(UnknownL2);
                writer.Write(Filler);
                writer.Write(Filler2);
                writer.Write(LayerBoundingBoxX1);
                writer.Write(LayerBoundingBoxY1);
                writer.Write(LayerBoundingBoxZ1);
                writer.Write(LayerBoundingBoxX2);
                writer.Write(LayerBoundingBoxY2);
                writer.Write(LayerBoundingBoxZ2);
                writer.Write(Filler3);
                writer.Write(UnknownL6);
                writer.Write(UnknownL7);
                writer.Write(UnknownL8);
            }

            return stream.ToArray();
        }
    }
}
