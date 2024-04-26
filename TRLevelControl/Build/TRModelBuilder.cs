using System.Diagnostics;
using TRLevelControl.Model;

namespace TRLevelControl.Build;

public class TRModelBuilder
{
    private static readonly ushort _tr5ModelPadding = 0xFFEF;

    private readonly TRGameVersion _version;
    private readonly ITRLevelObserver _observer;

    private List<TRAnimation> _animations;
    private List<TRAnimDispatch> _dispatches;
    private List<TRMeshTreeNode> _trees;

    private List<PlaceholderModel> _placeholderModels;
    private List<PlaceholderAnimation> _placeholderAnimations;
    private List<PlaceholderChange> _placeholderChanges;
    private List<short> _commands;
    private List<short> _frames;

    private Dictionary<TRAnimDispatch, short> _dispatchToAnimMap;
    private Dictionary<TRAnimDispatch, short> _dispatchFrameBase;

    public TRModelBuilder(TRGameVersion version, ITRLevelObserver observer = null)
    {
        _version = version;
        _observer = observer;
    }

    public List<TRModel> ReadModelData(TRLevelReader reader)
    {
        ReadAnimations(reader);
        ReadStateChanges(reader);
        ReadDispatches(reader);
        ReadCommands(reader);
        ReadTrees(reader);
        ReadFrames(reader);
        ReadModels(reader);

        List<TRModel> models = new();
        foreach (PlaceholderModel placeholder in _placeholderModels)
        {
            models.Add(BuildModel(placeholder));
        }

        TestTR5Changes(models);

        return models;
    }

    public void WriteModelData(TRLevelWriter writer, List<TRModel> models)
    {
        _placeholderAnimations = new();
        _placeholderChanges = new();
        _placeholderModels = new();
        _commands = new();
        _dispatches = new();
        _frames = new();
        _trees = new();
        _dispatchToAnimMap = new();
        _dispatchFrameBase = new();

        foreach (TRModel model in models)
        {
            DeconstructModel(model);
        }

        RestoreTR5Extras();

        WriteAnimations(writer, models);
        WriteChanges(writer);
        WriteDispatches(writer);
        WriteCommands(writer);
        WriteTrees(writer);
        WriteFrames(writer);
        WriteModels(writer, models);
    }

    private void ReadAnimations(TRLevelReader reader)
    {
        uint numAnimations = reader.ReadUInt32();
        _animations = new();
        _placeholderAnimations = new();

        for (int i = 0; i < numAnimations; i++)
        {
            TRAnimation animation = new();
            PlaceholderAnimation placeholder = new();
            _animations.Add(animation);
            _placeholderAnimations.Add(placeholder);

            placeholder.FrameOffset = reader.ReadUInt32();
            animation.FrameRate = reader.ReadByte();
            placeholder.FrameSize = reader.ReadByte();
            animation.StateID = reader.ReadUInt16();
            animation.Speed = reader.ReadFixed32();
            animation.Accel = reader.ReadFixed32();

            if (_version >= TRGameVersion.TR4)
            {
                animation.SpeedLateral = reader.ReadFixed32();
                animation.AccelLateral = reader.ReadFixed32();
            }

            animation.FrameStart = placeholder.RelFrameStart = reader.ReadUInt16();
            animation.FrameEnd = reader.ReadUInt16();
            animation.NextAnimation = reader.ReadUInt16();
            animation.NextFrame = reader.ReadUInt16();

            placeholder.NumStateChanges = reader.ReadUInt16();
            placeholder.ChangeOffset = reader.ReadUInt16();
            placeholder.NumAnimCommands = reader.ReadUInt16();
            placeholder.AnimCommand = reader.ReadUInt16();
        }
    }

    private void ReadStateChanges(TRLevelReader reader)
    {
        uint numStateChanges = reader.ReadUInt32();
        _placeholderChanges = new();

        for (int i = 0; i < numStateChanges; i++)
        {
            _placeholderChanges.Add(new()
            {
                StateID = reader.ReadUInt16(),
                NumAnimDispatches = reader.ReadUInt16(),
                AnimDispatch = reader.ReadUInt16()
            });
        }
    }

