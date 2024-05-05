using System.Drawing;
using TRImageControl.Helpers;
using TRImageControl.Packing;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRModelTransporter.Helpers;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Textures;

public class TR2Wireframer : AbstractTRWireframer<TR2Type, TR2Level>
{
    private static readonly List<TR2Type> _laraEntities = new()
    {
        TR2Type.Lara, TR2Type.LaraPonytail_H, TR2Type.LaraFlareAnim_H,
        TR2Type.LaraPistolAnim_H, TR2Type.LaraShotgunAnim_H, TR2Type.LaraAutoAnim_H,
        TR2Type.LaraUziAnim_H, TR2Type.LaraM16Anim_H, TR2Type.LaraHarpoonAnim_H,
        TR2Type.LaraGrenadeAnim_H, TR2Type.LaraMiscAnim_H, TR2Type.CameraTarget_N,
        TR2Type.FlameEmitter_N, TR2Type.LaraCutscenePlacement_N, TR2Type.DragonExplosionEmitter_N,
        TR2Type.BartoliHideoutClock_N, TR2Type.SingingBirds_N, TR2Type.WaterfallMist_N,
        TR2Type.DrippingWater_N, TR2Type.LavaAirParticleEmitter_N, TR2Type.AlarmBell_N, TR2Type.DoorBell_N
    };

    private static readonly List<TR2Type> _additionalEnemyEntities = new()
    {
        TR2Type.DragonFront_H, TR2Type.DragonBack_H, TR2Type.XianGuardSpearStatue, TR2Type.XianGuardSwordStatue
    };

    private TRPalette16Control _paletteTracker;

    protected override AbstractTexturePacker<TR2Type, TR2Level> CreatePacker(TR2Level level)
    {
        return new TR2TexturePacker(level);
    }

    protected override bool IsSkybox(TR2Type type)
    {
        return type == TR2Type.Skybox_H;
    }

    protected override bool IsInteractableModel(TR2Type type)
    {
        return TR2TypeUtilities.IsSwitchType(type)
            || TR2TypeUtilities.IsKeyholeType(type)
            || TR2TypeUtilities.IsSlotType(type)
            || TR2TypeUtilities.IsPushblockType(type);
    }

    protected override int GetBlackPaletteIndex(TR2Level level)
    {
        return level.Palette16.ToList().FindIndex(c => c.Red + c.Green + c.Blue == 0);
    }

    protected override IEnumerable<int> GetInvalidObjectTextureIndices(TR2Level level)
    {
        return level.GetInvalidObjectTextureIndices();
    }

    protected override TRDictionary<TR2Type, TRModel> GetModels(TR2Level level)
    {
        return level.Models;
    }

    protected override List<TRObjectTexture> GetObjectTextures(TR2Level level)
    {
        return level.ObjectTextures;
    }

    protected override IEnumerable<IEnumerable<TRFace>> GetRoomFace3s(TR2Level level)
    {
        List<List<TRFace>> faces = new();
        foreach (TR2Room room in level.Rooms)
        {
            faces.Add(room.Mesh.Triangles);
        }
        return faces;
    }

    protected override IEnumerable<IEnumerable<TRFace>> GetRoomFace4s(TR2Level level)
    {
        List<List<TRFace>> faces = new();
        foreach (TR2Room room in level.Rooms)
        {
            faces.Add(room.Mesh.Rectangles);
        }
        return faces;
    }

    protected override int ImportColour(TR2Level level, Color c)
    {
        _paletteTracker ??= new(level);
        return _paletteTracker.Import(c);
    }

    protected override bool IsLaraModel(TR2Type type)
    {
        return _laraEntities.Contains(type);
    }

    protected override bool IsEnemyModel(TR2Type type)
    {
        return TR2TypeUtilities.IsEnemyType(type) || _additionalEnemyEntities.Contains(type);
    }

    protected override void ResetUnusedTextures(TR2Level level)
    {
        level.ResetUnusedTextures();
    }

    protected override void SetSkyboxVisible(TR2Level level)
    {
        foreach (TR2Room room in level.Rooms)
        {
            room.IsSkyboxVisible = true;
        }
    }

    protected override Dictionary<TRFace, List<TRVertex>> CollectLadders(TR2Level level)
    {
        return FaceUtilities.GetClimbableFaces(level);
    }

    protected override List<TRFace> CollectTriggerFaces(TR2Level level, List<FDTrigType> triggerTypes)
    {
        return FaceUtilities.GetTriggerFaces(level, triggerTypes, false);
    }

    protected override List<TRFace> CollectDeathFaces(TR2Level level)
    {
        return FaceUtilities.GetTriggerFaces(level, new(), true);
    }

    protected override List<TRAnimatedTexture> GetAnimatedTextures(TR2Level level)
    {
        return level.AnimatedTextures;
    }
}
