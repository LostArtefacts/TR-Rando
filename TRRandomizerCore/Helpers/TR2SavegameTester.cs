using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRRandomizerCore.Helpers;

[Flags]
public enum SGFlags
{
    None = 1,
    Animation = 2,
    Flags = 4,
    Hitpoints = 8,
    Intelligence = 16,
    Position = 32
}

public class TR2SavegameTester
{
    private static readonly Dictionary<SGFlags, int> _sgSizes = new()
    {
        [SGFlags.Animation]
            = 5 * sizeof(short), // CurrentAnim,GoalAnim,RequiredAnim,AnumNum,FrameNum
        [SGFlags.Flags]
            = 2 * sizeof(short), // BasicFlags,Timer
        [SGFlags.Hitpoints]
            = sizeof(short),     // Just hitpoints
        [SGFlags.Position]
            = 3 * sizeof(int)    // X,Y,Z
            + 6 * sizeof(short), // XRot,YRot,ZRot,Room,Speed,Fallspeed
        [SGFlags.Intelligence]
            = 5 * sizeof(short)  // CarriedItem,HeadRot,NeckRot,MaxTurn,Flags
            + sizeof(int)        // Mood
    };

    private static readonly int _maxSize = 6272;

    // Lara and other general SG elements
    private static readonly int _laraSize = 722; // From Lara struct
    private static readonly int _cameraSize = sizeof(short);
    private static readonly int _flipStatus = sizeof(int);
    private static readonly int _flipEffect = sizeof(int);
    private static readonly int _flipTimer = sizeof(int);
    private static readonly int _monksAttack = sizeof(int);
    private static readonly int _flareCount = sizeof(int);
    private static readonly int _flipmapStatus = 10 * sizeof(sbyte);
    private static readonly int _cdTrackStatus = 64 * sizeof(short);

    private static readonly Dictionary<TR2Type, int> _extraSizes = new()
    {
        [TR2Type.RedSnowmobile]
            = 4 * sizeof(int) + 3 * sizeof(short),
        [TR2Type.Boat]
            = 5 * sizeof(int) + 2 * sizeof(short),
        [TR2Type.Elevator]
            = 2 * sizeof(int),
        [TR2Type.Lara]
            // Assume Lara has an active weapon, and that 5 flares are also active
            = 5 * sizeof(short)
            + 5 * (_sgSizes[SGFlags.Position] + sizeof(int))
    };

    public static bool Test(TR2Level level)
    {
        return EstimateSGSize(level) <= _maxSize;
    }