    private void ReadDispatches(TRLevelReader reader)
    {
        uint numAnimDispatches = reader.ReadUInt32();
        _dispatches = new();

        for (int i = 0; i < numAnimDispatches; i++)
        {
            _dispatches.Add(new()
            {
                Low = reader.ReadInt16(),
                High = reader.ReadInt16(),
                NextAnimation = reader.ReadInt16(),
                NextFrame = reader.ReadInt16(),
            });
        }
    }

    private void ReadCommands(TRLevelReader reader)
    {
        uint numAnimCommands = reader.ReadUInt32();
        _commands = new(reader.ReadInt16s(numAnimCommands));
    }

    private void ReadTrees(TRLevelReader reader)
    {
        uint numMeshTrees = reader.ReadUInt32() / sizeof(int);
        _trees = new();

        for (int i = 0; i < numMeshTrees; i++)
        {
            _trees.Add(new()
            {
                Flags = reader.ReadUInt32(),
                OffsetX = reader.ReadInt32(),
                OffsetY = reader.ReadInt32(),
                OffsetZ = reader.ReadInt32(),
            });
        }
    }

    private void ReadFrames(TRLevelReader reader)
    {
        uint numFrames = reader.ReadUInt32();
        _frames = new(reader.ReadInt16s(numFrames));
    }

    private void ReadModels(TRLevelReader reader)
    {
        uint numModels = reader.ReadUInt32();
        _placeholderModels = new();

        for (int i = 0; i < numModels; i++)
        {
            _placeholderModels.Add(new()
            {
                ID = reader.ReadUInt32(),
                NumMeshes = reader.ReadUInt16(),
                StartingMesh = reader.ReadUInt16(),
                MeshTree = reader.ReadUInt32(),
                FrameOffset = reader.ReadUInt32(),
                Animation = reader.ReadUInt16()
            });

            if (_version == TRGameVersion.TR5)
            {
                Debug.Assert(reader.ReadUInt16() == _tr5ModelPadding);
            }
        }

        List<PlaceholderModel> animatedModels = _placeholderModels.FindAll(m => m.Animation != TRConsts.NoAnimation);
        for (int i = 0; i < animatedModels.Count; i++)
        {
            PlaceholderModel model = animatedModels[i];
            int nextOffset = i == animatedModels.Count - 1
                ? _animations.Count
                : animatedModels[i + 1].Animation;
            model.AnimCount = nextOffset - model.Animation;
        }
    }

    private TRModel BuildModel(PlaceholderModel placeholder)
    {
        TRModel model = new()
        {
            // To be eliminated
            ID = placeholder.ID,
            NumMeshes = placeholder.NumMeshes,
            StartingMesh = placeholder.StartingMesh,
        };

        // Everything has a dummy mesh tree, so load one less than the mesh count
        int treePointer = (int)placeholder.MeshTree / sizeof(int);
        for (int i = 0; i < placeholder.NumMeshes - 1; i++)
        {
            model.MeshTrees.Add(_trees[treePointer + i]);
        }

        for (int i = 0; i < placeholder.AnimCount; i++)
        {
            TRAnimation animation = BuildAnimation(placeholder, i);
            model.Animations.Add(animation);
        }

        return model;
    }

