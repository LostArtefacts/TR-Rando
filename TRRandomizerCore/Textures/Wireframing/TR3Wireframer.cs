using System.Drawing;
using System.Drawing.Drawing2D;
using TRImageControl;
using TRImageControl.Packing;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Utilities;

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

    protected override TRTexturePacker CreatePacker(TR3Level level)
    {
        return new TR3TexturePacker(level);
    }

    protected override bool IsInteractableModel(TR3Type type)
    {
        return TR3TypeUtilities.IsSwitchType(type)
            || TR3TypeUtilities.IsKeyholeType(type)
            || TR3TypeUtilities.IsSlotType(type)
            || TR3TypeUtilities.IsPushblockType(type);
    }

    protected override int GetBlackPaletteIndex(TR3Level level)
    {
        return level.Palette16.ToList().FindIndex(c => c.Red + c.Green + c.Blue == 0);
    }

    protected override TRDictionary<TR3Type, TRModel> GetModels(TR3Level level)
    {
        return level.Models;
    }

    protected override IEnumerable<IEnumerable<TRFace>> GetRoomFace3s(TR3Level level)
    {
        List<List<TRFace>> faces = new();
        foreach (TR3Room room in level.Rooms)
        {
            faces.Add(room.Mesh.Triangles);
        }
        return faces;
    }

    protected override IEnumerable<IEnumerable<TRFace>> GetRoomFace4s(TR3Level level)
    {
        List<List<TRFace>> faces = new();
        foreach (TR3Room room in level.Rooms)
        {
            faces.Add(room.Mesh.Rectangles);
        }
        return faces;
    }

    protected override int ImportColour(TR3Level level, Color c)
    {
        _paletteTracker ??= new(level);
        return _paletteTracker.Import(c);
    }

    protected override bool IsLaraModel(TR3Type type)
    {
        return _laraEntities.Contains(type);
    }

    protected override bool IsEnemyModel(TR3Type type)
    {
        return TR3TypeUtilities.IsEnemyType(type) || _additionalEnemyEntities.Contains(type);
    }

    protected override bool IsSkybox(TR3Type type)
    {
        return type == TR3Type.Skybox_H;
    }

    protected override bool ShouldSolidifyModel(TR3Type type)
    {
        return TR3TypeUtilities.IsAnyPickupType(type) || TR3TypeUtilities.IsCrystalPickup(type);
    }

    protected override void SetSkyboxVisible(TR3Level level)
    {
        foreach (TR3Room room in level.Rooms)
        {
            room.IsSkyboxVisible = true;
        }
    }

    protected override Dictionary<TRFace, List<TRVertex>> CollectLadders(TR3Level level)
    {
        return FaceUtilities.GetClimbableFaces(level);
    }

    protected override List<TRFace> CollectTriggerFaces(TR3Level level, List<FDTrigType> triggerTypes)
    {
        return FaceUtilities.GetTriggerFaces(level, triggerTypes, false);
    }

    protected override List<TRFace> CollectDeathFaces(TR3Level level)
    {
        return FaceUtilities.GetTriggerFaces(level, new(), true);
    }

    protected override Dictionary<ushort, TRTextileRegion> CreateSpecialSegments(TR3Level level, Pen pen)
    {
        Dictionary<ushort, TRTextileRegion> segments = new();
        foreach (SpecialTextureHandling special in _data.SpecialTextures)
        {
            switch (special.Type)
            {
                case SpecialTextureType.CrashPads:
                    foreach (ushort texture in special.Textures)
                    {
                        if (CreateCrashPad(pen, special.Mode) is TRTextileRegion segment)
                        {
                            segments[texture] = segment;
                        }
                    }
                    break;
            }
        }

        return segments;
    }

    private TRTextileRegion CreateCrashPad(Pen pen, SpecialTextureMode mode)
    {
        const int width = 64;
        const int height = 64;

        TRTextileSegment segment = CreateSegment(new Rectangle(0, 0, width, height));
        TRImage frame = CreateFrame(width, height, pen, SmoothingMode.AntiAlias, true);
        using Bitmap bmp = frame.ToBitmap();
        using Graphics graphics = Graphics.FromImage(bmp);

        switch (mode)
        {
            case SpecialTextureMode.CrashPadCircle:
                graphics.FillEllipse(pen.Brush, new Rectangle(9, 9, 46, 46));
                break;
            case SpecialTextureMode.CrashPadDiamond:
                graphics.FillPolygon(pen.Brush, new Point[]
                {
                    new(32, 16),
                    new(48, 32),
                    new(32, 48),
                    new(16, 32),
                });
                break;
        }

        return new(segment, new(bmp));
    }
}
