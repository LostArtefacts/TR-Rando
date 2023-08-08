using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRLevelControl.Model.Enums;
using TRModelTransporter.Helpers;
using TRModelTransporter.Model.Textures;
using TRTexture16Importer;

namespace TRModelTransporter.Packing;

public class TR2TexturePacker : AbstractTexturePacker<TR2Entities, TR2Level>
{
    private const int _maximumTiles = 16;

    public override uint NumLevelImages => Level.NumImages;

    public TR2TexturePacker(TR2Level level, ITextureClassifier classifier = null)
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

    protected override TRMesh[] GetModelMeshes(TR2Entities modelEntity)
    {
        return TRMeshUtilities.GetModelMeshes(Level, modelEntity);
    }

    protected override TRSpriteSequence GetSpriteSequence(TR2Entities entity)
    {
        return Level.SpriteSequences.ToList().Find(s => s.SpriteID == (int)entity);
    }

    protected override IEnumerable<TR2Entities> GetAllModelTypes()
    {
        List<TR2Entities> modelIDs = new List<TR2Entities>();
        foreach (TRModel model in Level.Models)
        {
            modelIDs.Add((TR2Entities)model.ID);
        }
        return modelIDs;
    }

    protected override void CreateImageSpace(uint count)
    {
        List<TRTexImage16> imgs16 = Level.Images16.ToList();
        List<TRTexImage8> imgs8 = Level.Images8.ToList();

        for (int i = 0; i < count; i++)
        {
            imgs16.Add(new TRTexImage16());
            imgs8.Add(new TRTexImage8 { Pixels = new byte[256 * 256] });
        }

        Level.Images16 = imgs16.ToArray();
        Level.Images8 = imgs8.ToArray();
        Level.NumImages += count;
    }

    public override Bitmap GetTile(int tileIndex)
    {
        return Level.Images16[tileIndex].ToBitmap();
    }

    public override void SetTile(int tileIndex, Bitmap bitmap)
    {
        Level.Images16[tileIndex].Pixels = TextureUtilities.ImportFromBitmap(bitmap);
    }
}
