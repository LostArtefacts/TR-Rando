using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace TRModelTransporter.Model.Textures
{
    public abstract class AbstractTextureRemapGroup<E, L> 
        where E : Enum
        where L : class
    {
        public List<TextureRemap> Remapping { get; set; }
        public List<TextureDependency<E>> Dependencies { get; set; }

        public AbstractTextureRemapGroup()
        {
            Remapping = new List<TextureRemap>();
            Dependencies = new List<TextureDependency<E>>();
        }

        public abstract void CalculateDependencies(L level, E entity);

        public TextureDependency<E> GetDependency(int tileIndex, Rectangle rectangle)
        {
            foreach (TextureDependency<E> dependency in Dependencies)
            {
                if (dependency.TileIndex == tileIndex && dependency.Bounds == rectangle)
                {
                    return dependency;
                }
            }
            return null;
        }

        public TextureDependency<E> GetDependency(int tileIndex, Rectangle rectangle, IEnumerable<E> entities)
        {
            foreach (TextureDependency<E> dependency in Dependencies)
            {
                if (dependency.TileIndex == tileIndex && dependency.Bounds == rectangle && dependency.Entities.All(e => entities.Contains(e)))
                {
                    return dependency;
                }
            }
            return null;
        }

        public bool CanRemoveRectangle(int tileIndex, Rectangle rectangle, IEnumerable<E> entities)
        {
            // Is there a dependency for the given rectangle?
            TextureDependency<E> dependency = GetDependency(tileIndex, rectangle);
            if (dependency != null)
            {
                // The rectangle can be removed if all of the entities match the dependency
                return dependency.Entities.All(e => entities.Contains(e));
            }
            return true;
        }
    }
}