    private TRAnimation BuildAnimation(PlaceholderModel placeholderModel, int animIndex)
    {
        int globalAnimIndex = placeholderModel.Animation + animIndex;
        TRAnimation animation = _animations[globalAnimIndex];
        PlaceholderAnimation placeholderAnimation = _placeholderAnimations[globalAnimIndex];

        animation.Changes = BuildStateChanges(placeholderModel, globalAnimIndex);
        animation.Commands = BuildCommands(globalAnimIndex);

        // Keep everything relative to this model. Similar to dispatches, check that link animations are valid.
        if ((animation.NextAnimation - placeholderModel.Animation) >= placeholderModel.AnimCount)
        {
            _observer?.OnBadAnimLinkRead(placeholderModel.Animation + animIndex, animation.NextAnimation, animation.NextFrame);
            animation.NextAnimation = (ushort)((animation.NextAnimation - placeholderModel.Animation) % placeholderModel.AnimCount);
            animation.NextFrame = 0;
        }
        else
        {
            PlaceholderAnimation nextAnim = _placeholderAnimations[animation.NextAnimation];
            animation.NextAnimation -= placeholderModel.Animation;
            animation.NextFrame -= nextAnim.RelFrameStart;
        }
        animation.FrameEnd -= animation.FrameStart;
        animation.FrameStart = 0;

        animation.Frames = new();
        int frameIndex = (int)placeholderAnimation.FrameOffset / sizeof(short);
        uint numAnimFrames = 0;

        if (placeholderAnimation.FrameSize == 0)
        {
            if (_version == TRGameVersion.TR1)
            {
                numAnimFrames = (uint)(Math.Ceiling((animation.FrameEnd - animation.FrameStart) / (float)animation.FrameRate) + 1);
            }
        }
        else
        {
            uint nextOffset = globalAnimIndex == _animations.Count - 1
                ? (uint)(_frames.Count * sizeof(short))
                : _placeholderAnimations[globalAnimIndex + 1].FrameOffset;

            numAnimFrames = (nextOffset - placeholderAnimation.FrameOffset) / (uint)(sizeof(short) * placeholderAnimation.FrameSize);
            if (numAnimFrames == 0)
            {
                // TR4 Lara anim 63 for example. Allow it to be observed, it will be zeroed on write.
                _observer?.OnEmptyAnimFramesRead(globalAnimIndex, placeholderAnimation.FrameSize);
            }
        }

        for (int i = 0; i < numAnimFrames; i++)
        {
            int frameIndexStart = frameIndex;
            animation.Frames.Add(BuildFrame(ref frameIndex, placeholderModel.NumMeshes));

            if (_version != TRGameVersion.TR1)
            {
                // All frames in an animation are aligned to the size of the largest one in TR2+. In addition, TR5 has frames aligned to 2 shorts,
                // so we need to ensure the final frame is also padded. The padding is random, so it can be observed. Zeroed on write.
                int padding = Math.Max(0, placeholderAnimation.FrameSize - frameIndex + frameIndexStart);
                if (_version == TRGameVersion.TR5 && padding == 0 && _frames.Count % 2 == 0 && frameIndex == _frames.Count - 1)
                {
                    padding = 1;
                }
                _observer?.OnFramePaddingRead(globalAnimIndex, i, _frames.GetRange(frameIndex, padding));
                frameIndex += padding;
            }
        }

        return animation;
    }

    private List<TRStateChange> BuildStateChanges(PlaceholderModel placeholderModel, int parentAnimIndex)
    {
        TRAnimation animation = _animations[parentAnimIndex];
        PlaceholderAnimation placeholderAnimation = _placeholderAnimations[parentAnimIndex];
        List<TRStateChange> changes = new();

        for (int i = 0; i < placeholderAnimation.NumStateChanges; i++)
        {
            int changeOffset = placeholderAnimation.ChangeOffset + i;
            PlaceholderChange placeholderChange = _placeholderChanges[changeOffset];
            TRStateChange change = new()
            {
                StateID = placeholderChange.StateID,
            };
            changes.Add(change);

            for (int j = 0; j < placeholderChange.NumAnimDispatches; j++)
            {
                TRAnimDispatch dispatch = _dispatches[placeholderChange.AnimDispatch + j];
                change.Dispatches.Add(dispatch);

                if ((dispatch.NextAnimation - placeholderModel.Animation) >= placeholderModel.AnimCount)
                {
                    // Bad link in OG data - occurs e.g. in ToT, Obelisk and Sanctuary with the missile object.
                    // Observe it, and reset it to within the model's range.
                    _observer?.OnBadDispatchLinkRead(placeholderChange.AnimDispatch + j, dispatch.NextAnimation, dispatch.NextFrame);
                    dispatch.NextAnimation = (short)((dispatch.NextAnimation - placeholderModel.Animation) % placeholderModel.AnimCount);
                    dispatch.NextFrame = 0;
                }
                else
                {
                    PlaceholderAnimation linkAnim = _placeholderAnimations[dispatch.NextAnimation];
                    dispatch.NextAnimation -= (short)placeholderModel.Animation;
                    dispatch.NextFrame -= (short)linkAnim.RelFrameStart;
                }

                dispatch.High -= (short)animation.FrameStart;
                dispatch.Low -= (short)animation.FrameStart;
            }
        }

        return changes;
    }

