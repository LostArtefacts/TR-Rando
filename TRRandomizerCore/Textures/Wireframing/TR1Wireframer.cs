using System.Drawing;
using System.Drawing.Drawing2D;
using TRFDControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRModelTransporter.Helpers;
using TRModelTransporter.Model.Textures;
using TRModelTransporter.Packing;
using TRRandomizerCore.Utilities;
using TRTexture16Importer.Helpers;

namespace TRRandomizerCore.Textures;

public class TR1Wireframer : AbstractTRWireframer<TR1Type, TR1Level>
{
    private static readonly List<TR1Type> _laraEntities = new()
    {
        TR1Type.Lara, TR1Type.LaraPonytail_H_U, TR1Type.CutsceneActor1,
        TR1Type.LaraPistolAnim_H, TR1Type.LaraShotgunAnim_H, TR1Type.LaraMagnumAnim_H,
        TR1Type.LaraUziAnimation_H, TR1Type.LaraMiscAnim_H, TR1Type.CameraTarget_N,
        TR1Type.FlameEmitter_N, TR1Type.NonShootingAtlantean_N, TR1Type.ShootingAtlantean_N,
        TR1Type.MidasHand_N
    };

    private static readonly List<TR1Type> _enemyPlaceholderEntities = new()
    {
        TR1Type.NonShootingAtlantean_N, TR1Type.ShootingAtlantean_N
    };

    private static readonly List<TR1Type> _additionalEnemyEntities = new()
    {
        TR1Type.Missile1_H, TR1Type.Missile2_H, TR1Type.Missile3_H,
        TR1Type.CutsceneActor2, TR1Type.CutsceneActor3, TR1Type.CutsceneActor4,
        TR1Type.AdamEgg, TR1Type.ScionHolder, TR1Type.ScionPiece3_S_P, TR1Type.ScionPiece4_S_P,
        TR1Type.Skateboard, TR1Type.Doppelganger
    };

    private static readonly List<TR1Type> _pickupModels = new()
    {
        TR1Type.Pistols_M_H, TR1Type.Shotgun_M_H, TR1Type.Magnums_M_H, TR1Type.Uzis_M_H,
        TR1Type.ShotgunAmmo_M_H, TR1Type.MagnumAmmo_M_H, TR1Type.UziAmmo_M_H,
        TR1Type.SmallMed_M_H, TR1Type.LargeMed_M_H,
        TR1Type.Puzzle1_M_H, TR1Type.Puzzle2_M_H, TR1Type.Puzzle3_M_H, TR1Type.Puzzle4_M_H,
        TR1Type.Key1_M_H, TR1Type.Key2_M_H, TR1Type.Key3_M_H, TR1Type.Key4_M_H,
        TR1Type.Quest1_M_H, TR1Type.Quest2_M_H,
        TR1Type.ScionPiece_M_H
    };

    public override bool Is8BitPalette => true;

    private TR1TexturePacker _packer;

    protected override AbstractTexturePacker<TR1Type, TR1Level> CreatePacker(TR1Level level)
    {
        return _packer = new TR1TexturePacker(level);
    }

    protected override bool IsSkybox(TRModel model)
    {
        return false;
    }

    protected override bool IsInteractableModel(TRModel model)
    {
        TR1Type type = (TR1Type)model.ID;
        return TR1TypeUtilities.IsSwitchType(type)
            || TR1TypeUtilities.IsKeyholeType(type)
            || TR1TypeUtilities.IsSlotType(type)
            || TR1TypeUtilities.IsPushblockType(type)
            || type == TR1Type.Barricade
            || type == TR1Type.Compass_M_H;
    }

    protected override bool ShouldSolidifyModel(TRModel model)
    {
        return _data.Has3DPickups && _pickupModels.Contains((TR1Type)model.ID);
    }

    protected override int GetBlackPaletteIndex(TR1Level level)
    {
        return ImportColour(level, Color.Black);
    }

