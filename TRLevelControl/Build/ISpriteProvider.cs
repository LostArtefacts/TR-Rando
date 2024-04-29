namespace TRLevelControl.Build;

public interface ISpriteProvider<T>
    where T : Enum
{
    public short GetSpriteOffset(T type);
    public T FindSpriteType(short textureOffset);
}
