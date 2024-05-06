using TRDataControl.Remapping;
using TRImageControl.Packing;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRDataControl;

public class TR1DataExporter : TRDataExporter<TR1Level, TR1Type, TR1SFX, TR1Blob>
{
    public TR1DataExporter()
    {
        Data = new TR1DataProvider();
    }

    protected override TR1Blob CreateBlob(TR1Level level, TR1Type id, TRBlobType blobType)
    {
        return new()
        {
            Type = blobType,
            ID = Data.TranslateAlias(id),
            Alias = id,
            Palette8 = new(),
            SpriteOffsets = new(),
            SoundEffects = new()
        };
    }

    protected override TRTextureRemapper<TR1Level> CreateRemapper()
        => new TR1TextureRemapper();

    protected override bool IsMasterType(TR1Type type)
        => type == TR1Type.Lara;

    protected override TRMesh GetDummyMesh()
        => Level.Models[TR1Type.Lara].Meshes[0];

    protected override void StoreColour(ushort index, TR1Blob blob)
    {
        blob.Palette8[index] = Level.Palette[index];
    }

    protected override void StoreSFX(TR1SFX sfx, TR1Blob blob)
    {
        if (Level.SoundEffects.ContainsKey(sfx))
        {
            blob.SoundEffects[sfx] = Level.SoundEffects[sfx];
        }
    }

    protected override TRTexturePacker CreatePacker()
        => new TR1TexturePacker(Level, Data.TextureTileLimit);

    protected override TRDictionary<TR1Type, TRModel> Models
        => Level.Models;

    protected override TRDictionary<TR1Type, TRStaticMesh> StaticMeshes
        => Level.StaticMeshes;

    protected override TRDictionary<TR1Type, TRSpriteSequence> SpriteSequences
        => Level.Sprites;

    protected override List<TRCinematicFrame> CinematicFrames
        => Level.CinematicFrames;

    protected override void PreCreation(TR1Level level, TR1Type type, TRBlobType blobType)
    {
        switch (type)
        {
            case TR1Type.Pierre:
                AmendPierreGunshot(level);
                AmendPierreDeath(level);
                break;
            case TR1Type.Larson:
                AmendLarsonDeath(level);
                break;
            case TR1Type.SkateboardKid:
                AmendSkaterBoyDeath(level);
                break;
            case TR1Type.CowboyHeadless:
                //AmendDXtre3DTextures(definition);
                break;
            case TR1Type.Natla:
                AmendNatlaDeath(level);
                break;
            case TR1Type.MovingBlock:
                AddMovingBlockSFX(level, BaseLevelDirectory);
                break;
        }
    }

    protected override void PostCreation(TR1Blob blob)
    {
        switch (blob.ID)
        {
            case TR1Type.SkateboardKid:
                if (blob.Palette8.ContainsKey(60))
                {
                    // Incorrect yellow colouring on his arm
                    blob.Palette8[60].Red = 204;
                    blob.Palette8[60].Green = 132;
                    blob.Palette8[60].Blue = 88;
                }
                break;
            case TR1Type.Kold:
                if (blob.Palette8.ContainsKey(185))
                {
                    // Incorrect orange colouring on head and hands
                    blob.Palette8[185].Red = 112;
                    blob.Palette8[185].Green = 72;
                    blob.Palette8[185].Blue = 16;
                }
                break;
        }
    }

    public static void AmendPierreGunshot(TR1Level level)
    {
        level.Models[TR1Type.Pierre].Animations[10].Commands.Add(new TRSFXCommand
        {
            SoundID = (short)TR1SFX.LaraMagnums,
            FrameNumber = 1
        });
    }

    public static void AmendPierreDeath(TR1Level level)
    {
        level.Models[TR1Type.Pierre].Animations[12].Commands.Add(new TRSFXCommand
        {
            SoundID = (short)TR1SFX.PierreDeath,
            FrameNumber = 60
        });
    }

    public static void AmendLarsonDeath(TR1Level level)
    {
        level.Models[TR1Type.Larson].Animations[15].Commands.Add(new TRSFXCommand
        {
            SoundID = (short)TR1SFX.LarsonDeath,
            FrameNumber = 1
        });
    }

    public static void AmendSkaterBoyDeath(TR1Level level)
    {
        TRAnimCommand cmd = level.Models[TR1Type.SkateboardKid].Animations[13].Commands
            .Find(c => c is TRSFXCommand sfx && sfx.SoundID == (short)TR1SFX.SkateKidDeath);
        if (cmd is TRSFXCommand sfxCmd)
        {
            // OG has frame number 0, but this is skipped by the engine
            sfxCmd.FrameNumber = 1;
        }
    }

    public static void AmendNatlaDeath(TR1Level level)
    {
        level.Models[TR1Type.Natla].Animations[13].Commands.Add(new TRSFXCommand
        {
            SoundID = (short)TR1SFX.NatlaDeath,
            FrameNumber = 4
        });
    }

    public static void AddMovingBlockSFX(TR1Level level, string baseLevelDirectory)
    {
        // ToQ moving blocks are silent but we want them to scrape along the floor when they move.
        // Import the trapdoor closing SFX from Vilcabamba and adjust the animations accordingly.

        if (!level.SoundEffects.ContainsKey(TR1SFX.TrapdoorClose))
        {
            TR1Level vilcabamba = new TR1LevelControl().Read(Path.Combine(baseLevelDirectory, TR1LevelNames.VILCABAMBA));
            level.SoundEffects[TR1SFX.TrapdoorClose] = vilcabamba.SoundEffects[TR1SFX.TrapdoorClose];
        }

        for (int i = 2; i < 4; i++)
        {
            level.Models[TR1Type.MovingBlock].Animations[i].Commands.Add(new TRSFXCommand
            {
                SoundID = (short)TR1SFX.TrapdoorClose,
                FrameNumber = 0
            });
        }
    }
}
