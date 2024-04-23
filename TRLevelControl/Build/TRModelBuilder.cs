using System.Diagnostics;
using TRLevelControl.Model;

namespace TRLevelControl.Build;

public class TRModelBuilder
{
    private static readonly ushort _tr5ModelPadding = 0xFFEF;

    private readonly TRGameVersion _version;

    private List<TRAnimation> _animations;
    private List<TRMeshTreeNode> _trees;

    public TRModelBuilder(TRGameVersion version)
    {
        _version = version;
    }

    public void ReadAnimations(TRLevelReader reader)
    {
        uint numAnimations = reader.ReadUInt32();
        _animations = new();

        for (int i = 0; i < numAnimations; i++)
        {
            TRAnimation animation = new()
            {
                FrameOffset = reader.ReadUInt32(),
                FrameRate = reader.ReadByte(),
                FrameSize = reader.ReadByte(),
                StateID = reader.ReadUInt16(),
                Speed = reader.ReadFixed32(),
                Accel = reader.ReadFixed32(),
            };

            if (_version >= TRGameVersion.TR4)
            {
                animation.SpeedLateral = reader.ReadFixed32();
                animation.AccelLateral = reader.ReadFixed32();
            }

            animation.FrameStart = reader.ReadUInt16();
            animation.FrameEnd = reader.ReadUInt16();
            animation.NextAnimation = reader.ReadUInt16();
            animation.NextFrame = reader.ReadUInt16();
            animation.NumStateChanges = reader.ReadUInt16();
            animation.StateChangeOffset = reader.ReadUInt16();
            animation.NumAnimCommands = reader.ReadUInt16();
            animation.AnimCommand = reader.ReadUInt16();

            _animations.Add(animation);
        }
    }

    public void Write(List<TRAnimation> animations, TRLevelWriter writer)
    {
        writer.Write((uint)animations.Count);

        foreach (TRAnimation animation in animations)
        {
            writer.Write(animation.FrameOffset);
            writer.Write(animation.FrameRate);
            writer.Write(animation.FrameSize);
            writer.Write(animation.StateID);
            writer.Write(animation.Speed);
            writer.Write(animation.Accel);

            if (_version >= TRGameVersion.TR4)
            {
                writer.Write(animation.SpeedLateral);
                writer.Write(animation.AccelLateral);
            }

            writer.Write(animation.FrameStart);
            writer.Write(animation.FrameEnd);
            writer.Write(animation.NextAnimation);
            writer.Write(animation.NextFrame);
            writer.Write(animation.NumStateChanges);
            writer.Write(animation.StateChangeOffset);
            writer.Write(animation.NumAnimCommands);
            writer.Write(animation.AnimCommand);
        }
    }

    public void ReadStateChanges(TRLevelReader reader)
    {
        uint numStateChanges = reader.ReadUInt32();
        List<TRStateChange> stateChanges = new();

        for (int i = 0; i < numStateChanges; i++)
        {
            stateChanges.Add(new()
            {
                StateID = reader.ReadUInt16(),
                NumAnimDispatches = reader.ReadUInt16(),
                AnimDispatch = reader.ReadUInt16(),
            });
        }

        foreach (TRAnimation animation in _animations)
        {
            for (int i = 0; i < animation.NumStateChanges; i++)
            {
                int changeOffset = animation.StateChangeOffset + i;
                animation.Changes.Add(stateChanges[changeOffset]);
            }
        }

        if (_version != TRGameVersion.TR5)
        {
            // Some TR5 levels contain an unused change at the end of the list.
            // Figure out test observation things later.
            Debug.Assert(stateChanges.Count == _animations.Sum(a => a.Changes.Count));
        }
    }

    public void Write(List<TRStateChange> stateChanges, TRLevelWriter writer)
    {
        writer.Write((uint)stateChanges.Count);

        foreach (TRStateChange stateChange in stateChanges)
        {
            writer.Write(stateChange.StateID);
            writer.Write(stateChange.NumAnimDispatches);
            writer.Write(stateChange.AnimDispatch);
        }
    }

    public void ReadDispatches(TRLevelReader reader)
    {
        uint numAnimDispatches = reader.ReadUInt32();
        List<TRAnimDispatch> dispatches = new();

        for (int i = 0; i < numAnimDispatches; i++)
        {
            dispatches.Add(new()
            {
                Low = reader.ReadInt16(),
                High = reader.ReadInt16(),
                NextAnimation = reader.ReadInt16(),
                NextFrame = reader.ReadInt16(),
            });
        }

        foreach (TRStateChange change in _animations.SelectMany(a => a.Changes))
        {
            for (int i = 0; i < change.NumAnimDispatches; i++)
            {
                change.Dispatches.Add(dispatches[change.AnimDispatch + i]);
            }
        }

        if (_version != TRGameVersion.TR5)
        {
            // More unused TR5 data
            Debug.Assert(dispatches.Count == _animations.Sum(a => a.Changes.Sum(c => c.Dispatches.Count)));
        }
    }