    private List<TRAnimCommand> BuildCommands(int parentAnimIndex)
    {
        TRAnimation animation = _animations[parentAnimIndex];
        PlaceholderAnimation placeholderAnimation = _placeholderAnimations[parentAnimIndex];
        int offset = placeholderAnimation.AnimCommand;
        List<TRAnimCommand> animCommands = new();

        if (placeholderAnimation.NumAnimCommands >= _commands.Count)
        {
            // E.g. model 40 in Angkor Wat apparently has 43690 commands, even though the next animation
            // references the same command offset. Reset to zero, and inform the observer.
            _observer?.OnBadAnimCommandRead(parentAnimIndex, placeholderAnimation.NumAnimCommands);
        }
        else
        {
            for (int i = 0; i < placeholderAnimation.NumAnimCommands && offset < _commands.Count; i++)
            {
                TRAnimCommand command;
                TRAnimCommandType type = (TRAnimCommandType)_commands[offset++];
                switch (type)
                {
                    case TRAnimCommandType.SetPosition:
                        command = new TRSetPositionCommand
                        {
                            X = _commands[offset++],
                            Y = _commands[offset++],
                            Z = _commands[offset++],
                        };
                        break;
                    case TRAnimCommandType.JumpDistance:
                        command = new TRJumpDistanceCommand
                        {
                            VerticalSpeed = _commands[offset++],
                            HorizontalSpeed = _commands[offset++],
                        };
                        break;
                    case TRAnimCommandType.EmptyHands:
                        command = new TREmptyHandsCommand();
                        break;
                    case TRAnimCommandType.Kill:
                        command = new TRKillCommand();
                        break;
                    case TRAnimCommandType.PlaySound:
                        short sfxFrame = (short)(_commands[offset++] - animation.FrameStart);
                        short sfxID = _commands[offset++];
                        command = new TRSFXCommand
                        {
                            FrameNumber = sfxFrame,
                            SoundID = (short)(sfxID & 0x3FFF),
                            Environment = _version == TRGameVersion.TR1 ? TRSFXEnvironment.Any : (TRSFXEnvironment)(sfxID & 0xC000)
                        };
                        break;
                    case TRAnimCommandType.FlipEffect:
                        short fxFrame = (short)(_commands[offset++] - animation.FrameStart);
                        short fxID = _commands[offset++];
                        if (_version > TRGameVersion.TR2 && (fxID & 0x3FFF) == (int)TR3FX.Footprint)
                        {
                            command = new TRFootprintCommand
                            {
                                FrameNumber = fxFrame,
                                Foot = (TRFootprint)(fxID & 0xC000),
                            };
                        }
                        else
                        {
                            command = new TRFXCommand
                            {
                                FrameNumber = fxFrame,
                                EffectID = fxID,
                            };
                        }
                        break;
                    default:
                        // Karnak and Citadel Gate have these. Easier to store than observe as
                        // index tracking gets far too complex.
                        command = new TRNullCommand
                        {
                            Value = (short)type,
                        };
                        break;
                }

                animCommands.Add(command);
            }
        }

        if (_observer != null && _version == TRGameVersion.TR5 && offset == _commands.Count - 1)
        {
            _observer.OnAnimCommandPaddingRead(_commands[^1]);
        }

        return animCommands;
    }

