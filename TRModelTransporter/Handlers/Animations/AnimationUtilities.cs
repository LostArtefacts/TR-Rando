using TRLevelControl.Model;
using TRLevelControl.Model.Enums;
using TRModelTransporter.Model.Animations;
using TRModelTransporter.Model.Definitions;

namespace TRModelTransporter.Handlers;

public static class AnimationUtilities
{
    public static int GetModelAnimationCount(TR1Level level, TRModel model)
    {
        return GetModelAnimationCount(level.Models, model, level.Animations.Count);
    }

    public static int GetModelAnimationCount(TR2Level level, TRModel model)
    {
        return GetModelAnimationCount(level.Models, model, level.Animations.Count);
    }

    public static int GetModelAnimationCount(TR3Level level, TRModel model)
    {
        return GetModelAnimationCount(level.Models, model, level.Animations.Count);
    }

    public static int GetModelAnimationCount(List<TRModel> models, TRModel model, int totalAnimations)
    {
        TRModel nextModel = model;
        int modelIndex = models.ToList().IndexOf(model) + 1;
        while (modelIndex < models.Count)
        {
            nextModel = models[modelIndex++];
            if (nextModel.Animation != ushort.MaxValue)
            {
                break;
            }
        }

        ushort nextStartAnimation = nextModel.Animation;
        if (model == nextModel || nextStartAnimation == ushort.MaxValue)
        {
            nextStartAnimation = (ushort)totalAnimations;
        }

        return model.Animation == ushort.MaxValue ? 0 : nextStartAnimation - model.Animation;
    }

    public static void PackStateChanges(TR1Level level, TRAnimation animation, TR1PackedAnimation packedAnimation)
    {
        for (int stateChangeIndex = 0; stateChangeIndex < animation.NumStateChanges; stateChangeIndex++)
        {
            TRStateChange stateChange = level.StateChanges[animation.StateChangeOffset + stateChangeIndex];
            packedAnimation.StateChanges.Add(stateChange);

            int dispatchOffset = stateChange.AnimDispatch;
            for (int i = 0; i < stateChange.NumAnimDispatches; i++, dispatchOffset++)
            {
                if (!packedAnimation.AnimationDispatches.ContainsKey(dispatchOffset))
                {
                    TRAnimDispatch dispatch = level.AnimDispatches[dispatchOffset];
                    packedAnimation.AnimationDispatches[dispatchOffset] = dispatch;
                }
            }
        }
    }

    public static void PackStateChanges(TR2Level level, TRAnimation animation, TR2PackedAnimation packedAnimation)
    {
        for (int stateChangeIndex = 0; stateChangeIndex < animation.NumStateChanges; stateChangeIndex++)
        {
            TRStateChange stateChange = level.StateChanges[animation.StateChangeOffset + stateChangeIndex];
            packedAnimation.StateChanges.Add(stateChange);

            int dispatchOffset = stateChange.AnimDispatch;
            for (int i = 0; i < stateChange.NumAnimDispatches; i++, dispatchOffset++)
            {
                if (!packedAnimation.AnimationDispatches.ContainsKey(dispatchOffset))
                {
                    TRAnimDispatch dispatch = level.AnimDispatches[dispatchOffset];
                    packedAnimation.AnimationDispatches[dispatchOffset] = dispatch;
                }
            }
        }
    }

    public static void PackStateChanges(TR3Level level, TRAnimation animation, TR3PackedAnimation packedAnimation)
    {
        for (int stateChangeIndex = 0; stateChangeIndex < animation.NumStateChanges; stateChangeIndex++)
        {
            TRStateChange stateChange = level.StateChanges[animation.StateChangeOffset + stateChangeIndex];
            packedAnimation.StateChanges.Add(stateChange);

            int dispatchOffset = stateChange.AnimDispatch;
            for (int i = 0; i < stateChange.NumAnimDispatches; i++, dispatchOffset++)
            {
                if (!packedAnimation.AnimationDispatches.ContainsKey(dispatchOffset))
                {
                    TRAnimDispatch dispatch = level.AnimDispatches[dispatchOffset];
                    packedAnimation.AnimationDispatches[dispatchOffset] = dispatch;
                }
            }
        }
    }

