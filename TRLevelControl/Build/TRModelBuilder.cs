using System.Diagnostics;
using TRLevelControl.Model;

namespace TRLevelControl.Build;

public class TRModelBuilder
{
    private static readonly ushort _tr5ModelPadding = 0xFFEF;

    private readonly TRGameVersion _version;

    public TRModelBuilder(TRGameVersion version)
    {
        _version = version;
    }

    public List<TRAnimation> ReadAnimations(TRLevelReader reader)
    {
        uint numAnimations = reader.ReadUInt32();
        List<TRAnimation> animations = new();

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

            animations.Add(animation);
        }

        return animations;
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

    public List<TRStateChange> ReadStateChanges(TRLevelReader reader)
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

        return stateChanges;
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

    public List<TRAnimDispatch> ReadDispatches(TRLevelReader reader)
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

        return dispatches;
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

    public List<TRAnimCommand> ReadCommands(TRLevelReader reader)
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

        return commands;
    }

    public void Write(List<TRAnimCommand> commands, TRLevelWriter writer)
    {
        writer.Write((uint)commands.Count);

        foreach (TRAnimCommand command in commands)
        {
            writer.Write(command.Value);
        }
    }

    public List<TRMeshTreeNode> ReadTrees(TRLevelReader reader)
    {
        uint numMeshTrees = reader.ReadUInt32() / sizeof(int);
        List<TRMeshTreeNode> trees = new();

        for (int i = 0; i < numMeshTrees; i++)
        {
            trees.Add(new()
            {
                Flags = reader.ReadUInt32(),
                OffsetX = reader.ReadInt32(),
                OffsetY = reader.ReadInt32(),
                OffsetZ = reader.ReadInt32(),
            });
        }

        return trees;
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
        }

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
