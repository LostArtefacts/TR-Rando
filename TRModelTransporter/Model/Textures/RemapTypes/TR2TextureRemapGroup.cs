using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Packing;

namespace TRModelTransporter.Model.Textures
{
    public class TR2TextureRemapGroup : AbstractTextureRemapGroup<TR2Entities, TR2Level>
    {
        public override void CalculateDependencies(TR2Level level, TR2Entities entity)
        {
            using (TR2TexturePacker packer = new TR2TexturePacker(level))
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
                                TextureDependency<TR2Entities> dependency = GetDependency(tile.Index, matchedSegment.Bounds);
                                if (dependency == null)
                                {
                                    dependency = new TextureDependency<TR2Entities>
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
    }
}