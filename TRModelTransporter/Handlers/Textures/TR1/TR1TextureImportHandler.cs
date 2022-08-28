using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Helpers;
using TRModelTransporter.Model.Definitions;
using TRModelTransporter.Packing;
using TRTexture16Importer.Helpers;

namespace TRModelTransporter.Handlers.Textures
{
    public class TR1TextureImportHandler : AbstractTextureImportHandler<TREntities, TRLevel, TR1ModelDefinition>
    {
        public TR1PaletteManager PaletteManager { get; set; }

        protected override IEnumerable<TRSpriteSequence> GetExistingSpriteSequences()
        {
            // Allow replacing the Explosion sequence in Vilcabamba (it's there but empty)
            List<TRSpriteSequence> sequences = _level.SpriteSequences.ToList();
            TRSpriteSequence explosion = sequences.Find(s => s.SpriteID == (int)TREntities.Explosion1_S_H);
            if (explosion != null && explosion.NegativeLength == -1)
            {
                sequences.Remove(explosion);
            }
            return sequences;
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

        protected override AbstractTexturePacker<TREntities, TRLevel> CreatePacker()
        {
            return new TR1TexturePacker(_level)
            {
                PaletteManager = PaletteManager
            };
        }

        protected override void ProcessRemovals(AbstractTexturePacker<TREntities, TRLevel> packer)
        {
            List<TREntities> removals = new List<TREntities>();
            if (_clearUnusedSprites)
            {
                removals.Add(TREntities.Map_M_U);
            }

            if (_entitiesToRemove != null)
            {
                removals.AddRange(_entitiesToRemove);
            }
            packer.RemoveModelSegments(removals, _textureRemap);

            if (_clearUnusedSprites)
            {
                RemoveUnusedSprites(packer);
            }
        }

        private void RemoveUnusedSprites(AbstractTexturePacker<TREntities, TRLevel> packer)
        {
            List<TREntities> unusedItems = new List<TREntities>
            {
                TREntities.PistolAmmo_S_P,
                TREntities.Map_M_U
            };

            ISet<TREntities> allEntities = new HashSet<TREntities>();
            for (int i = 0; i < _level.Entities.Length; i++)
            {
                allEntities.Add((TREntities)_level.Entities[i].TypeID);
            }

            for (int i = unusedItems.Count - 1; i >= 0; i--)
            {
                if (allEntities.Contains(unusedItems[i]))
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

        protected override void RemapMeshTextures(Dictionary<TR1ModelDefinition, Dictionary<int, int>> indexMap)
        {
            foreach (TR1ModelDefinition definition in indexMap.Keys)
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
            // Patch - this doesn't break the game, but it prevents the level being
            // opened in trview. Some textures will now be unused, but rather than
            // removing them and having to reindex everything that points to the
            // the object textures, we'll just reset them to atlas 0, and set all
            // coordinates to 0.

            _level.ResetUnusedTextures();
        }

        protected override IEnumerable<TREntities> CollateWatchedTextures(IEnumerable<TREntities> watchedEntities, TR1ModelDefinition definition)
        {
            return new List<TREntities>();
        }
    }
}