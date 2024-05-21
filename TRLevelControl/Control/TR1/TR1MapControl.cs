using TRLevelControl.Model;

namespace TRLevelControl;

public class TR1MapControl : TRMapControlBase<TR1Type, TR1RAlias>
{
    protected override TR1Type ReadKey(string key)
        => _keyToTypeMap.ContainsKey(key) ? _keyToTypeMap[key] : default;

    protected override string ConvertKey(TR1Type key)
        => _typeToKeyMap[key];

    private static readonly Dictionary<string, TR1Type> _keyToTypeMap = new()
    {
        ["DUMMY"] = default,
        ["BRIDGE_FLAT"] = TR1Type.BridgeFlat,
        ["BRIDGE_TILT1"] = TR1Type.BridgeTilt1,
        ["BRIDGE_TILT2"] = TR1Type.BridgeTilt2,
        ["DART_EMITTER"] = TR1Type.DartEmitter,
        ["DARTS"] = TR1Type.Dart_H,
        ["DOOR_TYPE1"] = TR1Type.Door1,
        ["DOOR_TYPE2"] = TR1Type.Door2,
        ["DOOR_TYPE3"] = TR1Type.Door3,
        ["DOOR_TYPE4"] = TR1Type.Door4,
        ["DOOR_TYPE5"] = TR1Type.Door5,
        ["DOOR_TYPE6"] = TR1Type.Door6,
        ["DOOR_TYPE7"] = TR1Type.Door7,
        ["DOOR_TYPE8"] = TR1Type.Door8,
        ["FALLING_BLOCK"] = TR1Type.FallingBlock,
        ["FALLING_CEILING1"] = TR1Type.FallingCeiling1,
        ["FALLING_CEILING2"] = TR1Type.FallingCeiling2,
        ["KEY_HOLE1"] = TR1Type.Keyhole1,
        ["KEY_HOLE2"] = TR1Type.Keyhole2,
        ["KEY_HOLE3"] = TR1Type.Keyhole3,
        ["KEY_HOLE4"] = TR1Type.Keyhole4,
        ["KEY_OPTION1"] = TR1Type.Key1_M_H,
        ["KEY_OPTION2"] = TR1Type.Key2_M_H,
        ["KEY_OPTION3"] = TR1Type.Key3_M_H,
        ["KEY_OPTION4"] = TR1Type.Key4_M_H,
        ["LARSON"] = TR1Type.Larson,
        ["LAVA_WEDGE"] = TR1Type.AtlanteanLava,
        ["LIGHTNING_EMITTER"] = TR1Type.ThorLightning,
        ["MOVABLE_BLOCK"] = TR1Type.PushBlock1,
        ["MOVABLE_BLOCK2"] = TR1Type.PushBlock2,
        ["MOVABLE_BLOCK3"] = TR1Type.PushBlock3,
        ["MOVABLE_BLOCK4"] = TR1Type.PushBlock4,
        ["MOVING_BAR"] = TR1Type.Barricade,
        ["NATLA"] = TR1Type.Natla,
        ["PASSPORT_OPTION"] = TR1Type.PassportOpen_M_H,
        ["PENDULUM"] = TR1Type.SwingingBlade,
        ["PLAYER_1"] = TR1Type.CutsceneActor1,
        ["PLAYER_2"] = TR1Type.CutsceneActor2,
        ["PLAYER_3"] = TR1Type.CutsceneActor3,
        ["PLAYER_4"] = TR1Type.CutsceneActor4,
        ["PODS"] = TR1Type.AtlanteanEgg,
        ["PUZZLE_DONE1"] = TR1Type.PuzzleDone1,
        ["PUZZLE_DONE2"] = TR1Type.PuzzleDone2,
        ["PUZZLE_DONE3"] = TR1Type.PuzzleDone3,
        ["PUZZLE_DONE4"] = TR1Type.PuzzleDone4,
        ["PUZZLE_HOLE1"] = TR1Type.PuzzleHole1,
        ["PUZZLE_HOLE2"] = TR1Type.PuzzleHole2,
        ["PUZZLE_HOLE3"] = TR1Type.PuzzleHole3,
        ["PUZZLE_HOLE4"] = TR1Type.PuzzleHole4,
        ["PUZZLE_OPTION1"] = TR1Type.Puzzle1_M_H,
        ["PUZZLE_OPTION2"] = TR1Type.Puzzle2_M_H,
        ["PUZZLE_OPTION3"] = TR1Type.Puzzle3_M_H,
        ["PUZZLE_OPTION4"] = TR1Type.Puzzle4_M_H,
        ["ROLLING_BALL"] = TR1Type.RollingBall,
        ["SWITCH_TYPE1"] = TR1Type.WallSwitch,
        ["SWITCH_TYPE2"] = TR1Type.UnderwaterSwitch,
        ["TEETH_TRAP"] = TR1Type.SlammingDoor,
        ["TRAPDOOR"] = TR1Type.Trapdoor1,
        ["TRAPDOOR2"] = TR1Type.Trapdoor2,
        ["WARRIOR3"] = TR1Type.NonShootingAtlantean_N,
    };

    private static readonly Dictionary<TR1Type, string> _typeToKeyMap;

    static TR1MapControl()
    {
        _typeToKeyMap = _keyToTypeMap.ToDictionary(e => e.Value, e => e.Key);
    }
}
