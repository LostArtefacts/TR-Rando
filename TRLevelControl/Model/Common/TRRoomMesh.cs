﻿namespace TRLevelControl.Model;

public class TRRoomMesh<T, V> : ICloneable
    where T : Enum
    where V : TRRoomVertex
{
    public List<V> Vertices { get; set; } = new();
    public List<TRFace> Rectangles { get; set; } = new();
    public List<TRFace> Triangles { get; set; } = new();
    public List<TRRoomSprite<T>> Sprites { get; set; } = new();

    public IEnumerable<TRFace> Faces => Rectangles.Concat(Triangles);

    public TRRoomMesh<T, V> Clone()
    {
        return new()
        {
            Vertices = new(Vertices.Select(v => (V)v.Clone())),
            Rectangles = new(Rectangles.Select(r => r.Clone())),
            Triangles = new(Triangles.Select(t => t.Clone())),
            Sprites = new(Sprites.Select(s => s.Clone())),
        };
    }

    object ICloneable.Clone()
        => Clone();
}
