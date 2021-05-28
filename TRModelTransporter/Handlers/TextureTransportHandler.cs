using RectanglePacker.Organisation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Helpers;
using TRModelTransporter.Model;
using TRModelTransporter.Model.Textures;
using TRModelTransporter.Packing;
using TRModelTransporter.Utilities;
using TRTexture16Importer.Helpers;

namespace TRModelTransporter.Handlers
{
    public class TextureTransportHandler : AbstractTransportHandler
    {
        private const int _defaultBitmapWidth = 320;
        private const int _defaultBitmapHeight = 320;
        private const int _maxTextures = 2048;

        #region Export Properties
        public bool ExportIndividualSegments { get; set; }
        public string SegmentsFolder { get; set; }
        public ITextureClassifier TextureClassifier { get; set; }
        #endregion

        #region Import Properties
        private Dictionary<TRModelDefinition, List<TexturedTileSegment>> _importSegments;

        public IEnumerable<TRModelDefinition> Definitions { get; set; }
        public IEnumerable<TR2Entities> EntitiesToRemove { get; set; }
        public bool ClearUnusedSprites { get; set; }
        public TextureRemapGroup TextureRemap { get; set; }

        public ITexturePositionMonitor PositionMonitor { get; set; }
        #endregion

        private static readonly Dictionary<TR2Entities, List<TR2Entities>> _entitySpriteDependencies = new Dictionary<TR2Entities, List<TR2Entities>>
        {
            [TR2Entities.FlamethrowerGoon] = new List<TR2Entities> { TR2Entities.Flame_S_H },
            [TR2Entities.RedSnowmobile] = new List<TR2Entities> { TR2Entities.SnowmobileWake_S_H },
            [TR2Entities.XianGuardSword] = new List<TR2Entities> { TR2Entities.XianGuardSparkles_S_H }
        };

        #region Export Methods
        public override void Export()
        {
            List<TexturedTileSegment> allSegments = new List<TexturedTileSegment>();

            using (TexturePacker levelPacker = new TexturePacker(Level, TextureClassifier))
            {
                Dictionary<TexturedTile, List<TexturedTileSegment>> textureSegments = levelPacker.GetModelSegments(Definition.Entity);

                TRTextureDeduplicator deduplicator = new TRTextureDeduplicator
                {
                    SegmentMap = textureSegments,
                    UpdateGraphics = false
                };
                deduplicator.Deduplicate();

                Definition.ObjectTextures = new Dictionary<int, List<IndexedTRObjectTexture>>();
                Definition.SpriteSequences = new Dictionary<TR2Entities, TRSpriteSequence>();
                Definition.SpriteTextures = new Dictionary<TR2Entities, Dictionary<int, List<IndexedTRSpriteTexture>>>();

                int bitmapIndex = 0;
                foreach (List<TexturedTileSegment> segments in textureSegments.Values)
                {
                    for (int i = 0; i < segments.Count; i++)
                    {
                        TexturedTileSegment segment = segments[i];
                        if (!deduplicator.ShouldIgnoreSegment(Definition.Entity, segment))
                        {
                            allSegments.Add(segment);
                            Definition.ObjectTextures[bitmapIndex++] = new List<IndexedTRObjectTexture>(segment.Textures.Cast<IndexedTRObjectTexture>().ToArray());
                        }
                    }
                }

                if (_entitySpriteDependencies.ContainsKey(Definition.Entity))
                {
                    foreach (TR2Entities spriteEntity in _entitySpriteDependencies[Definition.Entity])
                    {
                        TRSpriteSequence sequence = Level.SpriteSequences.ToList().Find(s => s.SpriteID == (int)spriteEntity);
                        if (sequence != null)
                        {
                            Definition.SpriteSequences[spriteEntity] = sequence;
                        }

                        Dictionary<TexturedTile, List<TexturedTileSegment>> spriteSegments = levelPacker.GetSpriteSegments(spriteEntity);
                        Definition.SpriteTextures[spriteEntity] = new Dictionary<int, List<IndexedTRSpriteTexture>>();
                        foreach (List<TexturedTileSegment> segments in spriteSegments.Values)
                        {
                            for (int i = 0; i < segments.Count; i++)
                            {
                                TexturedTileSegment segment = segments[i];
                                allSegments.Add(segment);
                                Definition.SpriteTextures[spriteEntity][bitmapIndex++] = new List<IndexedTRSpriteTexture>(segment.Textures.Cast<IndexedTRSpriteTexture>().ToArray());
                            }
                        }
                    }
                }

                if (allSegments.Count > 0)
                {
                    using (TexturePacker segmentPacker = new TexturePacker())
                    {
                        segmentPacker.AddRectangles(allSegments);

                        segmentPacker.Options = new PackingOptions
                        {
                            FillMode = PackingFillMode.Horizontal,
                            OrderMode = PackingOrderMode.Area,
                            Order = PackingOrder.Descending,
                            GroupMode = PackingGroupMode.Squares
                        };
                        segmentPacker.TileWidth = _defaultBitmapWidth;
                        segmentPacker.TileHeight = _defaultBitmapHeight;
                        segmentPacker.MaximumTiles = 1;

                        segmentPacker.Pack();

                        if (segmentPacker.OrphanedRectangles.Count > 0)
                        {
                            throw new PackingException(string.Format("Failed to export textures for {0}.", Definition.Entity));
                        }

                        Definition.ObjectTextureCost = segmentPacker.TotalUsedSpace;

                        TexturedTile tile = segmentPacker.Tiles[0];
                        List<Rectangle> rects = new List<Rectangle>();
                        foreach (TexturedTileSegment segment in allSegments)
                        {
                            rects.Add(segment.MappedBounds);
                        }

                        Definition.TextureSegments = rects.ToArray();

                        Rectangle region = tile.GetOccupiedRegion();
                        Definition.Bitmap = tile.BitmapGraphics.Extract(region);

                        if (ExportIndividualSegments)
                        {
                            string dir = Path.Combine(SegmentsFolder, Definition.Alias.ToString());
                            if (Directory.Exists(dir))
                            {
                                Directory.Delete(dir, true);
                            }
                            Directory.CreateDirectory(dir);

                            foreach (TexturedTileSegment segment in allSegments)
                            {
                                segment.Bitmap.Save(Path.Combine(dir, segment.FirstTextureIndex + ".png"), ImageFormat.Png);
                            }
                        }
                    }
                }
                else
                {
                    Definition.ObjectTextureCost = 0;
                }
            }
        }

