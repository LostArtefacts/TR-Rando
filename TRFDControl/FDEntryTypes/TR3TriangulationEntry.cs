using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRFDControl.FDEntryTypes
{
    public class TR3TriangulationEntry : FDEntry
    {
        public FDTriangulationData TriData { get; set; }

        public bool IsFloorTriangulation
        {
            get
            {
                FDFunctions function = (FDFunctions)Setup.Function;
                return function == FDFunctions.FloorTriangulationNESW_NW
                    || function == FDFunctions.FloorTriangulationNESW_Solid
                    || function == FDFunctions.FloorTriangulationNESW_SE
                    || function == FDFunctions.FloorTriangulationNWSE_NE
                    || function == FDFunctions.FloorTriangulationNWSE_Solid
                    || function == FDFunctions.FloorTriangulationNWSE_SW;
            }
        }

        public bool IsFloorPortal
        {
            get
            {
                FDFunctions function = (FDFunctions)Setup.Function;
                return function == FDFunctions.FloorTriangulationNESW_NW
                    || function == FDFunctions.FloorTriangulationNESW_SE
                    || function == FDFunctions.FloorTriangulationNWSE_NE
                    || function == FDFunctions.FloorTriangulationNWSE_SW;
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
}