    private TRAnimFrame BuildFrame(ref int frameIndex, int numRotations)
    {
        TRAnimFrame frame = new()
        {
            Bounds = new()
            {
                MinX = _frames[frameIndex++],
                MaxX = _frames[frameIndex++],
                MinY = _frames[frameIndex++],
                MaxY = _frames[frameIndex++],
                MinZ = _frames[frameIndex++],
                MaxZ = _frames[frameIndex++],
            },
            OffsetX = _frames[frameIndex++],
            OffsetY = _frames[frameIndex++],
            OffsetZ = _frames[frameIndex++],
        };

        // TR1 stores the mesh count, but this always matches the model mesh count
        if (_version == TRGameVersion.TR1)
        {
            frameIndex++;
        }

        frame.Rotations = new();
        for (int i = 0; i < numRotations; i++)
        {
            TRAnimFrameRotation rot = new();
            frame.Rotations.Add(rot);

            short rot0, rot1;
            TRAngleMode rotMode = TRAngleMode.All;
            if (_version == TRGameVersion.TR1)
            {
                // Reversed words
                rot1 = _frames[frameIndex++];
                rot0 = _frames[frameIndex++];
            }
            else
            {
                rot0 = _frames[frameIndex++];
                rotMode = (TRAngleMode)(rot0 & 0xC000);
                rot1 = rotMode == TRAngleMode.All
                    ? _frames[frameIndex++]
                    : GetSingleRotation(rot0);
            }

            switch (rotMode)
            {
                case TRAngleMode.X:
                    rot.X = rot1;
                    break;
                case TRAngleMode.Y:
                    rot.Y = rot1;
                    break;
                case TRAngleMode.Z:
                    rot.Z = rot1;
                    break;
                default:
                    UnpackRotation(rot, rot0, rot1);
                    break;
            }
        }

        return frame;
    }

    private void TestTR5Changes(List<TRModel> models)
    {
        if (_observer == null || _version != TRGameVersion.TR5)
        {
            return;
        }

        // Some levels have an unreferenced state change at the end with a state ID that doesn't match
        // anything in the game. Need to observe for tests.
        int totalChanges = models.Sum(m => m.Animations.Sum(a => a.Changes.Count));
        if (totalChanges == _placeholderChanges.Count - 1)
        {
            PlaceholderChange finalChange = _placeholderChanges[^1];
            _observer.OnUnusedStateChangeRead(new(finalChange.StateID, finalChange.AnimDispatch));
        }
    }

    private void DeconstructModel(TRModel model)
    {
        PlaceholderModel placeholderModel = new()
        {
            ID = model.ID,
            Animation = model.Animations.Count == 0 ? TRConsts.NoAnimation : (ushort)_placeholderAnimations.Count,
            FrameOffset = (uint)_frames.Count * sizeof(short),
            NumMeshes = model.NumMeshes,
            StartingMesh = model.StartingMesh,
        };
        _placeholderModels.Add(placeholderModel);

        _trees.AddRange(model.MeshTrees);

        ushort frameBase = 0;
        foreach (TRAnimation animation in model.Animations)
        {
            PlaceholderAnimation placeholderAnimation = new()
            {
                FrameOffset = (uint)_frames.Count * sizeof(short),
                RelFrameStart = frameBase
            };
            _placeholderAnimations.Add(placeholderAnimation);

            if (animation.Frames.Count > 0)
            {
                frameBase += (ushort)(animation.FrameEnd + 1);
            }

            DeconstructFrames(placeholderAnimation, animation);
            DeconstructCommands(placeholderAnimation, animation);

            placeholderAnimation.ChangeOffset = (ushort)_placeholderChanges.Count;
            placeholderAnimation.NumStateChanges = (ushort)animation.Changes.Count;
            foreach (TRStateChange change in animation.Changes)
            {
                PlaceholderChange placeholderChange = new()
                {
                    StateID = change.StateID,
                    AnimDispatch = (ushort)_dispatches.Count,
                    NumAnimDispatches = (ushort)change.Dispatches.Count,
                };
                _placeholderChanges.Add(placeholderChange);

                foreach (TRAnimDispatch dispatch in change.Dispatches)
                {
                    _dispatches.Add(dispatch);
                    _dispatchFrameBase[dispatch] = (short)placeholderAnimation.RelFrameStart;
                    _dispatchToAnimMap[dispatch] = (short)(dispatch.NextAnimation + placeholderModel.Animation);
                }
            }
        }
    }

