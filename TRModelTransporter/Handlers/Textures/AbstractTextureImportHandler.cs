using RectanglePacker.Events;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TRLevelReader.Model;
using TRModelTransporter.Data;
using TRModelTransporter.Model;
using TRModelTransporter.Model.Textures;
using TRModelTransporter.Packing;
using TRTexture16Importer.Helpers;

namespace TRModelTransporter.Handlers
{
    public abstract class AbstractTextureImportHandler<E, L, D>
        where E : Enum
        where L : class
        where D : AbstractTRModelDefinition<E>
    {
        public ITransportDataProvider<E> Data { get; set; }

        protected Dictionary<D, List<TexturedTileSegment>> _importSegments;

        protected L _level;
        protected IEnumerable<D> _definitions;
        protected IEnumerable<E> _entitiesToRemove;
        protected AbstractTextureRemapGroup<E, L> _textureRemap;
        protected bool _clearUnusedSprites;
        protected ITexturePositionMonitor<E> _positionMonitor;

        public void Import(L level, IEnumerable<D> definitions, IEnumerable<E> entitiesToRemove, AbstractTextureRemapGroup<E, L> textureRemap, bool clearUnusedSprites, ITexturePositionMonitor<E> positionMonitor)
        {
            _level = level;
            _definitions = definitions;
            _entitiesToRemove = entitiesToRemove;
            _textureRemap = textureRemap;
            _clearUnusedSprites = clearUnusedSprites;
            _positionMonitor = positionMonitor;

            // Pull together all of the texture segments for each of the definitions
            CollateSegments();

            // Pack the textures into the level, or bail if it wasn't possible
            PackingResult<TexturedTile, TexturedTileSegment> packingResult = Pack();
            if (packingResult.OrphanCount > 0)
            {
                List<string> entityNames = new List<string>();
                foreach (D def in _definitions)
                {
                    entityNames.Add(def.Entity.ToString());
                }
                throw new PackingException(string.Format
                (
                    "Failed to pack {0} rectangles for model types [{1}].",
                    packingResult.OrphanCount,
                    string.Join(", ", entityNames)
                ));
            }

            // Update the level with any new ObjectTextures and update meshes accordingly
            MergeObjectTextures();

            // Update the level with any new SpriteTextures and SpriteSequences
            MergeSpriteTextures();

            // Inform the texture position monitor of the new location of tracked textures
            NotifyTextureWatcher();
        }

        protected virtual void CollateSegments()
        {
            // Rebuild the segment list. We assume the list of IndexedTRObjectTextures has been
            // ordered by area descending to preserve the "master" texture for each segment.
            _importSegments = new Dictionary<D, List<TexturedTileSegment>>();

            // Track existing sprite sequences to avoid duplication
            List<TRSpriteSequence> spriteSequences = new List<TRSpriteSequence>(GetExistingSpriteSequences());
            foreach (D definition in _definitions)
            {
                if (!definition.HasGraphics || definition.IsDependencyOnly)
                {
                    continue;
                }

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

                    List<E> spriteEntities = new List<E>(definition.SpriteSequences.Keys);
                    foreach (E spriteEntity in spriteEntities)
                    {
                        TRSpriteSequence existingSequence = spriteSequences.Find(s => s.SpriteID == Convert.ToInt32(spriteEntity));
                        if (existingSequence != null)
                        {
                            definition.SpriteSequences.Remove(spriteEntity);
                            continue;
                        }
                        else
                        {
                            // Add it to the tracking list in case we are importing 2 or more models
                            // that share a sequence e.g. Dragon/Flamethrower and Flame_S_H
                            spriteSequences.Add(new TRSpriteSequence { SpriteID = Convert.ToInt32(spriteEntity) });
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

        protected virtual PackingResult<TexturedTile, TexturedTileSegment> Pack()
        {
            using (AbstractTexturePacker<E, L> packer = CreatePacker())
            {
                packer.MaximumTiles = Data.TextureTileLimit;

                ProcessRemovals(packer);

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

                return packer.Pack(true);
            }
        }

        protected virtual void MergeObjectTextures()
        {
            // Add each ObjectTexture to the level and store a map of old index to new index.
            // Make use of any invalid texture first because we are limited to 2048 entries
            List<TRObjectTexture> levelObjectTextures = GetExistingObjectTextures().ToList();
            Queue<int> reusableIndices = new Queue<int>(GetInvalidObjectTextureIndices());

            Dictionary<D, Dictionary<int, int>> indexMap = new Dictionary<D, Dictionary<int, int>>();
            foreach (D definition in _definitions)
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
                        else if (levelObjectTextures.Count < Data.TextureObjectLimit)
                        {
                            levelObjectTextures.Add(objTexture.Texture);
                            newIndex = levelObjectTextures.Count - 1;
                        }
                        else
                        {
                            throw new PackingException(string.Format("Limit of {0} textures reached.", Data.TextureObjectLimit));
                        }

                        indexMap[definition][texture.Index] = newIndex;
                    }
                }
            }

            // Save the new textures in the level
            WriteObjectTextures(levelObjectTextures);

            // Change the definition's meshes so that the textured rectangles and triangles point
            // to the correct object texture.
            RemapMeshTextures(indexMap);
        }

        protected virtual void MergeSpriteTextures()
        {
            List<TRSpriteTexture> levelSpriteTextures = GetExistingSpriteTextures().ToList();
            List<TRSpriteSequence> levelSpriteSequences = GetExistingSpriteSequences().ToList();

            foreach (D definition in _definitions)
            {
                if (!_importSegments.ContainsKey(definition) || definition.SpriteSequences.Count == 0)
                {
                    continue;
                }

                foreach (E spriteEntity in definition.SpriteSequences.Keys)
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

            WriteSpriteTextures(levelSpriteTextures);
            WriteSpriteSequences(levelSpriteSequences);
        }

        protected abstract IEnumerable<TRSpriteSequence> GetExistingSpriteSequences();

        protected abstract IEnumerable<TRSpriteTexture> GetExistingSpriteTextures();

        protected abstract AbstractTexturePacker<E, L> CreatePacker();

        protected abstract void ProcessRemovals(AbstractTexturePacker<E, L> packer);

        protected abstract IEnumerable<TRObjectTexture> GetExistingObjectTextures();

        protected abstract IEnumerable<int> GetInvalidObjectTextureIndices();

        protected abstract void WriteObjectTextures(IEnumerable<TRObjectTexture> objectTextures);

        protected abstract void WriteSpriteTextures(IEnumerable<TRSpriteTexture> spriteTextures);

        protected abstract void WriteSpriteSequences(IEnumerable<TRSpriteSequence> spriteSequences);

        protected abstract void RemapMeshTextures(Dictionary<D, Dictionary<int, int>> indexMap);

        public abstract void ResetUnusedTextures();

        protected abstract IEnumerable<E> CollateWatchedTextures(IEnumerable<E> watchedEntities, D definition);

        protected virtual void NotifyTextureWatcher()
        {
            if (_positionMonitor == null)
            {
                return;
            }

            // Notify if anything has been removed first
            if (_entitiesToRemove != null)
            {
                _positionMonitor.EntityTexturesRemoved(new List<E>(_entitiesToRemove));
            }

            // If the monitor isn't interested in any new entities, skip the actual processing
            Dictionary<E, List<int>> watchedTextures = _positionMonitor.GetMonitoredTextureIndices();
            if (watchedTextures.Count == 0)
            {
                return;
            }

            Dictionary<E, List<PositionedTexture>> textureResults = new Dictionary<E, List<PositionedTexture>>();

            foreach (D definition in _importSegments.Keys)
            {
                // Does this definition have any entities we are interested in?
                List<E> entities = new List<E>();
                if (watchedTextures.ContainsKey(definition.Alias))
                {
                    entities.Add(definition.Alias);
                }

                // Allow subclasses to add to the list if required
                entities.AddRange(CollateWatchedTextures(watchedTextures.Keys, definition));

                if (entities.Count == 0)
                {
                    continue;
                }

                foreach (E entity in entities)
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

            _positionMonitor.MonitoredTexturesPositioned(textureResults);
        }

        protected int FindMatchingSegment(List<TexturedTileSegment> segments, TexturedTileSegment segment)
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

        protected void RemapMeshTextures(TRMesh[] meshes, Dictionary<int, int> indexMap)
        {
            foreach (TRMesh mesh in meshes)
            {
                foreach (TRFace4 rect in mesh.TexturedRectangles)
                {
                    rect.Texture = ConvertTextureReference(rect.Texture, indexMap);
                }
                foreach (TRFace3 tri in mesh.TexturedTriangles)
                {
                    tri.Texture = ConvertTextureReference(tri.Texture, indexMap);
                }
            }
        }

        protected ushort ConvertTextureReference(ushort textureReference, Dictionary<int, int> indexMap)
        {
            if (indexMap.ContainsKey(textureReference))
            {
                return (ushort)indexMap[textureReference];
            }
            return 0;
        }
    }
}