    public static void PackAnimCommands(TR1Level level, TRAnimation animation, TR1PackedAnimation packedAnimation)
    {
        packedAnimation.Commands = PackAnimCommands(level.AnimCommands, animation);
    }

    public static void PackAnimCommands(TR2Level level, TRAnimation animation, TR2PackedAnimation packedAnimation)
    {
        packedAnimation.Commands = PackAnimCommands(level.AnimCommands.ToList(), animation);
    }

    public static void PackAnimCommands(TR3Level level, TRAnimation animation, TR3PackedAnimation packedAnimation)
    {
        packedAnimation.Commands = PackAnimCommands(level.AnimCommands.ToList(), animation);
    }

    private static Dictionary<int, TR1PackedAnimationCommand> PackAnimCommands(List<TRAnimCommand> animCommands, TRAnimation animation)
    {
        Dictionary<int, TR1PackedAnimationCommand> cmds = new();

        int cmdOffset = animation.AnimCommand;
        for (int i = 0; i < animation.NumAnimCommands; i++)
        {
            int cmdIndex = cmdOffset++;
            TRAnimCommand cmd = animCommands[cmdIndex];
            int paramCount = (TRAnimCommandTypes)cmd.Value switch
            {
                TRAnimCommandTypes.SetPosition => 3,
                TRAnimCommandTypes.JumpDistance
                or TRAnimCommandTypes.PlaySound
                or TRAnimCommandTypes.FlipEffect => 2,
                _ => 0,
            };
            short[] paramArr = new short[paramCount];
            for (int j = 0; j < paramCount; j++)
            {
                paramArr[j] = animCommands[cmdOffset++].Value;
            }

            cmds[cmdIndex] = new TR1PackedAnimationCommand
            {
                Command = (TRAnimCommandTypes)cmd.Value,
                Params = paramArr
            };
        }

        return cmds;
    }

    public static void PackAnimSounds(TR1Level level, TR1ModelDefinition definition, TR1PackedAnimation packedAnimation)
    {
        definition.SoundEffects ??= new();
        foreach (TR1PackedAnimationCommand cmd in packedAnimation.Commands.Values)
        {
            if (cmd.Command != TRAnimCommandTypes.PlaySound)
            {
                continue;
            }

            TR1SFX sfxID = (TR1SFX)(cmd.Params[1] & 0x3FFF);
            if (level.SoundEffects.ContainsKey(sfxID))
            {
                definition.SoundEffects[sfxID] = level.SoundEffects[sfxID];
            }
        }
    }

    public static void PackAnimSounds(TR2Level level, TR2ModelDefinition definition, TR2PackedAnimation packedAnimation)
    {
        definition.SoundEffects ??= new();
        foreach (TR1PackedAnimationCommand cmd in packedAnimation.Commands.Values)
        {
            if (cmd.Command != TRAnimCommandTypes.PlaySound)
            {
                continue;
            }

            TR2SFX sfxID = (TR2SFX)(cmd.Params[1] & 0x3FFF);
            if (level.SoundEffects.ContainsKey(sfxID))
            {
                definition.SoundEffects[sfxID] = level.SoundEffects[sfxID];
            }
        }
    }

    public static void PackAnimSounds(TR3Level level, TR3ModelDefinition definition, TR3PackedAnimation packedAnimation)
    {
        definition.SoundEffects ??= new();
        foreach (TR1PackedAnimationCommand cmd in packedAnimation.Commands.Values)
        {
            if (cmd.Command != TRAnimCommandTypes.PlaySound)
            {
                continue;
            }

            TR3SFX sfxID = (TR3SFX)(cmd.Params[1] & 0x3FFF);
            if (level.SoundEffects.ContainsKey(sfxID))
            {
                definition.SoundEffects[sfxID] = level.SoundEffects[sfxID];
            }
        }
    }

    public static ushort[] GetAnimationFrames(TR1Level level, TRModel model)
    {
        return GetAnimationFrames(model, level.Models, level.Frames);
    }

    public static ushort[] GetAnimationFrames(TR2Level level, TRModel model)
    {
        return GetAnimationFrames(model, level.Models, level.Frames.ToList());
    }