    private void DeconstructFrames(PlaceholderAnimation placeholderAnimation, TRAnimation animation)
    {
        List<List<short>> unpaddedFrames = new();
        int longestSet = 0;
        foreach (TRAnimFrame frame in animation.Frames)
        {
            List<short> frameSet = Flatten(frame);
            unpaddedFrames.Add(frameSet);
            longestSet = Math.Max(longestSet, frameSet.Count);
        }

        if (_version != TRGameVersion.TR1)
        {
            placeholderAnimation.FrameSize = _observer?.GetEmptyAnimFrameSize(_placeholderAnimations.Count - 1) 
                ?? (byte)longestSet;
        }

        for (int i = 0; i < unpaddedFrames.Count; i++)
        {
            List<short> frameSet = unpaddedFrames[i];
            if (_version != TRGameVersion.TR1)
            {
                // Pad so that all frames are the same size as the largest one.
                frameSet.AddRange(_observer?.GetFramePadding(_placeholderAnimations.Count - 1, i)
                    ?? Enumerable.Repeat((short)0, longestSet - frameSet.Count));
            }
            _frames.AddRange(frameSet);
        }
    }

    public List<short> Flatten(TRAnimFrame frame)
    {
        List<short> frames = new()
        {
            frame.Bounds.MinX,
            frame.Bounds.MaxX,
            frame.Bounds.MinY,
            frame.Bounds.MaxY,
            frame.Bounds.MinZ,
            frame.Bounds.MaxZ,
            frame.OffsetX,
            frame.OffsetY,
            frame.OffsetZ,
        };

        if (_version == TRGameVersion.TR1)
        {
            frames.Add((short)frame.Rotations.Count);
        }

        foreach (TRAnimFrameRotation rot in frame.Rotations)
        {
            int rotX = rot.X & 0x03FF;
            int rotY = rot.Y & 0x03FF;
            int rotZ = rot.Z & 0x03FF;

            if (_version == TRGameVersion.TR1)
            {
                // Reverse order
                frames.Add(PackYZRotation(rotY, rotZ));
                frames.Add(PackXYRotation(rotX, rotY));
            }
            else
            {
                TRAngleMode mode = GetMode(rot);
                switch (mode)
                {
                    case TRAngleMode.X:
                        frames.Add(MaskSingleRotation(rot.X, mode));
                        break;
                    case TRAngleMode.Y:
                        frames.Add(MaskSingleRotation(rot.Y, mode));
                        break;
                    case TRAngleMode.Z:
                        frames.Add(MaskSingleRotation(rot.Z, mode));
                        break;
                    default:
                        frames.Add(PackXYRotation(rotX, rotY));
                        frames.Add(PackYZRotation(rotY, rotZ));
                        break;
                }
            }
        }

        return frames;
    }

    private void DeconstructCommands(PlaceholderAnimation placeholderAnimation, TRAnimation animation)
    {
        // NumAnimCommands may have been wrong on read, so test observers can restore.
        placeholderAnimation.AnimCommand = (ushort)_commands.Count;
        ushort commandCount = 0;

        foreach (TRAnimCommand cmd in animation.Commands)
        {
            List<short> values = new();
            bool validCommand = true;

            switch (cmd)
            {
                case TRSetPositionCommand setCmd:
                    values.Add(setCmd.X);
                    values.Add(setCmd.Y);
                    values.Add(setCmd.Z);
                    break;
                case TRJumpDistanceCommand jumpCmd:
                    values.Add(jumpCmd.VerticalSpeed);
                    values.Add(jumpCmd.HorizontalSpeed);
                    break;
                case TRFXCommand flipCmd:
                    values.Add((short)(flipCmd.FrameNumber + placeholderAnimation.RelFrameStart));
                    values.Add(flipCmd.EffectID);
                    break;
                case TRFootprintCommand footCmd:
                    if (validCommand = _version >= TRGameVersion.TR3)
                    {
                        values.Add((short)(footCmd.FrameNumber + placeholderAnimation.RelFrameStart));
                        values.Add((short)((int)TR3FX.Footprint | (int)footCmd.Foot));
                    }
                    break;
                case TRSFXCommand sfxCmd:
                    {
                        short soundID = sfxCmd.SoundID;
                        if (_version > TRGameVersion.TR1)
                        {
                            soundID = (short)((int)soundID | (int)sfxCmd.Environment);
                        }
                        values.Add((short)(sfxCmd.FrameNumber + placeholderAnimation.RelFrameStart));
                        values.Add(soundID);
                        break;
                    }
            }

            if (validCommand)
            {
                _commands.Add(cmd is TRNullCommand nullCmd ? nullCmd.Value : (short)cmd.Type);
                _commands.AddRange(values);
                commandCount++;
            }
        }

        placeholderAnimation.NumAnimCommands = _observer?.GetNumAnimCommands(_placeholderAnimations.Count - 1) ?? commandCount;
    }

