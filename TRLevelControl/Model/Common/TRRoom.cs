namespace TRLevelControl.Model;

public abstract class TRRoom
{
    public TRRoomInfo Info { get; set; }
    public List<TRRoomPortal> Portals { get; set; }
    public ushort NumZSectors { get; set; }
    public ushort NumXSectors { get; set; }
    public List<TRRoomSector> Sectors { get; set; }
    public short AlternateRoom { get; set; }
    public TRRoomFlag Flags { get; set; }

    public void SetFlag(TRRoomFlag flag, bool state)
    {
        if (state)
        {
            Flags |= flag;
        }
        else
        {
            Flags &= ~flag;
        }
    }

    public bool ContainsWater
    {
        get => Flags.HasFlag(TRRoomFlag.Water);
        set => SetFlag(TRRoomFlag.Water, value);
    }

    public bool IsSkyboxVisible
    {
        get => Flags.HasFlag(TRRoomFlag.Skybox);
        set => SetFlag(TRRoomFlag.Skybox, value);
    }

    public bool IsWindy
    {
        get => Flags.HasFlag(TRRoomFlag.Wind);
        set => SetFlag(TRRoomFlag.Wind, value);
    }

    public bool IsSwamp
    {
        get => Flags.HasFlag(TRRoomFlag.SwampOrNoLensflare);
        set => SetFlag(TRRoomFlag.SwampOrNoLensflare, value);
    }

    public TRRoomSector GetSector(int x, int z, TRUnit unit = TRUnit.World)
    {
        switch (unit)
        {
            case TRUnit.Room:
                x /= TRConsts.Step4;
                z /= TRConsts.Step4;
                break;
            case TRUnit.World:
                x = Math.Min(Math.Max(x, Info.X), Info.X + (NumXSectors - 1) * TRConsts.Step4);
                z = Math.Min(Math.Max(z, Info.Z), Info.Z + (NumZSectors - 1) * TRConsts.Step4);
                x = (x - Info.X) / TRConsts.Step4;
                z = (z - Info.Z) / TRConsts.Step4;
                break;
        }
        return Sectors[x * NumZSectors + z];
    }
}
