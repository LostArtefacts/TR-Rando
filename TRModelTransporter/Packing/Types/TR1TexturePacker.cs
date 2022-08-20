using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Helpers;
using TRModelTransporter.Model.Textures;
using TRTexture16Importer;
using TRTexture16Importer.Helpers;

namespace TRModelTransporter.Packing
{
    public class TR1TexturePacker : AbstractTexturePacker<TREntities, TRLevel>
    {
        private const int _maximumTiles = 16;

        public TR1PaletteManager PaletteManager { get; set; }

        public override uint NumLevelImages => Level.NumImages;

        public TR1TexturePacker(TRLevel level, ITextureClassifier classifier = null)
            : base(level, _maximumTiles, classifier) { }

        protected override List<AbstractIndexedTRTexture> LoadObjectTextures()
        {
            List<AbstractIndexedTRTexture> textures = new List<AbstractIndexedTRTexture>((int)Level.NumObjectTextures);
            for (int i = 0; i < Level.NumObjectTextures; i++)
            {
                TRObjectTexture texture = Level.ObjectTextures[i];
                if (texture.IsValid())
                {
                    textures.Add(new IndexedTRObjectTexture
                    {
                        Index = i,
                        Classification = _levelClassifier,
                        Texture = texture
                    });
                }
            }
            return textures;
        }

        protected override List<AbstractIndexedTRTexture> LoadSpriteTextures()
        {
            List<AbstractIndexedTRTexture> textures = new List<AbstractIndexedTRTexture>((int)Level.NumSpriteTextures);
            for (int i = 0; i < Level.NumSpriteTextures; i++)
            {
                TRSpriteTexture texture = Level.SpriteTextures[i];
                if (texture.IsValid())
                {
                    textures.Add(new IndexedTRSpriteTexture
                    {
                        Index = i,
                        Classification = _levelClassifier,
                        Texture = texture
                    });
                }
            }
            return textures;
        }

        protected override TRMesh[] GetModelMeshes(TREntities modelEntity)
        {
            return TRMeshUtilities.GetModelMeshes(Level, modelEntity);
        }

        protected override TRSpriteSequence GetSpriteSequence(TREntities entity)
        {
            return Level.SpriteSequences.ToList().Find(s => s.SpriteID == (int)entity);
        }

        protected override IEnumerable<TREntities> GetAllModelTypes()
        {
            List<TREntities> modelIDs = new List<TREntities>();
            foreach (TRModel model in Level.Models)
            {
                modelIDs.Add((TREntities)model.ID);
            }
            return modelIDs;
        }

        protected override void CreateImageSpace(uint count)
        {
            List<TRTexImage8> imgs8 = Level.Images8.ToList();

            for (int i = 0; i < count; i++)
            {
                imgs8.Add(new TRTexImage8 { Pixels = new byte[256 * 256] });
            }

            Level.Images8 = imgs8.ToArray();
            Level.NumImages += count;
        }

        public override Bitmap GetTile(int tileIndex)
        {
            return Level.Images8[tileIndex].ToBitmap(Level.Palette);
        }

        public override void SetTile(int tileIndex, Bitmap bitmap)
        {
            if (PaletteManager == null)
            {
                PaletteManager = new TR1PaletteManager
                {
                    Level = Level
                };
            }
            PaletteManager.ChangedTiles[tileIndex] = bitmap;
        }

        protected override void PostCommit()
        {
            if (PaletteManager != null)
            {
                PaletteManager.MergeTiles();
            }
        }
    }
}