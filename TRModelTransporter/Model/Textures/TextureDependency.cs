using System.Collections.Generic;
using System.Drawing;
using TRLevelReader.Model.Enums;

namespace TRModelTransporter.Model.Textures
{
    public class TextureDependency
    {
        public List<TR2Entities> Entities { get; set; }
        public int TileIndex { get; set; }
        public Rectangle Bounds { get; set; }

        public TextureDependency()
        {
            Entities = new List<TR2Entities>();
        }

        public void AddEntity(TR2Entities entity)
        {
            if (!Entities.Contains(entity))
            {
                Entities.Add(entity);
            }
        }
    }
}