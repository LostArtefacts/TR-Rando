namespace TREnvironmentEditor.Helpers;

// Texture index => room index => rect/tri indices
public class EMTextureMap : Dictionary<ushort, EMGeometryMap>
{
    public void Remap(Dictionary<ushort, ushort> indexMap)
    {
        foreach (ushort currentIndex in indexMap.Keys)
        {
            if (ContainsKey(currentIndex))
            {
                EMGeometryMap temp = this[currentIndex];
                Remove(currentIndex);
                this[indexMap[currentIndex]] = temp;
            }
        }
    }
}

// Room index => rect/tri indices
public class EMGeometryMap : Dictionary<int, Dictionary<EMTextureFaceType, int[]>> { }

public enum EMTextureFaceType
{
    Rectangles,
    Triangles
}