    public void Write(List<TRAnimDispatch> dispatches, TRLevelWriter writer)
    {
        writer.Write((uint)dispatches.Count);

        foreach (TRAnimDispatch dispatch in dispatches)
        {
            writer.Write(dispatch.Low);
            writer.Write(dispatch.High);
            writer.Write(dispatch.NextAnimation);
            writer.Write(dispatch.NextFrame);
        }
    }

    public void ReadCommands(TRLevelReader reader)
    {
        uint numAnimCommands = reader.ReadUInt32();
        List<TRAnimCommand> commands = new();

        for (int i = 0; i < numAnimCommands; i++)
        {
            commands.Add(new()
            {
                Value = reader.ReadInt16(),
            });
        }

        foreach (TRAnimation animation in _animations)
        {
            int offset = animation.AnimCommand;
            if (animation.NumAnimCommands < commands.Count)
            {
                for (int i = 0; i < animation.NumAnimCommands; i++)
                {
                    int additionalValues = 0;
                    TRAnimCommandTypes type = (TRAnimCommandTypes)commands[offset].Value;
                    switch (type)
                    {
                        case TRAnimCommandTypes.SetPosition:
                            additionalValues = 3;
                            break;
                        case TRAnimCommandTypes.JumpDistance:
                        case TRAnimCommandTypes.PlaySound:
                        case TRAnimCommandTypes.FlipEffect:
                            additionalValues = 2;
                            break;

                    }

                    animation.Commands.Add(commands[offset++]);
                    for (int j = 0; j < additionalValues; j++)
                    {
                        animation.Commands.Add(commands[offset++]);
                    }
                }
            }
            else
            {
                // TR4/5 have some crazy out of range offset pointers. To be observed for tests.
            }
        }

        if (_version < TRGameVersion.TR4)
        {
            Debug.Assert(commands.Count == _animations.Sum(a => a.Commands.Count));
        }
    }

    public void Write(List<TRAnimCommand> commands, TRLevelWriter writer)
    {
        writer.Write((uint)commands.Count);

        foreach (TRAnimCommand command in commands)
        {
            writer.Write(command.Value);
        }
    }

    public void ReadTrees(TRLevelReader reader)
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

    public void Write(List<TRMeshTreeNode> trees, TRLevelWriter writer)
    {
        writer.Write((uint)trees.Count * sizeof(int));

        foreach (TRMeshTreeNode tree in trees)
        {
            writer.Write(tree.Flags);
            writer.Write(tree.OffsetX);
            writer.Write(tree.OffsetY);
            writer.Write(tree.OffsetZ);
        }
    }

    public List<ushort> ReadFrames(TRLevelReader reader)
    {
        uint numFrames = reader.ReadUInt32();
        return new(reader.ReadUInt16s(numFrames));
    }

    public void Write(List<ushort> frames, TRLevelWriter writer)
    {
        writer.Write((uint)frames.Count);
        writer.Write(frames);
    }

    public List<TRModel> ReadModels(TRLevelReader reader)
    {
        uint numModels = reader.ReadUInt32();
        List<TRModel> models = new();

        for (int i = 0; i < numModels; i++)
        {
            TRModel model = new()
            {
                ID = reader.ReadUInt32(),
                NumMeshes = reader.ReadUInt16(),
                StartingMesh = reader.ReadUInt16(),
                MeshTree = reader.ReadUInt32(),
                FrameOffset = reader.ReadUInt32(),
                Animation = reader.ReadUInt16()
            };

            if (_version == TRGameVersion.TR5)
            {
                Debug.Assert(reader.ReadUInt16() == _tr5ModelPadding);
            }

            models.Add(model);

            // Everything has a dummy mesh tree
            int treePointer = (int)model.MeshTree / sizeof(int);
            for (int j = 1; j < model.NumMeshes; j++)
            {
                model.MeshTrees.Add(_trees[treePointer + j - 1]);
            }
        }

        Debug.Assert(_trees.Count == models.Sum(m => m.MeshTrees.Count));

        List<TRModel> animatedModels = models.FindAll(m => m.Animation != TRConsts.NoAnimation);
        for (int i = 0; i < animatedModels.Count; i++)
        {
            TRModel model = animatedModels[i];
            int nextOffset = i == animatedModels.Count - 1
                ? _animations.Count
                : animatedModels[i + 1].Animation;
            int animCount = nextOffset - model.Animation;

            for (int j = 0; j < animCount; j++)
            {
                model.Animations.Add(_animations[model.Animation + j]);
            }
        }

        Debug.Assert(_animations.Count == models.Sum(m => m.Animations.Count));

        return models;
    }

    public void Write(List<TRModel> models, TRLevelWriter writer)
    {
        writer.Write((uint)models.Count);

        foreach (TRModel model in models)
        {
            writer.Write(model.ID);
            writer.Write(model.NumMeshes);
            writer.Write(model.StartingMesh);
            writer.Write(model.MeshTree);
            writer.Write(model.FrameOffset);
            writer.Write(model.Animation);

            if (_version == TRGameVersion.TR5)
            {
                writer.Write(_tr5ModelPadding);
            }
        }
    }
}
