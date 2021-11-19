using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TRModelTransporter.Packing;

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

        public void CalculateDependencies(L level, E entity)
        {
            using (AbstractTexturePacker<E, L> packer = CreatePacker(level))
            {
                Dictionary<TexturedTile, List<TexturedTileSegment>> entitySegments = packer.GetModelSegments(entity);
                foreach (E otherEntity in GetModelTypes(level))
                {
                    if (EqualityComparer<E>.Default.Equals(entity, otherEntity))
                    {
                        continue;
                    }

                    Dictionary<TexturedTile, List<TexturedTileSegment>> modelSegments = packer.GetModelSegments(otherEntity);

                    foreach (TexturedTile tile in entitySegments.Keys)
                    {
                        if (modelSegments.ContainsKey(tile))
                        {
                            List<TexturedTileSegment> matches = entitySegments[tile].FindAll(s1 => modelSegments[tile].Any(s2 => s1 == s2));
                            foreach (TexturedTileSegment matchedSegment in matches)
                            {
                                TextureDependency<E> dependency = GetDependency(tile.Index, matchedSegment.Bounds);
                                if (dependency == null)
                                {
                                    dependency = new TextureDependency<E>
                                    {
                                        TileIndex = tile.Index,
                                        Bounds = matchedSegment.Bounds
                                    };
                                    Dependencies.Add(dependency);
                                }
                                dependency.AddEntity(entity);
                                dependency.AddEntity(otherEntity);
                            }
                        }
                    }
                }
            }
        }

        protected abstract AbstractTexturePacker<E, L> CreatePacker(L level);
        protected abstract IEnumerable<E> GetModelTypes(L level);

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