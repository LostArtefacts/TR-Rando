using System;
using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TRRandomizerCore.Helpers
{
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
        private static readonly Dictionary<SGFlags, int> _sgSizes = new Dictionary<SGFlags, int>
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

        private static readonly Dictionary<TR2Entities, int> _extraSizes = new Dictionary<TR2Entities, int>
        {
            [TR2Entities.RedSnowmobile]
                = 4 * sizeof(int) + 3 * sizeof(short),
            [TR2Entities.Boat]
                = 5 * sizeof(int) + 2 * sizeof(short),
            [TR2Entities.Elevator]
                = 2 * sizeof(int),
            [TR2Entities.Lara]
                // Assume Lara has an active weapon, and that 5 flares are also active
                = 5 * sizeof(short)
                + 5 * (_sgSizes[SGFlags.Position] + sizeof(int))
        };

        public bool Test(TR2Level level)
        {
            return EstimateSGSize(level) <= _maxSize;
        }

        public int EstimateSGSize(TR2Level level)
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
            List<TR2Entity> entities = level.Entities.ToList();
            foreach (TR2Entity entity in level.Entities)
            {
                TR2Entities type = (TR2Entities)entity.TypeID;
                if (type == TR2Entities.MercSnowmobDriver)
                {
                    entities.Add(new TR2Entity { TypeID = (short)TR2Entities.BlackSnowmob });
                }
                else if (type == TR2Entities.MarcoBartoli)
                {
                    entities.Add(new TR2Entity { TypeID = (short)TR2Entities.DragonFront_H });
                    entities.Add(new TR2Entity { TypeID = (short)TR2Entities.DragonBack_H });
                    entities.Add(new TR2Entity { TypeID = (short)TR2Entities.DragonExplosion1_H });
                    entities.Add(new TR2Entity { TypeID = (short)TR2Entities.DragonExplosion2_H });
                    entities.Add(new TR2Entity { TypeID = (short)TR2Entities.DragonExplosion3_H });
                }
                else if (type == TR2Entities.Knifethrower)
                {
                    // Assume each knifethrower has 2 active knives
                    for (int i = 0; i < 2; i++)
                    {
                        entities.Add(new TR2Entity { TypeID = (short)TR2Entities.KnifeProjectile_H });
                    }
                }
                else if (type == TR2Entities.ScubaDiver)
                {
                    // Assume every Steve has fired 3 harpoons
                    for (int i = 0; i < 3; i++)
                    {
                        entities.Add(new TR2Entity { TypeID = (short)TR2Entities.ScubaHarpoonProjectile_H });
                    }
                }
                else if (type == TR2Entities.Lara)
                {
                    // Assume Lara has fired a grenade and 3 harpoons (imposssible, probably, but we are trying to be cautious)
                    entities.Add(new TR2Entity { TypeID = (short)TR2Entities.GrenadeProjectile_H });
                    for (int i = 0; i < 3; i++)
                    {
                        entities.Add(new TR2Entity { TypeID = (short)TR2Entities.HarpoonProjectile_H });
                    }
                }
            }

            foreach (TR2Entity entity in entities)
            {
                TR2Entities type = (TR2Entities)entity.TypeID;
                SGFlags saveFlags = SGFlags.None;
                
                if (TR2EntityUtilities.IsEnemyType(type))
                {
                    switch (type)
                    {
                        case TR2Entities.MarcoBartoli:
                            saveFlags = SGFlags.Animation | SGFlags.Flags;
                            break;
                        case TR2Entities.BlackMorayEel:
                        case TR2Entities.YellowMorayEel:
                            saveFlags = SGFlags.Animation | SGFlags.Flags | SGFlags.Hitpoints;
                            break;
                        case TR2Entities.Winston:
                            saveFlags = SGFlags.Animation | SGFlags.Flags | SGFlags.Position | SGFlags.Intelligence;
                            break;
                        default:
                            saveFlags = SGFlags.Animation | SGFlags.Flags | SGFlags.Hitpoints | SGFlags.Intelligence | SGFlags.Position;
                            break;
                    }
                }
                else if (TR2EntityUtilities.IsAnyPickupType(type))
                {
                    saveFlags = SGFlags.Flags | SGFlags.Position;
                }
                else
                {
                    switch (type)
                    {
                        case TR2Entities.KnifeProjectile_H:
                        case TR2Entities.ScubaHarpoonProjectile_H:
                        case TR2Entities.HarpoonProjectile_H:
                        case TR2Entities.GrenadeProjectile_H:
                            saveFlags = SGFlags.Position;
                            break;

                        case TR2Entities.Monk:
                        case TR2Entities.AquaticMine:
                        case TR2Entities.FlameEmitter_N:
                        case TR2Entities.LavaAirParticleEmitter_N:
                        case TR2Entities.Keyhole1:
                        case TR2Entities.Keyhole2:
                        case TR2Entities.Keyhole3:
                        case TR2Entities.Keyhole4:
                        case TR2Entities.PuzzleHole1:
                        case TR2Entities.PuzzleHole2:
                        case TR2Entities.PuzzleHole3:
                        case TR2Entities.PuzzleHole4:
                        case TR2Entities.PuzzleDone1:
                        case TR2Entities.PuzzleDone2:
                        case TR2Entities.PuzzleDone3:
                        case TR2Entities.PuzzleDone4:
                        case TR2Entities.Alarm_N:
                        case TR2Entities.AlarmBell_N:
                        case TR2Entities.BartoliHideoutClock_N:
                        case TR2Entities.LaraCutscenePlacement_N:
                        case TR2Entities.DragonExplosionEmitter_N:
                        case TR2Entities.Discgun:
                            saveFlags = SGFlags.Flags;
                            break;

                        case TR2Entities.Gondola:
                        case TR2Entities.Helicopter:
                        case TR2Entities.Minisub:
                        case TR2Entities.OverheadPulleyHook:
                        case TR2Entities.PowerSaw:
                        case TR2Entities.UnderwaterPropeller:
                        case TR2Entities.AirFan:
                        case TR2Entities.AirplanePropeller:
                        case TR2Entities.WallMountedKnifeBlade:
                        case TR2Entities.BouncePad:
                        case TR2Entities.SandbagOrBallsack:
                        case TR2Entities.SwingingBoxOrBall:
                        case TR2Entities.SlammingDoor:
                        case TR2Entities.LavaBowl:
                        case TR2Entities.TibetanBell:
                        case TR2Entities.BreakableWindow1:
                        case TR2Entities.BreakableWindow2:
                        case TR2Entities.Drawbridge:
                        case TR2Entities.SmallWallSwitch:
                        case TR2Entities.PushButtonSwitch:
                        case TR2Entities.WheelKnob:
                        case TR2Entities.WallSwitch:
                        case TR2Entities.UnderwaterSwitch:
                        case TR2Entities.Door1:
                        case TR2Entities.Door2:
                        case TR2Entities.Door3:
                        case TR2Entities.Door4:
                        case TR2Entities.Door5:
                        case TR2Entities.LiftingDoor1:
                        case TR2Entities.LiftingDoor2:
                        case TR2Entities.LiftingDoor3:
                        case TR2Entities.Trapdoor1:
                        case TR2Entities.Trapdoor2:
                        case TR2Entities.Gong:
                        case TR2Entities.ShotgunShowerAnimation_H:
                        case TR2Entities.DetonatorBox:
                            saveFlags = SGFlags.Animation | SGFlags.Flags;
                            break;

                        case TR2Entities.Helicopter2:
                        case TR2Entities.SpikyWall:
                        case TR2Entities.SpikyCeiling:
                        case TR2Entities.DragonExplosion1_H:
                        case TR2Entities.DragonExplosion2_H:
                        case TR2Entities.DragonExplosion3_H:
                            saveFlags = SGFlags.Flags | SGFlags.Position;
                            break;

                        case TR2Entities.ZiplineHandle:
                        case TR2Entities.RollingSpindle:
                        case TR2Entities.DetatchableIcicles:
                        case TR2Entities.StatueWithKnifeBlade:
                        case TR2Entities.FallingBlock:
                        case TR2Entities.FallingBlock2:
                        case TR2Entities.LooseBoards:
                        case TR2Entities.BouldersOrSnowballs:
                        case TR2Entities.RollingStorageDrums:
                        case TR2Entities.RollingBall:
                        case TR2Entities.FallingCeilingOrSandbag:
                        case TR2Entities.RedSnowmobile:
                        case TR2Entities.Boat:
                        case TR2Entities.Elevator:
                        case TR2Entities.BlackSnowmob:
                        case TR2Entities.DragonBack_H:
                        case TR2Entities.PushBlock1:
                        case TR2Entities.PushBlock2:
                        case TR2Entities.PushBlock3:
                        case TR2Entities.PushBlock4:
                            saveFlags = SGFlags.Animation | SGFlags.Flags | SGFlags.Position;
                            break;

                        case TR2Entities.Lara:
                            saveFlags = SGFlags.Animation | SGFlags.Flags | SGFlags.Hitpoints | SGFlags.Position;
                            break;

                        case TR2Entities.DragonFront_H:
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
}