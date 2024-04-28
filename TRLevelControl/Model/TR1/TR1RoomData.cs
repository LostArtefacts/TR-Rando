using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR1RoomData : ISerializableCompact
{
    public short NumVertices { get; set; }

    public TR1RoomVertex[] Vertices { get; set; }

    public short NumRectangles { get; set; }

    public TRFace4[] Rectangles { get; set; }

    public short NumTriangles { get; set; }

    public TRFace3[] Triangles { get; set; }

    public short NumSprites { get; set; }

    public TRRoomSprite[] Sprites { get; set; }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write(NumVertices);

            foreach (TR1RoomVertex vert in Vertices)
            {
                writer.Write(vert.Serialize());
            }

            writer.Write(NumRectangles);

            foreach (TRFace4 face in Rectangles)
            {
                writer.Write(face.Serialize());
            }

            writer.Write(NumTriangles);

            foreach (TRFace3 face in Triangles)
            {
                writer.Write(face.Serialize());
            }

            writer.Write(NumSprites);

            foreach (TRRoomSprite sprite in Sprites)
            {
                writer.Write(sprite.Serialize());
            }
        }

        return stream.ToArray();
    }
}
