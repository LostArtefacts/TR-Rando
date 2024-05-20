using TRLevelControl.Model;

namespace TRLevelControl;

public class TR2MapControl : TRMapControlBase<TR2Type, TR2RAlias>
{
    protected override TR2Type ReadKey(string key)
        => _keyToTypeMap.ContainsKey(key) ? _keyToTypeMap[key] : default;

    protected override string ConvertKey(TR2Type key)
        => _typeToKeyMap[key];

    private static readonly Dictionary<string, TR2Type> _keyToTypeMap = new()
    {
        ["DUMMY"] = default,
        ["ALARM_SOUND"] = TR2Type.DoorBell_N,
        ["AVALANCHE"] = TR2Type.BouldersOrSnowballs,
        ["BANDIT2"] = TR2Type.Mercenary2,
        ["BANDIT2B"] = TR2Type.Mercenary3,
        ["BANDIT3"] = TR2Type.Mercenary1,
        ["BARACUDDA"] = TR2Type.Barracuda,
        ["BIG_SPIDER"] = TR2Type.GiantSpider,
        ["BIG_YETI"] = TR2Type.BirdMonster,
        ["BLADE"] = TR2Type.WallMountedKnifeBlade,
        ["BRIDGE_FLAT"] = TR2Type.BridgeFlat,
        ["BRIDGE_TILT1"] = TR2Type.BridgeTilt1,
        ["BRIDGE_TILT2"] = TR2Type.BridgeTilt2,
        ["CEILING_SPIKES"] = TR2Type.SpikyCeiling,
        ["COPTER"] = TR2Type.Helicopter,
        ["CULT3"] = TR2Type.ShotgunGoon,
        ["DOOR_TYPE1"] = TR2Type.Door1,
        ["DOOR_TYPE2"] = TR2Type.Door2,
        ["DOOR_TYPE3"] = TR2Type.Door3,
        ["DOOR_TYPE4"] = TR2Type.Door4,
        ["DOOR_TYPE5"] = TR2Type.Door5,
        ["DOOR_TYPE6"] = TR2Type.LiftingDoor1,
        ["DOOR_TYPE7"] = TR2Type.LiftingDoor2,
        ["DOOR_TYPE8"] = TR2Type.LiftingDoor3,
        ["DRAW_BRIDGE"] = TR2Type.Drawbridge,
        ["EAGLE"] = TR2Type.Eagle,
        ["FALLING_BLOCK"] = TR2Type.FallingBlock,
        ["FALLING_CEILING1"] = TR2Type.FallingCeilingOrSandbag,
        ["HARPOON_BOLT"] = TR2Type.HarpoonProjectile_H,
        ["KEY_HOLE1"] = TR2Type.Keyhole1,
        ["KEY_HOLE2"] = TR2Type.Keyhole2,
        ["KEY_HOLE3"] = TR2Type.Keyhole3,
        ["KEY_HOLE4"] = TR2Type.Keyhole4,
        ["KEY_OPTION1"] = TR2Type.Key1_M_H,
        ["KEY_OPTION2"] = TR2Type.Key2_M_H,
        ["KEY_OPTION3"] = TR2Type.Key3_M_H,
        ["KEY_OPTION4"] = TR2Type.Key4_M_H,
        ["KILLER_STATUE"] = TR2Type.StatueWithKnifeBlade,
        ["MESHSWAP1"] = TR2Type.CutsceneActor1,
        ["MESHSWAP2"] = TR2Type.CutsceneActor2,
        ["MINI_COPTER"] = TR2Type.Helicopter2,
        ["MONK1"] = TR2Type.MonkWithLongStick,
        ["MONK2"] = TR2Type.MonkWithKnifeStick,
        ["MOVABLE_BLOCK"] = TR2Type.PushBlock1,
        ["MOVABLE_BLOCK2"] = TR2Type.PushBlock2,
        ["MOVABLE_BLOCK3"] = TR2Type.PushBlock3,
        ["MOVABLE_BLOCK4"] = TR2Type.PushBlock4,
        ["OILDRUMS"] = TR2Type.RollingStorageDrums,
        ["PASSPORT_OPTION"] = TR2Type.PassportOpen_M_H,
        ["PENDULUM"] = TR2Type.SandbagOrBallsack,
        ["PICKUP_OPTION1"] = TR2Type.Quest1_M_H,
        ["PICKUP_OPTION2"] = TR2Type.Quest2_M_H,
        ["PLAYER_1"] = TR2Type.CutsceneActor4,
        ["PLAYER_2"] = TR2Type.CutsceneActor5,
        ["PLAYER_3"] = TR2Type.CutsceneActor6,
        ["PLAYER_5"] = TR2Type.CutsceneActor8,
        ["PLAYER_6"] = TR2Type.CutsceneActor9,
        ["PLAYER_7"] = TR2Type.CutsceneActor10,
        ["PLAYER_8"] = TR2Type.CutsceneActor11,
        ["PLAYER4"] = TR2Type.CutsceneActor7,
        ["PUSH_SWITCH"] = TR2Type.PushButtonSwitch,
        ["PUZZLE_DONE1"] = TR2Type.PuzzleDone1,
        ["PUZZLE_DONE2"] = TR2Type.PuzzleDone2,
        ["PUZZLE_DONE3"] = TR2Type.PuzzleDone3,
        ["PUZZLE_DONE4"] = TR2Type.PuzzleDone4,
        ["PUZZLE_HOLE1"] = TR2Type.PuzzleHole1,
        ["PUZZLE_HOLE2"] = TR2Type.PuzzleHole2,
        ["PUZZLE_HOLE3"] = TR2Type.PuzzleHole3,
        ["PUZZLE_HOLE4"] = TR2Type.PuzzleHole4,
        ["PUZZLE_OPTION1"] = TR2Type.Puzzle1_M_H,
        ["PUZZLE_OPTION2"] = TR2Type.Puzzle2_M_H,
        ["PUZZLE_OPTION3"] = TR2Type.Puzzle3_M_H,
        ["PUZZLE_OPTION4"] = TR2Type.Puzzle4_M_H,
        ["ROCKET"] = TR2Type.GrenadeProjectile_H,
        ["ROLLING_BALL"] = TR2Type.RollingBall,
        ["SHARK"] = TR2Type.Shark,
        ["SKIDMAN"] = TR2Type.MercSnowmobDriver,
        ["SKIDOO"] = TR2Type.RedSnowmobile,
        ["SMASH_ICE"] = TR2Type.BreakableWindow2,
        ["SMASH_WINDOW"] = TR2Type.BreakableWindow1,
        ["SPIDER"] = TR2Type.Spider,
        ["SPIKE_WALL"] = TR2Type.SpikyWall,
        ["SPIKES"] = TR2Type.TeethSpikesOrGlassShards,
        ["SPINNING_BLADE"] = TR2Type.RollingSpindle,
        ["SPRING_BOARD"] = TR2Type.BouncePad,
        ["SWING_BOX"] = TR2Type.SwingingBoxOrBall,
        ["SWITCH_TYPE1"] = TR2Type.WallSwitch,
        ["SWITCH_TYPE2"] = TR2Type.UnderwaterSwitch,
        ["TEETH_TRAP"] = TR2Type.SlammingDoor,
        ["TIGER"] = TR2Type.TigerOrSnowLeopard,
        ["TRAPDOOR"] = TR2Type.Trapdoor1,
        ["TRAPDOOR2"] = TR2Type.Trapdoor2,
        ["WORKER1"] = TR2Type.Gunman1,
        ["WORKER3"] = TR2Type.StickWieldingGoon1,
        ["WORKER4"] = TR2Type.StickWieldingGoon2,
        ["YETI"] = TR2Type.Yeti,
    };

    private static readonly Dictionary<TR2Type, string> _typeToKeyMap;

    static TR2MapControl()
    {
        _typeToKeyMap = _keyToTypeMap.ToDictionary(e => e.Value, e => e.Key);
    }
}
