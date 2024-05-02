namespace TRLevelControl.Model;

public class FDTriangulationEntry : FDEntry
{
    private static readonly List<FDTriangulationType> _floorTriTypes = new()
    {
        FDTriangulationType.FloorNWSE_Solid,
        FDTriangulationType.FloorNESW_Solid,
    };

    private static readonly List<FDTriangulationType> _floorPortalTypes = new()
    {
        FDTriangulationType.FloorNWSE_SW,
        FDTriangulationType.FloorNWSE_NE,
        FDTriangulationType.FloorNESW_SE,
        FDTriangulationType.FloorNESW_NW,
    };

    private static readonly List<FDTriangulationType> _ceilingTriTypes = new()
    {
        FDTriangulationType.CeilingNWSE_Solid,
        FDTriangulationType.CeilingNESW_Solid,
    };

    private static readonly List<FDTriangulationType> _ceilingPortalTypes = new()
    {
        FDTriangulationType.CeilingNWSE_SW,
        FDTriangulationType.CeilingNWSE_NE,
        FDTriangulationType.CeilingNESW_NW,
        FDTriangulationType.CeilingNESW_SE,
    };

    public FDTriangulationType Type { get; set; }
    public byte C10 { get; set; }
    public byte C00 { get; set; }
    public byte C01 { get; set; }
    public byte C11 { get; set; }
    public sbyte H1 { get; set; }
    public sbyte H2 { get; set; }

    public bool IsFloorTriangulation
        => _floorTriTypes.Contains(Type);

    public bool IsFloorPortal
        => _floorPortalTypes.Contains(Type);

    public bool IsCeilingTriangulation
        => _ceilingTriTypes.Contains(Type);

    public bool IsCeilingPortal
        => _ceilingPortalTypes.Contains(Type);

    public override FDFunction GetFunction()
        => (FDFunction)Type;

    public override FDEntry Clone()
        => (FDTriangulationEntry)MemberwiseClone();
}
