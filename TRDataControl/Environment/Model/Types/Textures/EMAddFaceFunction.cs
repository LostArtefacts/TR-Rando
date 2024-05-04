using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMAddFaceFunction : BaseEMFunction, ITextureModifier
{
    public Dictionary<short, List<TRFace>> Quads { get; set; }
    public Dictionary<short, List<TRFace>> Triangles { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        Apply(data, r => level.Rooms[r].Mesh);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        Apply(data, r => level.Rooms[r].Mesh);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        Apply(data, r => level.Rooms[r].Mesh);
    }

    private void Apply<T, V>(EMLevelData data, Func<int, TRRoomMesh<T, V>> meshAction)
        where T : Enum
        where V : TRRoomVertex
    {
        if (Quads != null)
        {
            Quads.Values.SelectMany(v => v).ToList()
                .ForEach(face => face.Type = TRFaceType.Rectangle);
            foreach (var (roomIndex, faces) in Quads)
            {
                TRRoomMesh<T, V> mesh = meshAction(data.ConvertRoom(roomIndex));
                mesh.Rectangles.AddRange(faces);
            }
        }

        if (Triangles != null)
        {
            Triangles.Values.SelectMany(v => v).ToList()
                .ForEach(face => face.Type = TRFaceType.Triangle);
            foreach (var (roomIndex, faces) in Triangles)
            {
                TRRoomMesh<T, V> mesh = meshAction(data.ConvertRoom(roomIndex));
                mesh.Triangles.AddRange(faces);
            }
        }
    }

    public void RemapTextures(Dictionary<ushort, ushort> indexMap)
    {
        Quads?.Values.SelectMany(q => q)
                .Where(q => indexMap.ContainsKey(q.Texture))
                .ToList()
                .ForEach(q => q.Texture = indexMap[q.Texture]);

        Triangles?.Values.SelectMany(t => t)
                .Where(t => indexMap.ContainsKey(t.Texture))
                .ToList()
                .ForEach(t => t.Texture = indexMap[t.Texture]);
    }
}