    private void RestoreTR5Extras()
    {
        if (_version != TRGameVersion.TR5)
        {
            return;
        }

        short? commandPadding = _observer?.GetAnimCommandPadding();
        if (commandPadding.HasValue)
        {
            _commands.Add(commandPadding.Value);
        }

        Tuple<ushort, ushort> extraChange = _observer?.GetUnusedStateChange();
        if (extraChange != null)
        {
            _placeholderChanges.Add(new()
            {
                StateID = extraChange.Item1,
                AnimDispatch = extraChange.Item2,
            });
        }
    }

    private void WriteAnimations(TRLevelWriter writer, List<TRModel> models)
    {
        writer.Write((uint)_placeholderAnimations.Count);
        foreach (TRModel model in models)
        {
            PlaceholderModel placeholderModel = _placeholderModels.Find(m => m.ID == model.ID);

            for (int i = 0; i < model.Animations.Count; i++)
            {
                TRAnimation animation = model.Animations[i];
                PlaceholderAnimation placeholderAnimation = _placeholderAnimations[placeholderModel.Animation + i];

                // Allow bad links to be restored for tests.
                Tuple<ushort, ushort> nextAnimLink = _observer?.GetAnimLink(placeholderModel.Animation + i);
                if (nextAnimLink == null)
                {
                    ushort nextAnim = (ushort)(placeholderModel.Animation + animation.NextAnimation);
                    ushort nextFrame = (ushort)(animation.NextFrame + _placeholderAnimations[nextAnim].RelFrameStart);
                    nextAnimLink = new(nextAnim, nextFrame);
                }

                writer.Write(placeholderAnimation.FrameOffset);
                writer.Write(animation.FrameRate);
                writer.Write(placeholderAnimation.FrameSize);
                writer.Write(animation.StateID);
                writer.Write(animation.Speed);
                writer.Write(animation.Accel);

                if (_version >= TRGameVersion.TR4)
                {
                    writer.Write(animation.SpeedLateral);
                    writer.Write(animation.AccelLateral);
                }

                writer.Write((ushort)(animation.FrameStart + placeholderAnimation.RelFrameStart));
                writer.Write((ushort)(animation.FrameEnd + placeholderAnimation.RelFrameStart));
                writer.Write(nextAnimLink.Item1); // Next animation
                writer.Write(nextAnimLink.Item2); // Next frame
                writer.Write((ushort)animation.Changes.Count);
                writer.Write(placeholderAnimation.ChangeOffset);
                writer.Write(placeholderAnimation.NumAnimCommands);
                writer.Write(placeholderAnimation.AnimCommand);
            }
        }
    }

    private void WriteChanges(TRLevelWriter writer)
    {
        writer.Write((uint)_placeholderChanges.Count);
        foreach (PlaceholderChange change in _placeholderChanges)
        {
            writer.Write(change.StateID);
            writer.Write(change.NumAnimDispatches);
            writer.Write(change.AnimDispatch);
        }
    }

    private void WriteDispatches(TRLevelWriter writer)
    {
        writer.Write((uint)_dispatches.Count);
        for (int i = 0; i < _dispatches.Count; i++)
        {
            TRAnimDispatch dispatch = _dispatches[i];
            writer.Write((short)(dispatch.Low + _dispatchFrameBase[dispatch]));
            writer.Write((short)(dispatch.High + _dispatchFrameBase[dispatch]));

            // Allow bad links to be restored for tests.
            Tuple<short, short> link = _observer?.GetDispatchLink(i);
            if (link == null)
            {
                PlaceholderAnimation nextAnim = _placeholderAnimations[_dispatchToAnimMap[dispatch]];
                link = new(_dispatchToAnimMap[dispatch], (short)(dispatch.NextFrame + nextAnim.RelFrameStart));
            }
            writer.Write(link.Item1); // Next animation
            writer.Write(link.Item2); // Next frame
        }
    }

