using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMAddFaceFunction : BaseEMFunction, ITextureModifier
{
    public Dictionary<short, List<TRFace4>> Quads { get; set; }
    public Dictionary<short, List<TRFace3>> Triangles { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);

        if (Quads != null)
        {
            foreach (short roomIndex in Quads.Keys)
            {
                TR1Room room = level.Rooms[data.ConvertRoom(roomIndex)];
                room.Mesh.Rectangles.AddRange(Quads[roomIndex]);
            }
        }

        if (Triangles != null)
        {
            foreach (short roomIndex in Triangles.Keys)
            {
                TR1Room room = level.Rooms[data.ConvertRoom(roomIndex)];
                room.Mesh.Triangles.AddRange(Triangles[roomIndex]);
            }
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);

        if (Quads != null)
        {
            foreach (short roomIndex in Quads.Keys)
            {
                TR2Room room = level.Rooms[data.ConvertRoom(roomIndex)];
                room.Mesh.Rectangles.AddRange(Quads[roomIndex]);
            }
        }

        if (Triangles != null)
        {
            foreach (short roomIndex in Triangles.Keys)
            {
                TR2Room room = level.Rooms[data.ConvertRoom(roomIndex)];
                room.Mesh.Triangles.AddRange(Triangles[roomIndex]);
            }
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);

        if (Quads != null)
        {
            foreach (short roomIndex in Quads.Keys)
            {
                TR3Room room = level.Rooms[data.ConvertRoom(roomIndex)];
                room.Mesh.Rectangles.AddRange(Quads[roomIndex]);
            }
        }

        if (Triangles != null)
        {
            foreach (short roomIndex in Triangles.Keys)
            {
                TR3Room room = level.Rooms[data.ConvertRoom(roomIndex)];
                room.Mesh.Triangles.AddRange(Triangles[roomIndex]);
            }
        }
    }

    public void RemapTextures(Dictionary<ushort, ushort> indexMap)
    {
        if (Quads != null)
        {
            foreach (List<TRFace4> faces in Quads?.Values)
            {
                foreach (TRFace4 face in faces)
                {
                    if (indexMap.ContainsKey(face.Texture))
                    {
                        face.Texture = indexMap[face.Texture];
                    }
                }
            }
        }

        if (Triangles != null)
        {
            foreach (List<TRFace3> faces in Triangles?.Values)
            {
                foreach (TRFace3 face in faces)
                {
                    if (indexMap.ContainsKey(face.Texture))
                    {
                        face.Texture = indexMap[face.Texture];
                    }
                }
            }
        }
    }
}