        #endregion

        #region Import Methods
        public override void Import()
        {
            if (Definitions == null)
            {
                throw new Exception();
            }

            // Pull together all of the texture segments for each of the definitions
            CollateSegments();

            // Pack the textures into the level
            Pack();

            // Update the level with any new ObjectTextures and update meshes accordingly
            MergeObjectTextures();

            // Update the level with any new SpriteTextures and SpriteSequences
            MergeSpriteTextures();

            // Inform the texture position monitor of the new location of tracked textures
            NotifyTextureWatcher();
        }

        private void CollateSegments()
        {
            // Rebuild the segment list. We assume the list of IndexedTRObjectTextures has been
            // ordered by area descending to preserve the "master" texture for each segment.
            _importSegments = new Dictionary<TRModelDefinition, List<TexturedTileSegment>>();
            foreach (TRModelDefinition definition in Definitions)
            {
                if (!definition.HasGraphics || definition.IsDependencyOnly)
                {
                    continue;
                }

                List<TRSpriteSequence> spriteSequences = Level.SpriteSequences.ToList();
                _importSegments[definition] = new List<TexturedTileSegment>();
                using (BitmapGraphics bg = new BitmapGraphics(definition.Bitmap))
                {
                    foreach (int segmentIndex in definition.ObjectTextures.Keys)
                    {
                        Bitmap segmentClip = bg.Extract(definition.TextureSegments[segmentIndex]);
                        TexturedTileSegment segment = null;
                        foreach (IndexedTRObjectTexture texture in definition.ObjectTextures[segmentIndex])
                        {
                            if (segment == null)
                            {
                                _importSegments[definition].Add(segment = new TexturedTileSegment(texture, segmentClip));
                            }
                            else
                            {
                                segment.AddTexture(texture);
                            }
                        }
                    }

                    List<TR2Entities> spriteEntities = new List<TR2Entities>(definition.SpriteSequences.Keys);
                    foreach (TR2Entities spriteEntity in spriteEntities)
                    {
                        TRSpriteSequence existingSequence = spriteSequences.Find(s => s.SpriteID == (int)spriteEntity);
                        if (existingSequence != null)
                        {
                            definition.SpriteSequences.Remove(spriteEntity);
                            continue;
                        }

                        // The sequence will be merged later when we know the sprite texture offsets.
                        // For now, add the segments we need for packing.
                        Dictionary<int, List<IndexedTRSpriteTexture>> spriteTextures = definition.SpriteTextures[spriteEntity];
                        foreach (int segmentIndex in spriteTextures.Keys)
                        {
                            Bitmap segmentClip = bg.Extract(definition.TextureSegments[segmentIndex]);
                            TexturedTileSegment segment = null;
                            foreach (IndexedTRSpriteTexture texture in spriteTextures[segmentIndex])
                            {
                                if (segment == null)
                                {
                                    _importSegments[definition].Add(segment = new TexturedTileSegment(texture, segmentClip));
                                }
                                else
                                {
                                    segment.AddTexture(texture);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void Pack()
        {
            using (TexturePacker packer = new TexturePacker(Level))
            {
                List<TR2Entities> removals = new List<TR2Entities> { TR2Entities.Map_M_U };
                if (EntitiesToRemove != null)
                {
                    removals.AddRange(EntitiesToRemove);
                }
                packer.RemoveModelSegments(removals, TextureRemap);

                ApplyFlamePatch();

                if (ClearUnusedSprites)
                {
                    RemoveUnusedSprites(packer);
                }

                List<TexturedTileSegment> allSegments = new List<TexturedTileSegment>();
                foreach (List<TexturedTileSegment> segmentList in _importSegments.Values)
                {
                    // We only add unique segments, so if another segment already exists, 
                    // remap the definition's segment to that one. Example of when this is
                    // needed is importing the dragon as DragonBack duplicates a lot of
                    // DragonFront, so this will greatly reduce the import cost.
                    for (int i = 0; i < segmentList.Count; i++)
                    {
                        TexturedTileSegment segment = segmentList[i];
                        int j = FindMatchingSegment(allSegments, segment);
                        if (j == -1)
                        {
                            allSegments.Add(segment);
                        }
                        else
                        {
                            TexturedTileSegment otherSegment = allSegments[j];
                            segmentList[i] = allSegments[j];
                            foreach (IndexedTRObjectTexture texture in segment.Textures)
                            {
                                if (!otherSegment.IsObjectTextureFor(texture.Index))
                                {
                                    otherSegment.AddTexture(texture);
                                }
                            }
                        }
                    }
                }

                packer.AddRectangles(allSegments);
                packer.Pack(true);

                if (packer.OrphanedRectangles.Count > 0)
                {
                    List<string> entityNames = new List<string>();
                    foreach (TRModelDefinition def in Definitions)
                    {
                        entityNames.Add(def.Entity.ToString());
                    }
                    throw new PackingException(string.Format
                    (
                        "Failed to pack {0} rectangles for model types [{1}].",
                        packer.OrphanedRectangles.Count,
                        string.Join(", ", entityNames)
                    ));
                }
            }
        }

        private int FindMatchingSegment(List<TexturedTileSegment> segments, TexturedTileSegment segment)
        {
            for (int i = 0; i < segments.Count; i++)
            {
                TexturedTileSegment otherSegment = segments[i];
                if
                (
                    otherSegment.FirstTextureIndex == segment.FirstTextureIndex &&
                    otherSegment.FirstClassification == segment.FirstClassification
                )
                {
                    return i;
                }
            }
            return -1;
        }

        private void ApplyFlamePatch()
        {
            // TextureDeduplicator will have removed the extra flame blasts present in DA and Lair (Flamethrower and Dragon).
            // We need to ensure that if these models are present in any level, that the sprite sequences for the blasts point
            // to the same as the grenade blast instead.

            List<TR2Entities> flameEnemies = new List<TR2Entities>
            {
                TR2Entities.FlamethrowerGoon, TR2Entities.DragonExplosionEmitter_N
            };

            if
            (
                Definitions.ToList().FindIndex(d => flameEnemies.Contains(d.Entity)) != -1 ||
                Level.Models.ToList().FindIndex(m => flameEnemies.Contains((TR2Entities)m.ID)) != -1
            )
            {
                List<TRSpriteSequence> sequences = Level.SpriteSequences.ToList();
                int blastSequence = sequences.FindIndex(s => s.SpriteID == (int)TR2Entities.FireBlast_S_H);
                int grenadeSequence = sequences.FindIndex(s => s.SpriteID == (int)TR2Entities.Explosion_S_H);

                if (grenadeSequence != -1)
                {
                    if (blastSequence == -1)
                    {
                        TRSpriteSequence grenadeBlast = sequences[grenadeSequence];
                        sequences.Add(new TRSpriteSequence
                        {
                            SpriteID = (int)TR2Entities.FireBlast_S_H,
                            NegativeLength = grenadeBlast.NegativeLength,
                            Offset = grenadeBlast.Offset
                        });

                        Level.SpriteSequences = sequences.ToArray();
                        Level.NumSpriteSequences++;
                    }
                    else
                    {
                        Level.SpriteSequences[blastSequence].Offset = sequences[grenadeSequence].Offset;
                    }
                }
            }
        }

        private void RemoveUnusedSprites(TexturePacker packer)
        {
            // TODO: We could potentially work out if guns/ammo other
            // than pistols can be removed - would need to get secret
            // rewards and convert to entities as script uses different
            // IDs.
            List<TR2Entities> unusedItems = new List<TR2Entities>
            {
                TR2Entities.PistolAmmo_S_P,
                TR2Entities.Map_M_U,
                TR2Entities.GrayDisk_S_H
            };

            ISet<TR2Entities> allEntities = new HashSet<TR2Entities>();
            for (int i = 0; i < Level.Entities.Length; i++)
            {
                allEntities.Add((TR2Entities)Level.Entities[i].TypeID);
            }

            for (int i = unusedItems.Count - 1; i >= 0; i--)
            {
                if (unusedItems[i] != TR2Entities.GrayDisk_S_H && allEntities.Contains(unusedItems[i]))
                {
                    unusedItems.RemoveAt(i);
                }
            }

            packer.RemoveSpriteSegments(unusedItems);
        }

        private void MergeObjectTextures()
        {
            // Add each ObjectTexture to the level and store a map of old index to new index.
            // Make use of any invalid texture first because we are limited to 2048 entries
            List<TRObjectTexture> levelObjectTextures = Level.ObjectTextures.ToList();
            Queue<int> reusableIndices = new Queue<int>(Level.GetInvalidObjectTextureIndices());

            Dictionary<TRModelDefinition, Dictionary<int, int>> indexMap = new Dictionary<TRModelDefinition, Dictionary<int, int>>();
            foreach (TRModelDefinition definition in Definitions)
            {
                if (!_importSegments.ContainsKey(definition))
                {
                    continue;
                }

                indexMap[definition] = new Dictionary<int, int>();
                foreach (TexturedTileSegment segment in _importSegments[definition])
                {
                    foreach (AbstractIndexedTRTexture texture in segment.Textures)
                    {
                        if (!(texture is IndexedTRObjectTexture objTexture)) // Sprites handled later
                        {
                            continue;
                        }

                        int newIndex;
                        if (reusableIndices.Count > 0)
                        {
                            newIndex = reusableIndices.Dequeue();
                            levelObjectTextures[newIndex] = objTexture.Texture;
                        }
                        else if (levelObjectTextures.Count < _maxTextures)
                        {
                            levelObjectTextures.Add(objTexture.Texture);
                            newIndex = levelObjectTextures.Count - 1;
                        }
                        else
                        {
                            throw new PackingException(string.Format("Limit of {0} textures reached.", _maxTextures));
                        }

                        indexMap[definition][texture.Index] = newIndex;
                    }
                }
            }

            // Save the new textures in the level
            Level.ObjectTextures = levelObjectTextures.ToArray();
            Level.NumObjectTextures = (uint)levelObjectTextures.Count;

            // Change the definition's meshes so that the textured rectangles and triangles point
            // to the correct object texture.
            foreach (TRModelDefinition definition in indexMap.Keys)
            {
                foreach (TRMesh mesh in definition.Meshes)
                {
                    foreach (TRFace4 rect in mesh.TexturedRectangles)
                    {
                        rect.Texture = ConvertTextureReference(rect.Texture, indexMap[definition]);
                    }
                    foreach (TRFace3 tri in mesh.TexturedTriangles)
                    {
                        tri.Texture = ConvertTextureReference(tri.Texture, indexMap[definition]);
                    }
                }
            }
        }

        private ushort ConvertTextureReference(ushort textureReference, Dictionary<int, int> indexMap)
        {
            if (indexMap.ContainsKey(textureReference))
            {
                return (ushort)indexMap[textureReference];
            }
            return 0;
        }

        private void MergeSpriteTextures()
        {
            List<TRSpriteTexture> levelSpriteTextures = Level.SpriteTextures.ToList();
            List<TRSpriteSequence> levelSpriteSequences = Level.SpriteSequences.ToList();

            foreach (TRModelDefinition definition in Definitions)
            {
                if (!_importSegments.ContainsKey(definition) || definition.SpriteSequences.Count == 0)
                {
                    continue;
                }

                foreach (TR2Entities spriteEntity in definition.SpriteSequences.Keys)
                {
                    TRSpriteSequence sequence = definition.SpriteSequences[spriteEntity];
                    sequence.Offset = -1;
                    levelSpriteSequences.Add(sequence);

                    foreach (int bitmapIndex in definition.SpriteTextures[spriteEntity].Keys)
                    {
                        List<IndexedTRSpriteTexture> textures = definition.SpriteTextures[spriteEntity][bitmapIndex];

                        for (int i = 0; i < textures.Count; i++)
                        {
                            if (sequence.Offset == -1)
                            {
                                // mark the position of the first sprite only
                                sequence.Offset = (short)levelSpriteTextures.Count;
                            }
                            levelSpriteTextures.Add(textures[i].Texture);
                        }
                    }
                }
            }

            Level.SpriteTextures = levelSpriteTextures.ToArray();
            Level.NumSpriteTextures = (uint)levelSpriteTextures.Count;

            Level.SpriteSequences = levelSpriteSequences.ToArray();
            Level.NumSpriteSequences = (uint)levelSpriteSequences.Count;
        }

        public void ResetUnusedTextures()
        {
            // Patch - this doesn't break the game, but it prevents the level being
            // opened in trview. Some textures will now be unused, but rather than
            // removing them and having to reindex everything that points to the
            // the object textures, we'll just reset them to atlas 0, and set all
            // coordinates to 0.

            Level.ResetUnusedTextures();
        }

        private void NotifyTextureWatcher()
        {
            if (PositionMonitor == null)
            {
                return;
            }

            // Notify if anything has been removed first
            if (EntitiesToRemove != null)
            {
                PositionMonitor.EntityTexturesRemoved(new List<TR2Entities>(EntitiesToRemove));
            }

            // If the monitor isn't interested in any new entities, skip the actual processing
            Dictionary<TR2Entities, List<int>> watchedTextures = PositionMonitor.GetMonitoredTextureIndices();
            if (watchedTextures.Count == 0)
            {
                return;
            }

            Dictionary<TR2Entities, List<PositionedTexture>> textureResults = new Dictionary<TR2Entities, List<PositionedTexture>>();

            foreach (TRModelDefinition definition in _importSegments.Keys)
            {
                // Does this definition have any entities we are interested in?
                List<TR2Entities> entities = new List<TR2Entities>();
                if (watchedTextures.ContainsKey(definition.Alias))
                {
                    entities.Add(definition.Alias);
                }

                foreach (TR2Entities spriteEntity in definition.SpriteSequences.Keys)
                {
                    if (watchedTextures.ContainsKey(spriteEntity))
                    {
                        entities.Add(spriteEntity);
                    }
                }

                if (entities.Count == 0)
                {
                    continue;
                }

                foreach (TR2Entities entity in entities)
                {
                    foreach (int watchedIndex in watchedTextures[entity])
                    {
                        foreach (TexturedTileSegment segment in _importSegments[definition])
                        {
                            AbstractIndexedTRTexture texture = segment.GetTexture(watchedIndex);
                            if (texture == null)
                            {
                                continue;
                            }

                            if (!textureResults.ContainsKey(entity))
                            {
                                textureResults[entity] = new List<PositionedTexture>();
                            }
                            textureResults[entity].Add(new PositionedTexture(texture));
                            break;
                        }
                    }
                }
            }

            PositionMonitor.MonitoredTexturesPositioned(textureResults);
        }
        #endregion
    }
}