    public static ushort[] GetAnimationFrames(TR3Level level, TRModel model)
    {
        return GetAnimationFrames(model, level.Models, level.Frames.ToList());
    }

    public static ushort[] GetAnimationFrames(TRModel model, List<TRModel> allModels, List<ushort> allFrames)
    {
        int modelIndex = allModels.IndexOf(model);
        uint endFrame = 0;
        if (modelIndex == allModels.Count - 1)
        {
            endFrame = (uint)allFrames.Count;
        }
        else
        {
            while (endFrame == 0 && modelIndex < allModels.Count)
            {
                endFrame = allModels[++modelIndex].FrameOffset / 2;
            }
        }

        List<ushort> frames = new();
        for (int i = (int)model.FrameOffset / 2; i < endFrame; i++)
        {
            frames.Add(allFrames[i]);
        }

        return frames.ToArray();
    }

    public static void UnpackStateChanges(List<TRAnimDispatch> animDispatches, List<TRStateChange> stateChanges, TR1PackedAnimation packedAnimation)
    {
        if (packedAnimation.Animation.NumStateChanges == 0)
        {
            return;
        }

        // Import the AnimDispatches first, noting their new indices            
        Dictionary<int, int> indexMap = new();
        foreach (int oldDispatchIndex in packedAnimation.AnimationDispatches.Keys)
        {
            TRAnimDispatch dispatch = packedAnimation.AnimationDispatches[oldDispatchIndex];
            indexMap[oldDispatchIndex] = animDispatches.Count;
            animDispatches.Add(dispatch);
        }

        // The animation's StateChangeOffset will be the current length of level StateChanges
        packedAnimation.Animation.StateChangeOffset = (ushort)stateChanges.Count;

        // Import Each state change, re-mapping AnimDispatch to new index
        foreach (TRStateChange stateChange in packedAnimation.StateChanges)
        {
            stateChange.AnimDispatch = (ushort)indexMap[stateChange.AnimDispatch];
            stateChanges.Add(stateChange);
        }
    }

    public static void UnpackStateChanges(List<TRAnimDispatch> animDispatches, List<TRStateChange> stateChanges, TR2PackedAnimation packedAnimation)
    {
        if (packedAnimation.Animation.NumStateChanges == 0)
        {
            return;
        }

        // Import the AnimDispatches first, noting their new indices            
        Dictionary<int, int> indexMap = new();
        foreach (int oldDispatchIndex in packedAnimation.AnimationDispatches.Keys)
        {
            TRAnimDispatch dispatch = packedAnimation.AnimationDispatches[oldDispatchIndex];
            indexMap[oldDispatchIndex] = animDispatches.Count;
            animDispatches.Add(dispatch);
        }

        // The animation's StateChangeOffset will be the current length of level StateChanges
        packedAnimation.Animation.StateChangeOffset = (ushort)stateChanges.Count;

        // Import Each state change, re-mapping AnimDispatch to new index
        foreach (TRStateChange stateChange in packedAnimation.StateChanges)
        {
            stateChange.AnimDispatch = (ushort)indexMap[stateChange.AnimDispatch];
            stateChanges.Add(stateChange);
        }
    }

    public static void UnpackStateChanges(List<TRAnimDispatch> animDispatches, List<TRStateChange> stateChanges, TR3PackedAnimation packedAnimation)
    {
        if (packedAnimation.Animation.NumStateChanges == 0)
        {
            return;
        }

        // Import the AnimDispatches first, noting their new indices            
        Dictionary<int, int> indexMap = new();
        foreach (int oldDispatchIndex in packedAnimation.AnimationDispatches.Keys)
        {
            TRAnimDispatch dispatch = packedAnimation.AnimationDispatches[oldDispatchIndex];
            indexMap[oldDispatchIndex] = animDispatches.Count;
            animDispatches.Add(dispatch);
        }

        // The animation's StateChangeOffset will be the current length of level StateChanges
        packedAnimation.Animation.StateChangeOffset = (ushort)stateChanges.Count;

        // Import Each state change, re-mapping AnimDispatch to new index
        foreach (TRStateChange stateChange in packedAnimation.StateChanges)
        {
            stateChange.AnimDispatch = (ushort)indexMap[stateChange.AnimDispatch];
            stateChanges.Add(stateChange);
        }
    }