    private void WriteCommands(TRLevelWriter writer)
    {
        writer.Write((uint)_commands.Count);
        writer.Write(_commands);
    }

    private void WriteTrees(TRLevelWriter writer)
    {
        writer.Write((uint)_trees.Count * sizeof(int));
        foreach (TRMeshTreeNode tree in _trees)
        {
            writer.Write(tree.Flags);
            writer.Write(tree.OffsetX);
            writer.Write(tree.OffsetY);
            writer.Write(tree.OffsetZ);
        }
    }

    private void WriteFrames(TRLevelWriter writer)
    {
        writer.Write((uint)_frames.Count);
        writer.Write(_frames);
    }

    private void WriteModels(TRLevelWriter writer, List<TRModel> models)
    {
        writer.Write((uint)models.Count);

        uint treePointer = 0;
        foreach (TRModel model in models)
        {
            PlaceholderModel placeholderModel = _placeholderModels.Find(m => m.ID == model.ID);

            writer.Write(placeholderModel.ID);
            writer.Write(model.NumMeshes);
            writer.Write(model.StartingMesh);
            writer.Write(treePointer);
            writer.Write(placeholderModel.FrameOffset);
            writer.Write(placeholderModel.Animation);

            if (_version == TRGameVersion.TR5)
            {
                writer.Write(_tr5ModelPadding);
            }

            treePointer += (uint)(model.MeshTrees.Count * sizeof(int));
        }
    }

    private static TRAngleMode GetMode(TRAnimFrameRotation rot)
    {
        if (rot.X == 0 && rot.Y == 0)
        {
            return TRAngleMode.Z;
        }
        if (rot.X == 0 && rot.Z == 0)
        {
            return TRAngleMode.Y;
        }
        if (rot.Y == 0 && rot.Z == 0)
        {
            return TRAngleMode.X;
        }
        return TRAngleMode.All;
    }

    private static void UnpackRotation(TRAnimFrameRotation rot, short rot0, short rot1)
    {
        rot.X = (short)((rot0 & 0x3FF0) >> 4);
        rot.Y = (short)(((rot0 & 0x000F) << 6) | ((rot1 & 0xFC00) >> 10));
        rot.Z = (short)(rot1 & 0x03FF);
    }

    private static short PackXYRotation(int x, int y)
    {
        return (short)((x << 4) | ((y & 0x0FC0) >> 6));
    }

    private static short PackYZRotation(int y, int z)
    {
        return (short)(((y & 0x003F) << 10) | (z & 0x03FF));
    }

    private short GetSingleRotation(int angle)
    {
        if (_version < TRGameVersion.TR4)
        {
            return (short)(angle & 0x03FF);
        }

        return (short)(angle & 0x0FFF);
    }

    private short MaskSingleRotation(int angle, TRAngleMode mode)
    {
        if (_version < TRGameVersion.TR4)
        {
            return (short)((angle & 0x03FF) | (int)mode);
        }

        return (short)((angle & 0x0FFF) | (int)mode);
    }

    // Information we need for building, but do not want to retain.
    class PlaceholderModel
    {
        public uint ID { get; set; }
        public ushort NumMeshes { get; set; }
        public ushort StartingMesh { get; set; }
        public uint MeshTree { get; set; }
        public uint FrameOffset { get; set; }
        public ushort Animation { get; set; }
        public int AnimCount { get; set; }
    }

    class PlaceholderAnimation
    {
        public byte FrameSize { get; set; }
        public uint FrameOffset { get; set; }
        public ushort RelFrameStart { get; set; }
        public ushort NumStateChanges { get; set; }
        public ushort ChangeOffset { get; set; }
        public ushort NumAnimCommands { get; set; }
        public ushort AnimCommand { get; set; }
    }

    class PlaceholderChange
    {
        public ushort StateID { get; set; }
        public ushort NumAnimDispatches { get; set; }
        public ushort AnimDispatch { get; set; }
    }
}
