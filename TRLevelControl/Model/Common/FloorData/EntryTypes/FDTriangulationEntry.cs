namespace TRLevelControl.Model;

public class FDTriangulationEntry : FDEntry
{
    public FDTriangulationData TriData { get; set; }

    public bool IsFloorTriangulation
    {
        get
        {
            FDFunction function = (FDFunction)Setup.Function;
            return function == FDFunction.FloorTriangulationNESW_NW
                || function == FDFunction.FloorTriangulationNESW_Solid
                || function == FDFunction.FloorTriangulationNESW_SE
                || function == FDFunction.FloorTriangulationNWSE_NE
                || function == FDFunction.FloorTriangulationNWSE_Solid
                || function == FDFunction.FloorTriangulationNWSE_SW;
        }
    }

    public bool IsFloorPortal
    {
        get
        {
            FDFunction function = (FDFunction)Setup.Function;
            return function == FDFunction.FloorTriangulationNESW_NW
                || function == FDFunction.FloorTriangulationNESW_SE
                || function == FDFunction.FloorTriangulationNWSE_NE
                || function == FDFunction.FloorTriangulationNWSE_SW;
        }
    }

    public override ushort[] Flatten()
    {
        return new ushort[]
        {
            Setup.Value,
            TriData.Value
        };
    }
}
