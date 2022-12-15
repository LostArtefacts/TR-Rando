using System;
using System.Collections.Generic;

namespace TREnvironmentEditor.Helpers
{
    public class EMTextureGroup
    {
        public ushort Floor { get; set; }
        public ushort Ceiling { get; set; }
        public Direction WallAlignment { get; set; }
        // 64x64
        public ushort Wall4 { get; set; }
        // 64x48
        public ushort Wall3 { get; set; }
        // 64x32
        public ushort Wall2 { get; set; }
        // 64x16
        public ushort Wall1 { get; set;}

        public EMTextureGroup()
        {
            Wall4 = Wall3 = Wall2 = Wall1 = ushort.MaxValue;
            WallAlignment = Direction.Up;
        }

        public ushort GetWall(int height)
        {
            List<ushort> temp = new List<ushort> { Wall1, Wall2, Wall3, Wall4 };
            ushort result = ushort.MaxValue;
            int clicks = Math.Min(height / 256, 4);
            
            for (int i = 0; i < temp.Count; i++)
            {
                if (clicks == i + 1)
                {
                    if (temp[i] != ushort.MaxValue)
                    {
                        result = temp[i];
                        break;
                    }
                    else
                    {
                        clicks++;
                    }
                }
            }

            return result == ushort.MaxValue ? Floor : result;
        }
    }
}
