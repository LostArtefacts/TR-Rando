using System.Diagnostics;
using System.Drawing;
using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TRDataControl;

public class TR2DataExporter : TRDataExporter<TR2Level, TR2Type, TR2SFX, TR2Blob>
{
    public TR2DataExporter()
    {
        Data = new TR2DataProvider();
    }

    protected override TR2Blob CreateBlob(TR2Level level, TR2Type id, TRBlobType blobType)
    {
        return new()
        {
            Type = blobType,
            ID = Data.TranslateAlias(id),
            Alias = id,
            Palette16 = new(),
            SpriteOffsets = new(),
            SoundEffects = new()
        };
    }

    protected override TRTextureRemapper<TR2Level> CreateRemapper(TR2Level level)
        => new TR2TextureRemapper(level);

    protected override bool IsMasterType(TR2Type type)
        => type == TR2Type.Lara;

    protected override TRMesh GetDummyMesh()
        => Level.Models[TR2Type.Lara].Meshes[0];

    protected override void StoreColour(ushort index, TR2Blob blob)
    {
        blob.Palette16[index] = Level.Palette16[index >> 8];
    }

    protected override void StoreSFX(TR2SFX sfx, TR2Blob blob)
    {
        if (Level.SoundEffects.ContainsKey(sfx))
        {
            blob.SoundEffects[sfx] = Level.SoundEffects[sfx];
        }
    }

    protected override TRTexturePacker CreatePacker()
        => new TR2TexturePacker(Level, Data.TextureTileLimit);

    protected override TRDictionary<TR2Type, TRModel> Models
        => Level.Models;

    protected override TRDictionary<TR2Type, TRStaticMesh> StaticMeshes
        => Level.StaticMeshes;

    protected override TRDictionary<TR2Type, TRSpriteSequence> SpriteSequences
        => Level.Sprites;

    protected override List<TRCinematicFrame> CinematicFrames
        => Level.CinematicFrames;

    protected override void PostCreation(TR2Blob blob)
    {
        switch (blob.Alias)
        {
            case TR2Type.DragonExplosion1_H:
            case TR2Type.DragonExplosion2_H:
                ScaleSphereOfDoom(blob);
                break;
            case TR2Type.Gunman1TopixtorORC:
            case TR2Type.Gunman1TopixtorCAC:
                //AmendDXtre3DTextures(blob);
                break;
            case TR2Type.FlamethrowerGoonTopixtor:
                //AmendDXtre3DTextures(blob);
                //AmendDXtre3DFlameTextures(blob);
                break;

        }
    }

    private static void ScaleSphereOfDoom(TR2Blob blob)
    {
        // Scale down the Sphere of Doom textures for a better chance of
        // importing into levels later.

        Debug.Assert(blob.Textures.Count == 1);
        Debug.Assert(blob.Textures[0].Width == 128);
        Debug.Assert(blob.Textures[0].Height == 128);

        TRTextileRegion region = blob.Textures[0];
        Size newSize = new(64, 64);

        using Bitmap scaledBmp = new(region.Image.ToBitmap(), newSize);

        region.Image = new(scaledBmp);
        region.Bounds = new(0, 0, newSize.Width, newSize.Height);
        region.GenerateID();
        region.Segments.ForEach(s => s.Texture.Size = newSize);
    }

    //protected void AmendDXtre3DTextures(TR2Blob definition)
    //{
    //    // Dxtre3D can produce faulty UV mapping which can cause casting issues
    //    // when used in model IO, so fix coordinates at this stage.
    //    // This may no longer be the case...
    //    foreach (List<IndexedTRObjectTexture> textureList in definition.ObjectTextures.Values)
    //    {
    //        foreach (IndexedTRObjectTexture texture in textureList)
    //        {
    //            Dictionary<TRObjectTextureVert, Point> points = new();
    //            foreach (TRObjectTextureVert vertex in texture.Texture.Vertices)
    //            {
    //                int x = vertex.XCoordinate.Fraction;
    //                if (vertex.XCoordinate.Whole == byte.MaxValue)
    //                {
    //                    x++;
    //                }

    //                int y = vertex.YCoordinate.Fraction;
    //                if (vertex.YCoordinate.Whole == byte.MaxValue)
    //                {
    //                    y++;
    //                }
    //                points[vertex] = new Point(x, y);
    //            }

    //            int maxX = points.Values.Max(p => p.X);
    //            int maxY = points.Values.Max(p => p.Y);
    //            foreach (TRObjectTextureVert vertex in texture.Texture.Vertices)
    //            {
    //                Point p = points[vertex];
    //                if (p.X == maxX && maxX != byte.MaxValue)
    //                {
    //                    vertex.XCoordinate.Fraction--;
    //                    vertex.XCoordinate.Whole = byte.MaxValue;
    //                }
    //                if (p.Y == maxY && maxY != byte.MaxValue)
    //                {
    //                    vertex.YCoordinate.Fraction--;
    //                    vertex.YCoordinate.Whole = byte.MaxValue;
    //                }
    //            }
    //        }
    //    }
    //}

    //private static void AmendDXtre3DFlameTextures(TR2Blob definition)
    //{
    //    if (!definition.SpriteSequences.ContainsKey(TR2Type.Flame_S_H))
    //    {
    //        return;
    //    }

    //    // Ensures the flame sprite is aligned to OG - required for texture monitoring
    //    Dictionary<int, List<IndexedTRSpriteTexture>> defaultSprites = definition.SpriteTextures[TR2Type.Flame_S_H];
    //    foreach (int id in defaultSprites.Keys)
    //    {
    //        foreach (IndexedTRSpriteTexture sprite in defaultSprites[id])
    //        {
    //            sprite.Index += 22;
    //        }
    //    }
    //}
}
