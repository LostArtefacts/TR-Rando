using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR1RoomData : ISerializableCompact
{
    public List<TR1RoomVertex> Vertices { get; set; }
    public List<TRFace4> Rectangles { get; set; }
    public List<TRFace3> Triangles { get; set; }
    public List<TRRoomSprite> Sprites { get; set; }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write((short)Vertices.Count);
            foreach (TR1RoomVertex vert in Vertices)
            {
                writer.Write(vert.Serialize());
            }

            writer.Write((short)Rectangles.Count);
            foreach (TRFace4 face in Rectangles)
            {
                writer.Write(face.Serialize());
            }

            writer.Write((short)Triangles.Count);
            foreach (TRFace3 face in Triangles)
            {
                writer.Write(face.Serialize());
            }

            writer.Write((short)Sprites.Count);
            foreach (TRRoomSprite sprite in Sprites)
            {
                writer.Write(sprite.Serialize());
            }
        }

        return stream.ToArray();
    }
}
