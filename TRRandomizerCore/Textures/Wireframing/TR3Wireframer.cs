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

public class TR3Wireframer : AbstractTRWireframer<TR3Type, TR3Level>
{
    private static readonly List<TR3Type> _laraEntities = new()
    {
        TR3Type.Lara, TR3Type.LaraPonytail_H, TR3Type.LaraFlareAnimation_H,
        TR3Type.LaraPistolAnimation_H, TR3Type.LaraShotgunAnimation_H, TR3Type.LaraUziAnimation_H,
        TR3Type.LaraDeagleAnimation_H, TR3Type.LaraMP5Animation_H, TR3Type.LaraGrenadeAnimation_H,
        TR3Type.LaraRocketAnimation_H, TR3Type.LaraExtraAnimation_H, TR3Type.LaraSkin_H,
        TR3Type.LaraHarpoonAnimation_H, TR3Type.LaraVehicleAnimation_H
    };

    private static readonly List<TR3Type> _additionalEnemyEntities = new()
    {
        TR3Type.ShivaStatue, TR3Type.MonkeyKeyMeshswap, TR3Type.MonkeyMedMeshswap
    };

    private TRPalette16Control _paletteTracker;

    protected override AbstractTexturePacker<TR3Type, TR3Level> CreatePacker(TR3Level level)
    {
        return new TR3TexturePacker(level);
    }

    protected override bool IsInteractableModel(TRModel model)
    {
        TR3Type type = (TR3Type)model.ID;
        return TR3TypeUtilities.IsSwitchType(type)
            || TR3TypeUtilities.IsKeyholeType(type)
            || TR3TypeUtilities.IsSlotType(type)
            || TR3TypeUtilities.IsPushblockType(type);
    }

    protected override int GetBlackPaletteIndex(TR3Level level)
    {
        return level.Palette16.ToList().FindIndex(c => c.Red + c.Green + c.Blue == 0);
    }

    protected override IEnumerable<int> GetInvalidObjectTextureIndices(TR3Level level)
    {
        return level.GetInvalidObjectTextureIndices();
    }

    protected override List<TRMesh> GetLevelMeshes(TR3Level level)
    {
        return level.Meshes;
    }

    protected override List<TRMesh> GetModelMeshes(TR3Level level, TRModel model)
    {
        return TRMeshUtilities.GetModelMeshes(level, model);
    }

    protected override List<TRModel> GetModels(TR3Level level)
    {
        return level.Models.ToList();
    }

    protected override List<TRObjectTexture> GetObjectTextures(TR3Level level)
    {
        return level.ObjectTextures;
    }

    protected override IEnumerable<IEnumerable<TRFace3>> GetRoomFace3s(TR3Level level)
    {
        List<List<TRFace3>> faces = new();
        foreach (TR3Room room in level.Rooms)
        {
            faces.Add(room.RoomData.Triangles.ToList());
        }
        return faces;
    }

    protected override IEnumerable<IEnumerable<TRFace4>> GetRoomFace4s(TR3Level level)
    {
        List<List<TRFace4>> faces = new();
        foreach (TR3Room room in level.Rooms)
        {
            faces.Add(room.RoomData.Rectangles.ToList());
        }
        return faces;
    }

    protected override TRMesh GetStaticMesh(TR3Level level, TRStaticMesh staticMesh)
    {
        return TRMeshUtilities.GetMesh(level, staticMesh.Mesh);
    }

    protected override List<TRStaticMesh> GetStaticMeshes(TR3Level level)
    {
        return level.StaticMeshes;
    }

    protected override int ImportColour(TR3Level level, Color c)
    {
        _paletteTracker ??= new(level);
        return _paletteTracker.Import(c);
    }

    protected override bool IsLaraModel(TRModel model)
    {
        return _laraEntities.Contains((TR3Type)model.ID);
    }

    protected override bool IsEnemyModel(TRModel model)
    {
        TR3Type id = (TR3Type)model.ID;
        return TR3TypeUtilities.IsEnemyType(id) || _additionalEnemyEntities.Contains(id);
    }

    protected override bool IsSkybox(TRModel model)
    {
        return (TR3Type)model.ID == TR3Type.Skybox_H;
    }

    protected override bool ShouldSolidifyModel(TRModel model)
    {
        TR3Type type = (TR3Type)model.ID;
        return TR3TypeUtilities.IsAnyPickupType(type) || TR3TypeUtilities.IsCrystalPickup(type);
    }

    protected override void ResetUnusedTextures(TR3Level level)
    {
        level.ResetUnusedTextures();
    }

    protected override void SetSkyboxVisible(TR3Level level)
    {
        foreach (TR3Room room in level.Rooms)
        {
            room.IsSkyboxVisible = true;
        }
    }

    protected override Dictionary<TRFace4, List<TRVertex>> CollectLadders(TR3Level level)
    {
        return FaceUtilities.GetClimbableFaces(level);
    }

    protected override List<TRFace4> CollectTriggerFaces(TR3Level level, List<FDTrigType> triggerTypes)
    {
        return FaceUtilities.GetTriggerFaces(level, triggerTypes, false);
    }

    protected override List<TRFace4> CollectDeathFaces(TR3Level level)
    {
        return FaceUtilities.GetTriggerFaces(level, new List<FDTrigType>(), true);
    }

    protected override TRAnimatedTexture[] GetAnimatedTextures(TR3Level level)
    {
        return level.AnimatedTextures;
    }

    protected override void SetAnimatedTextures(TR3Level level, TRAnimatedTexture[] animatedTextures, ushort length)
    {
        level.AnimatedTextures = animatedTextures;
        level.NumAnimatedTextures = length;
    }

    protected override Dictionary<ushort, TexturedTileSegment> CreateSpecialSegments(TR3Level level, Pen pen)
    {
        Dictionary<ushort, TexturedTileSegment> segments = new();
        foreach (SpecialTextureHandling special in _data.SpecialTextures)
        {
            switch (special.Type)
            {
                case SpecialTextureType.CrashPads:
                    foreach (ushort texture in special.Textures)
                    {
                        if (CreateCrashPad(pen, special.Mode) is TexturedTileSegment segment)
                        {
                            segments[texture] = segment;
                        }
                    }
                    break;
            }
        }

        return segments;
    }

    private TexturedTileSegment CreateCrashPad(Pen pen, SpecialTextureMode mode)
    {
        const int width = 64;
        const int height = 64;

        IndexedTRObjectTexture texture = CreateTexture(new Rectangle(0, 0, width, height));
        BitmapGraphics frame = CreateFrame(width, height, pen, SmoothingMode.AntiAlias, true);

        switch (mode)
        {
            case SpecialTextureMode.CrashPadCircle:
                frame.Graphics.FillEllipse(pen.Brush, new Rectangle(16, 16, 46, 46));
                break;
            case SpecialTextureMode.CrashPadDiamond:
                frame.Graphics.FillPolygon(pen.Brush, new Point[]
                {
                    new(32, 16),
                    new(48, 32),
                    new(32, 48),
                    new(16, 32),
                });
                break;
        }

        return new TexturedTileSegment(texture, frame.Bitmap);
    }
}
