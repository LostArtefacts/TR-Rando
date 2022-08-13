using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Helpers;
using TRModelTransporter.Model.Definitions;
using TRModelTransporter.Packing;

namespace TRModelTransporter.Handlers
{
    public class TR2TextureImportHandler : AbstractTextureImportHandler<TR2Entities, TR2Level, TR2ModelDefinition>
    {
        protected override IEnumerable<TRSpriteSequence> GetExistingSpriteSequences()
        {
            return _level.SpriteSequences;
        }

        protected override void WriteSpriteSequences(IEnumerable<TRSpriteSequence> spriteSequences)
        {
            _level.SpriteSequences = spriteSequences.ToArray();
            _level.NumSpriteSequences = (uint)_level.SpriteSequences.Length;
        }

        protected override IEnumerable<TRSpriteTexture> GetExistingSpriteTextures()
        {
            return _level.SpriteTextures.ToList();
        }

        protected override void WriteSpriteTextures(IEnumerable<TRSpriteTexture> spriteTextures)
        {
            _level.SpriteTextures = spriteTextures.ToArray();
            _level.NumSpriteTextures = (uint)_level.SpriteTextures.Length;
        }

        protected override AbstractTexturePacker<TR2Entities, TR2Level> CreatePacker()
        {
            return new TR2TexturePacker(_level);
        }

        protected override void ProcessRemovals(AbstractTexturePacker<TR2Entities, TR2Level> packer)
        {
            List<TR2Entities> removals = new List<TR2Entities>();
            if (_clearUnusedSprites)
            {
                removals.Add(TR2Entities.Map_M_U);
            }

            // Marco is in Floaters by default but he isn't used. Removing the textures will break precompiled deduplication
            // so this remains unimplemented for the time being.
            //List<TRModel> models = _level.Models.ToList();
            //if (models.Find(m => m.ID == (uint)TR2Entities.MarcoBartoli) != null && models.Find(m => m.ID == (uint)TR2Entities.DragonBack_H) == null)
            //{
            //    removals.Add(TR2Entities.MarcoBartoli);
            //}

            if (_entitiesToRemove != null)
            {
                removals.AddRange(_entitiesToRemove);
            }
            packer.RemoveModelSegments(removals, _textureRemap);

            ApplyFlamePatch();

            if (_clearUnusedSprites)
            {
                RemoveUnusedSprites(packer);
            }
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
                _definitions.ToList().FindIndex(d => flameEnemies.Contains(d.Entity)) != -1 ||
                _level.Models.ToList().FindIndex(m => flameEnemies.Contains((TR2Entities)m.ID)) != -1
            )
            {
                List<TRSpriteSequence> sequences = _level.SpriteSequences.ToList();
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

                        _level.SpriteSequences = sequences.ToArray();
                        _level.NumSpriteSequences++;
                    }
                    else
                    {
                        // #275 Rather than just pointing the blast sequence offset to the grenade sequence offset,
                        // retain the original sprite texture objects but just remap where they point in the tiles.
                        for (int i = 0; i < sequences[grenadeSequence].NegativeLength * -1; i++)
                        {
                            _level.SpriteTextures[_level.SpriteSequences[blastSequence].Offset + i] = _level.SpriteTextures[_level.SpriteSequences[grenadeSequence].Offset + i];
                        }
                    }
                }
            }
        }

        private void RemoveUnusedSprites(AbstractTexturePacker<TR2Entities, TR2Level> packer)
        {
            List<TR2Entities> unusedItems = new List<TR2Entities>
            {
                TR2Entities.PistolAmmo_S_P,
                TR2Entities.Map_M_U,
                TR2Entities.GrayDisk_S_H
            };

            ISet<TR2Entities> allEntities = new HashSet<TR2Entities>();
            for (int i = 0; i < _level.Entities.Length; i++)
            {
                allEntities.Add((TR2Entities)_level.Entities[i].TypeID);
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

        protected override IEnumerable<TRObjectTexture> GetExistingObjectTextures()
        {
            return _level.ObjectTextures.ToList();
        }

        protected override IEnumerable<int> GetInvalidObjectTextureIndices()
        {
            return _level.GetInvalidObjectTextureIndices();
        }

        protected override void WriteObjectTextures(IEnumerable<TRObjectTexture> objectTextures)
        {
            _level.ObjectTextures = objectTextures.ToArray();
            _level.NumObjectTextures = (uint)_level.ObjectTextures.Length;
        }

        protected override void RemapMeshTextures(Dictionary<TR2ModelDefinition, Dictionary<int, int>> indexMap)
        {
            foreach (TR2ModelDefinition definition in indexMap.Keys)
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

        public override void ResetUnusedTextures()
        {
            _level.ResetUnusedTextures();
        }

        protected override IEnumerable<TR2Entities> CollateWatchedTextures(IEnumerable<TR2Entities> watchedEntities, TR2ModelDefinition definition)
        {
            // Ensure the likes of the flamethrower having been imported triggers the fact that
            // the flame sprite sequence has been positioned.
            List<TR2Entities> entities = new List<TR2Entities>();
            foreach (TR2Entities spriteEntity in definition.SpriteSequences.Keys)
            {
                if (watchedEntities.Contains(spriteEntity))
                {
                    entities.Add(spriteEntity);
                }
            }

            return entities;
        }
    }
}