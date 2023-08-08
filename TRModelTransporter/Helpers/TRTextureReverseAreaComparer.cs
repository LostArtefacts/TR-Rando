using System.Collections.Generic;
using TRModelTransporter.Model.Textures;

namespace TRModelTransporter.Helpers;

public class TRTextureReverseAreaComparer : IComparer<AbstractIndexedTRTexture>
{
    public int Compare(AbstractIndexedTRTexture t1, AbstractIndexedTRTexture t2)
    {
        return t2.Area.CompareTo(t1.Area);
    }
}