    protected override IEnumerable<int> GetInvalidObjectTextureIndices(TR1Level level)
    {
        return level.GetInvalidObjectTextureIndices();
    }

    protected override List<TRMesh> GetLevelMeshes(TR1Level level)
    {
        return level.Meshes;
    }

    protected override List<TRMesh> GetModelMeshes(TR1Level level, TRModel model)
    {
        return TRMeshUtilities.GetModelMeshes(level, model);
    }

    protected override List<TRModel> GetModels(TR1Level level)
    {
        return level.Models;
    }

    protected override TRObjectTexture[] GetObjectTextures(TR1Level level)
    {
        return level.ObjectTextures;
    }

    protected override IEnumerable<IEnumerable<TRFace3>> GetRoomFace3s(TR1Level level)
    {
        List<List<TRFace3>> faces = new();
        foreach (TRRoom room in level.Rooms)
        {
            faces.Add(room.RoomData.Triangles.ToList());
        }
        return faces;
    }

    protected override IEnumerable<IEnumerable<TRFace4>> GetRoomFace4s(TR1Level level)
    {
        List<List<TRFace4>> faces = new();
        foreach (TRRoom room in level.Rooms)
        {
            faces.Add(room.RoomData.Rectangles.ToList());
        }
        return faces;
    }

    protected override TRMesh GetStaticMesh(TR1Level level, TRStaticMesh staticMesh)
    {
        return TRMeshUtilities.GetMesh(level, staticMesh.Mesh);
    }

    protected override List<TRStaticMesh> GetStaticMeshes(TR1Level level)
    {
        return level.StaticMeshes;
    }

    protected override int ImportColour(TR1Level level, Color c)
    {
        _packer.PaletteManager ??= new();
        return _packer.PaletteManager.AddPredefinedColour(c);
    }

    protected override bool IsLaraModel(TRModel model)
    {
        return _laraEntities.Contains((TR1Type)model.ID);
    }

    protected override bool IsEnemyModel(TRModel model)
    {
        TR1Type id = (TR1Type)model.ID;
        return TR1TypeUtilities.IsEnemyType(id) || _additionalEnemyEntities.Contains(id);
    }

    protected override bool IsEnemyPlaceholderModel(TRModel model)
    {
        TR1Type id = (TR1Type)model.ID;
        return _enemyPlaceholderEntities.Contains(id);
    }

    protected override void ResetPaletteTracking(TR1Level level)
    {
        _packer.PaletteManager?.MergePredefinedColours();
    }

    protected override void ResetUnusedTextures(TR1Level level)
    {
        level.ResetUnusedTextures();
    }

    protected override void SetObjectTextures(TR1Level level, IEnumerable<TRObjectTexture> textures)
    {
        level.ObjectTextures = textures.ToArray();
        level.NumObjectTextures = (uint)level.ObjectTextures.Length;
    }

    protected override void SetSkyboxVisible(TR1Level level) { }

    protected override Dictionary<TRFace4, List<TRVertex>> CollectLadders(TR1Level level)
    {
        return new Dictionary<TRFace4, List<TRVertex>>();
    }

    protected override List<TRFace4> CollectTriggerFaces(TR1Level level, List<FDTrigType> triggerTypes)
    {
        return FaceUtilities.GetTriggerFaces(level, triggerTypes, false);
    }

    protected override List<TRFace4> CollectDeathFaces(TR1Level level)
    {
        return FaceUtilities.GetTriggerFaces(level, new List<FDTrigType>(), true);
    }

    protected override TRAnimatedTexture[] GetAnimatedTextures(TR1Level level)
    {
        return level.AnimatedTextures;
    }

    protected override void SetAnimatedTextures(TR1Level level, TRAnimatedTexture[] animatedTextures, ushort length)
    {
        level.AnimatedTextures = animatedTextures;
        level.NumAnimatedTextures = length;
    }

