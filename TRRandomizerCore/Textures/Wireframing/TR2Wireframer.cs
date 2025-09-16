using System.Drawing;
using TRImageControl;
using TRImageControl.Packing;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
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

    private static readonly List<TR2Type> _pickupModels =
    [
        TR2Type.Pistols_M_H, TR2Type.Shotgun_M_H, TR2Type.Autos_M_H, TR2Type.Uzi_M_H, TR2Type.Harpoon_M_H,
        TR2Type.M16_M_H, TR2Type.GrenadeLauncher_M_H, TR2Type.ShotgunAmmo_M_H, TR2Type.AutoAmmo_M_H, TR2Type.UziAmmo_M_H,
        TR2Type.HarpoonAmmo_M_H, TR2Type.M16Ammo_M_H, TR2Type.Grenades_M_H, TR2Type.SmallMed_M_H, TR2Type.LargeMed_M_H,
        TR2Type.Flares_M_H, TR2Type.Puzzle1_M_H, TR2Type.Puzzle2_M_H, TR2Type.Puzzle3_M_H, TR2Type.Puzzle4_M_H,
        TR2Type.Key1_M_H, TR2Type.Key2_M_H, TR2Type.Key3_M_H, TR2Type.Key4_M_H, TR2Type.Quest1_M_H, TR2Type.Quest2_M_H,
    ];

    private static readonly List<TR2Type> _inventoryTypes =
    [
        TR2Type.Sunglasses_M_H, TR2Type.Map_M_U, TR2Type.DirectionKeys_M_H, TR2Type.PassportOpen_M_H, TR2Type.PassportClosed_M_H,
        TR2Type.CDPlayer_M_H, TR2Type.Stopwatch_M_H,
    ];

    private TRPalette16Control _paletteTracker;

    protected override TRTexturePacker CreatePacker(TR2Level level)
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
            || TR2TypeUtilities.IsPushblockType(type)
            || TR2TypeUtilities.IsVehicleType(type) || type == TR2Type.SnowmobileBelt
            || _inventoryTypes.Contains(type);
    }

    protected override bool ShouldSolidifyModel(TR2Type type)
    {
        return _data.Has3DPickups && _pickupModels.Contains(type);
    }

    protected override int GetBlackPaletteIndex(TR2Level level)
    {
        return level.Palette16.ToList().FindIndex(c => c.Red + c.Green + c.Blue == 0);
    }

    protected override TRDictionary<TR2Type, TRModel> GetModels(TR2Level level)
    {
        return level.Models;
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

    protected override void ResetPaletteTracking(TR2Level level)
    {
        _paletteTracker = null;
    }

    protected override bool IsLaraModel(TR2Type type)
    {
        return _laraEntities.Contains(type);
    }

    protected override bool IsEnemyModel(TR2Type type)
    {
        return TR2TypeUtilities.IsEnemyType(type) || _additionalEnemyEntities.Contains(type);
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
}
