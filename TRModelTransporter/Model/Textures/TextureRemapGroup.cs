using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Packing;

namespace TRModelTransporter.Model.Textures
{
    public class TextureRemapGroup
    {
        public List<TextureRemap> Remapping { get; set; }
        public List<TextureDependency> Dependencies { get; set; }

        public TextureRemapGroup()
        {
            Remapping = new List<TextureRemap>();
            Dependencies = new List<TextureDependency>();
        }

        public void CalculateDependencies(TR2Level level, TR2Entities entity)
        {
            using (TexturePacker packer = new TexturePacker(level))
            {
                Dictionary<TexturedTile, List<TexturedTileSegment>> entitySegments = packer.GetModelSegments(entity);
                foreach (TRModel model in level.Models)
                {
                    TR2Entities otherEntity = (TR2Entities)model.ID;
                    if (entity == otherEntity)
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
                                TextureDependency dependency = GetDependency(tile.Index, matchedSegment.Bounds);
                                if (dependency == null)
                                {
                                    dependency = new TextureDependency
                                    {
                                        TileIndex = tile.Index,
                                        Bounds = matchedSegment.Bounds
                                    };
                                    Dependencies.Add(dependency);
                                }
                                dependency.AddEntity(entity);
                                dependency.AddEntity(otherEntity);
                                System.Diagnostics.Debug.WriteLine("\t\t" + otherEntity + ", " + tile.Index + ": " + matchedSegment.Bounds);
                            }
                        }
                    }
                }
            }
        }

        public TextureDependency GetDependency(int tileIndex, Rectangle rectangle)
        {
            foreach (TextureDependency dependency in Dependencies)
            {
                if (dependency.TileIndex == tileIndex && dependency.Bounds == rectangle)
                {
                    return dependency;
                }
            }
            return null;
        }

        public TextureDependency GetDependency(int tileIndex, Rectangle rectangle, IEnumerable<TR2Entities> entities)
        {
            foreach (TextureDependency dependency in Dependencies)
            {
                if (dependency.TileIndex == tileIndex && dependency.Bounds == rectangle && dependency.Entities.All(e => entities.Contains(e)))
                {
                    return dependency;
                }
            }
            return null;
        }

        public bool CanRemoveRectangle(int tileIndex, Rectangle rectangle, IEnumerable<TR2Entities> entities)
        {
            // Is there a dependency for the given rectangle?
            TextureDependency dependency = GetDependency(tileIndex, rectangle);
            if (dependency != null)
            {
                // The rectangle can be removed if all of the entities match the dependency
                return dependency.Entities.All(e => entities.Contains(e));
            }
            return true;
        }
    }
}