    protected override Dictionary<ushort, TexturedTileSegment> CreateSpecialSegments(TR1Level level, Pen pen)
    {
        Dictionary<ushort, TexturedTileSegment> segments = new();
        foreach (SpecialTextureHandling special in _data.SpecialTextures)
        {
            switch (special.Type)
            {
                case SpecialTextureType.MidasDoors:
                    foreach (ushort texture in special.Textures)
                    {
                        if (CreateMidasDoor(level, pen, texture, special.Mode) is TexturedTileSegment segment)
                        {
                            segments[texture] = segment;
                        }
                    }
                    break;
            }
        }

        return segments;
    }

    private TexturedTileSegment CreateMidasDoor(TR1Level level, Pen pen, ushort textureIndex, SpecialTextureMode mode)
    {
        TRModel doorModel = FindDoorModel(level, textureIndex);
        if (doorModel == null)
        {
            return null;
        }

        TR1Entity doorInstance = level.Entities.Find(e => e.TypeID == (TR1Type)doorModel.ID);
        if (doorInstance == null)
        {
            return null;
        }

        const int width = 64;
        const int height = 16;
                    
        IndexedTRObjectTexture texture = CreateTexture(new Rectangle(0, 0, width, height));
        BitmapGraphics frame = CreateFrame(width, height, pen, SmoothingMode.AntiAlias, false);

        int flags = (doorInstance.Flags & 0x3E00) >> 9;
        for (int i = 0; i < 5; i++)
        {
            int x = 3 + i * 12;
            int y = 3;
            int w = 10;
            int h = 10;

            // Make a smaller rectangle
            frame.Graphics.DrawRectangle(pen, x, y, w - 1, h - 1);

            // Decorate based on the door's bits
            bool doorBitSet = (flags & (1 << i)) == 0;
            switch (mode)
            {
                case SpecialTextureMode.MidasDoorBars:
                    // Bar at top = lever up; at bottom = lever down
                    y += doorBitSet ? 5 : 1;
                    frame.Graphics.DrawLine(pen, x + 4, y, x + 4, y + 3);
                    frame.Graphics.DrawLine(pen, x + 5, y, x + 5, y + 3);
                    break;
                case SpecialTextureMode.MidasDoorFill:
                    // Empty blocks need "filled" - levers go down
                    if (!doorBitSet)
                    {
                        frame.Graphics.FillRectangle(pen.Brush, x, y, w - 1, h - 1);
                    }
                    break;
                case SpecialTextureMode.MidasDoorLines:
                    if (doorBitSet)
                    {
                        // Lever up
                        frame.Graphics.DrawLine(pen, x + 1, y + 4, x + 8, y + 4);
                        frame.Graphics.DrawLine(pen, x + 1, y + 5, x + 8, y + 5);
                    }
                    else
                    {
                        // Lever down
                        frame.Graphics.DrawLine(pen, x + 4, y + 1, x + 4, y + 8);
                        frame.Graphics.DrawLine(pen, x + 5, y + 1, x + 5, y + 8);
                    }
                    break;
                case SpecialTextureMode.MidasDoorDiagonals:
                    if (doorBitSet)
                    {
                        // Lever up \
                        frame.Graphics.DrawLine(pen, x + 1, y + 1, x + 8, y + 8);
                    }
                    else
                    {
                        // Lever down /
                        frame.Graphics.DrawLine(pen, x + 1, y + 8, x + 8, y + 1);
                    }
                    break;
            }
        }

        return new TexturedTileSegment(texture, frame.Bitmap);
    }

    private static TRModel FindDoorModel(TR1Level level, ushort textureIndex)
    {
        foreach (TRModel model in level.Models)
        {
            TR1Type type = (TR1Type)model.ID;
            if (!TR1TypeUtilities.IsDoorType(type))
            {
                continue;
            }

            foreach (TRMesh mesh in TRMeshUtilities.GetModelMeshes(level, type))
            {
                if (mesh.TexturedRectangles.Any(f => f.Texture == textureIndex))
                {
                    return model;
                }
            }
        }

        return null;
    }
}