    /**
     * Commands are packed into CmdType + Params, but these are just 
     * translated straight back into normal TRAnimCommands.
     */
    public static void UnpackAnimCommands(List<TRAnimCommand> levelAnimCommands, TR1PackedAnimation packedAnimation)
    {
        if (packedAnimation.Commands.Count == 0)
        {
            return;
        }

        packedAnimation.Animation.AnimCommand = (ushort)levelAnimCommands.Count;
        foreach (TR1PackedAnimationCommand cmd in packedAnimation.Commands.Values)
        {
            levelAnimCommands.Add(new TRAnimCommand { Value = (short)cmd.Command });
            foreach (short param in cmd.Params)
            {
                levelAnimCommands.Add(new TRAnimCommand { Value = param });
            }
        }
    }

    public static void UnpackAnimCommands(List<TRAnimCommand> levelAnimCommands, TR2PackedAnimation packedAnimation)
    {
        if (packedAnimation.Commands.Count == 0)
        {
            return;
        }

        packedAnimation.Animation.AnimCommand = (ushort)levelAnimCommands.Count;
        foreach (TR1PackedAnimationCommand cmd in packedAnimation.Commands.Values)
        {
            levelAnimCommands.Add(new TRAnimCommand { Value = (short)cmd.Command });
            foreach (short param in cmd.Params)
            {
                levelAnimCommands.Add(new TRAnimCommand { Value = param });
            }
        }
    }

    public static void UnpackAnimCommands(List<TRAnimCommand> levelAnimCommands, TR3PackedAnimation packedAnimation)
    {
        if (packedAnimation.Commands.Count == 0)
        {
            return;
        }

        packedAnimation.Animation.AnimCommand = (ushort)levelAnimCommands.Count;
        foreach (TR1PackedAnimationCommand cmd in packedAnimation.Commands.Values)
        {
            levelAnimCommands.Add(new TRAnimCommand { Value = (short)cmd.Command });
            foreach (short param in cmd.Params)
            {
                levelAnimCommands.Add(new TRAnimCommand { Value = param });
            }
        }
    }

    public static int UnpackAnimation(TR1Level level, TR1PackedAnimation animation)
    {
        level.Animations.Add(animation.Animation);
        return level.Animations.Count - 1;
    }

    public static int UnpackAnimation(TR2Level level, TR2PackedAnimation animation)
    {
        level.Animations.Add(animation.Animation);
        return level.Animations.Count - 1;
    }

    public static int UnpackAnimation(TR3Level level, TR3PackedAnimation animation)
    {
        level.Animations.Add(animation.Animation);
        return level.Animations.Count - 1;
    }

    public static void ImportAnimationFrames(TR1Level level, TR1ModelDefinition definition)
    {
        definition.Model.FrameOffset = (uint)level.Frames.Count * 2;
        level.Frames.AddRange(definition.AnimationFrames);

        foreach (TR1PackedAnimation packedAnimation in definition.Animations.Values)
        {
            packedAnimation.Animation.FrameOffset += definition.Model.FrameOffset;
        }
    }

    public static void ImportAnimationFrames(TR2Level level, TR2ModelDefinition definition)
    {
        definition.Model.FrameOffset = (uint)level.Frames.Count * 2;
        level.Frames.AddRange(definition.AnimationFrames);

        foreach (TR2PackedAnimation packedAnimation in definition.Animations.Values)
        {
            packedAnimation.Animation.FrameOffset += definition.Model.FrameOffset;
        }
    }

    public static void ImportAnimationFrames(TR3Level level, TR3ModelDefinition definition)
    {
        definition.Model.FrameOffset = (uint)level.Frames.Count * 2;
        level.Frames.AddRange(definition.AnimationFrames);

        foreach (TR3PackedAnimation packedAnimation in definition.Animations.Values)
        {
            packedAnimation.Animation.FrameOffset += definition.Model.FrameOffset;
        }
    }
}
