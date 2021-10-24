using System;
using System.Collections.Generic;
using System.Drawing;

namespace TRModelTransporter.Model.Textures
{
    public class TextureDependency<E> where E : Enum
    {
        public List<E> Entities { get; set; }
        public int TileIndex { get; set; }
        public Rectangle Bounds { get; set; }

        public TextureDependency()
        {
            Entities = new List<E>();
        }

        public void AddEntity(E entity)
        {
            if (!Entities.Contains(entity))
            {
                Entities.Add(entity);
            }
        }
    }
}