    public static int EstimateSGSize(TR2Level level)
    {
        int size = 0;

        size += _laraSize;

        size += _flipEffect;
        size += _flipTimer;
        size += _monksAttack;
        size += _flareCount;

        size += _flipStatus;
        size += _flipmapStatus;
        size += _cdTrackStatus;

        size += (int)level.NumCameras * _cameraSize;

        // Expand the entity list to simulate everything that could actually be present
        List<TR2Entity> entities = new(level.Entities);
        foreach (TR2Entity entity in level.Entities)
        {
            TR2Type type = (TR2Type)entity.TypeID;
            if (type == TR2Type.MercSnowmobDriver)
            {
                entities.Add(new TR2Entity { TypeID = (short)TR2Type.BlackSnowmob });
            }
            else if (type == TR2Type.MarcoBartoli)
            {
                entities.Add(new TR2Entity { TypeID = (short)TR2Type.DragonFront_H });
                entities.Add(new TR2Entity { TypeID = (short)TR2Type.DragonBack_H });
                entities.Add(new TR2Entity { TypeID = (short)TR2Type.DragonExplosion1_H });
                entities.Add(new TR2Entity { TypeID = (short)TR2Type.DragonExplosion2_H });
                entities.Add(new TR2Entity { TypeID = (short)TR2Type.DragonExplosion3_H });
            }
            else if (type == TR2Type.Knifethrower)
            {
                // Assume each knifethrower has 2 active knives
                for (int i = 0; i < 2; i++)
                {
                    entities.Add(new TR2Entity { TypeID = (short)TR2Type.KnifeProjectile_H });
                }
            }
            else if (type == TR2Type.ScubaDiver)
            {
                // Assume every Steve has fired 3 harpoons
                for (int i = 0; i < 3; i++)
                {
                    entities.Add(new TR2Entity { TypeID = (short)TR2Type.ScubaHarpoonProjectile_H });
                }
            }
            else if (type == TR2Type.Lara)
            {
                // Assume Lara has fired a grenade and 3 harpoons (imposssible, probably, but we are trying to be cautious)
                entities.Add(new TR2Entity { TypeID = (short)TR2Type.GrenadeProjectile_H });
                for (int i = 0; i < 3; i++)
                {
                    entities.Add(new TR2Entity { TypeID = (short)TR2Type.HarpoonProjectile_H });
                }
            }
        }

        foreach (TR2Entity entity in entities)
        {
            TR2Type type = (TR2Type)entity.TypeID;
            SGFlags saveFlags = SGFlags.None;
            
            if (TR2TypeUtilities.IsEnemyType(type))
            {
                saveFlags = type switch
                {
                    TR2Type.MarcoBartoli => SGFlags.Animation | SGFlags.Flags,
                    TR2Type.BlackMorayEel
                    or TR2Type.YellowMorayEel => SGFlags.Animation | SGFlags.Flags | SGFlags.Hitpoints,
                    TR2Type.Winston => SGFlags.Animation | SGFlags.Flags | SGFlags.Position | SGFlags.Intelligence,
                    _ => SGFlags.Animation | SGFlags.Flags | SGFlags.Hitpoints | SGFlags.Intelligence | SGFlags.Position,
                };
            }
            else if (TR2TypeUtilities.IsAnyPickupType(type))
            {
                saveFlags = SGFlags.Flags | SGFlags.Position;
            }
            else
            {
                switch (type)
                {
                    case TR2Type.KnifeProjectile_H:
                    case TR2Type.ScubaHarpoonProjectile_H:
                    case TR2Type.HarpoonProjectile_H:
                    case TR2Type.GrenadeProjectile_H:
                        saveFlags = SGFlags.Position;
                        break;

                    case TR2Type.Monk:
                    case TR2Type.AquaticMine:
                    case TR2Type.FlameEmitter_N:
                    case TR2Type.LavaAirParticleEmitter_N:
                    case TR2Type.Keyhole1:
                    case TR2Type.Keyhole2:
                    case TR2Type.Keyhole3:
                    case TR2Type.Keyhole4:
                    case TR2Type.PuzzleHole1:
                    case TR2Type.PuzzleHole2:
                    case TR2Type.PuzzleHole3:
                    case TR2Type.PuzzleHole4:
                    case TR2Type.PuzzleDone1:
                    case TR2Type.PuzzleDone2:
                    case TR2Type.PuzzleDone3:
                    case TR2Type.PuzzleDone4:
                    case TR2Type.Alarm_N:
                    case TR2Type.AlarmBell_N:
                    case TR2Type.BartoliHideoutClock_N:
                    case TR2Type.LaraCutscenePlacement_N:
                    case TR2Type.DragonExplosionEmitter_N:
                    case TR2Type.Discgun:
                        saveFlags = SGFlags.Flags;
                        break;

                    case TR2Type.Gondola:
                    case TR2Type.Helicopter:
                    case TR2Type.Minisub:
                    case TR2Type.OverheadPulleyHook:
                    case TR2Type.PowerSaw:
                    case TR2Type.UnderwaterPropeller:
                    case TR2Type.AirFan:
                    case TR2Type.AirplanePropeller:
                    case TR2Type.WallMountedKnifeBlade:
                    case TR2Type.BouncePad:
                    case TR2Type.SandbagOrBallsack:
                    case TR2Type.SwingingBoxOrBall:
                    case TR2Type.SlammingDoor:
                    case TR2Type.LavaBowl:
                    case TR2Type.TibetanBell:
                    case TR2Type.BreakableWindow1:
                    case TR2Type.BreakableWindow2:
                    case TR2Type.Drawbridge:
                    case TR2Type.SmallWallSwitch:
                    case TR2Type.PushButtonSwitch:
                    case TR2Type.WheelKnob:
                    case TR2Type.WallSwitch:
                    case TR2Type.UnderwaterSwitch:
                    case TR2Type.Door1:
                    case TR2Type.Door2:
                    case TR2Type.Door3:
                    case TR2Type.Door4:
                    case TR2Type.Door5:
                    case TR2Type.LiftingDoor1:
                    case TR2Type.LiftingDoor2:
                    case TR2Type.LiftingDoor3:
                    case TR2Type.Trapdoor1:
                    case TR2Type.Trapdoor2:
                    case TR2Type.Gong:
                    case TR2Type.ShotgunShowerAnimation_H:
                    case TR2Type.DetonatorBox:
                        saveFlags = SGFlags.Animation | SGFlags.Flags;
                        break;

                    case TR2Type.Helicopter2:
                    case TR2Type.SpikyWall:
                    case TR2Type.SpikyCeiling:
                    case TR2Type.DragonExplosion1_H:
                    case TR2Type.DragonExplosion2_H:
                    case TR2Type.DragonExplosion3_H:
                        saveFlags = SGFlags.Flags | SGFlags.Position;
                        break;

                    case TR2Type.ZiplineHandle:
                    case TR2Type.RollingSpindle:
                    case TR2Type.DetatchableIcicles:
                    case TR2Type.StatueWithKnifeBlade:
                    case TR2Type.FallingBlock:
                    case TR2Type.FallingBlock2:
                    case TR2Type.LooseBoards:
                    case TR2Type.BouldersOrSnowballs:
                    case TR2Type.RollingStorageDrums:
                    case TR2Type.RollingBall:
                    case TR2Type.FallingCeilingOrSandbag:
                    case TR2Type.RedSnowmobile:
                    case TR2Type.Boat:
                    case TR2Type.Elevator:
                    case TR2Type.BlackSnowmob:
                    case TR2Type.DragonBack_H:
                    case TR2Type.PushBlock1:
                    case TR2Type.PushBlock2:
                    case TR2Type.PushBlock3:
                    case TR2Type.PushBlock4:
                        saveFlags = SGFlags.Animation | SGFlags.Flags | SGFlags.Position;
                        break;

                    case TR2Type.Lara:
                        saveFlags = SGFlags.Animation | SGFlags.Flags | SGFlags.Hitpoints | SGFlags.Position;
                        break;

                    case TR2Type.DragonFront_H:
                        saveFlags = SGFlags.Animation | SGFlags.Flags | SGFlags.Hitpoints | SGFlags.Intelligence | SGFlags.Position;
                        break;

                    default:
                        // Nothing saved for null meshes, stationary spikes, bridges
                        break;
                }

                if (_extraSizes.ContainsKey(type))
                {
                    size += _extraSizes[type];
                }
            }

            if (saveFlags != SGFlags.None)
            {
                foreach (SGFlags flag in _sgSizes.Keys)
                {
                    if ((saveFlags & flag) > 0)
                    {
                        size += _sgSizes[flag];
                    }
                }
            }
        }

        return size;
    }
}
