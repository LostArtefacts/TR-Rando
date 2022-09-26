using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Model;
using TRModelTransporter.Model.Animations;
using TRModelTransporter.Model.Definitions;

namespace TRModelTransporter.Handlers
{
    public class AnimationTransportHandler
    {
        public void Export(TRLevel level, TR1ModelDefinition definition)
        {
            definition.Animations = new Dictionary<int, TR1PackedAnimation>();

            int endAnimation = AnimationUtilities.GetModelAnimationCount(level, definition.Model) + definition.Model.Animation;
            for (int animationIndex = definition.Model.Animation; animationIndex < endAnimation; animationIndex++)
            {
                TRAnimation animation = level.Animations[animationIndex];
                TR1PackedAnimation packedAnimation = new TR1PackedAnimation
                {
                    Animation = animation,
                };
                definition.Animations[animationIndex] = packedAnimation;

                AnimationUtilities.PackStateChanges(level, animation, packedAnimation);
                AnimationUtilities.PackAnimCommands(level, animation, packedAnimation);
                AnimationUtilities.PackAnimSounds(level, packedAnimation);
            }

            foreach (TR1PackedAnimation anim in definition.Animations.Values)
            {
                anim.Animation.FrameOffset -= definition.Model.FrameOffset;
            }
            definition.AnimationFrames = AnimationUtilities.GetAnimationFrames(level, definition.Model);
        }

        public void Export(TR2Level level, TR2ModelDefinition definition)
        {
            definition.Animations = new Dictionary<int, TR2PackedAnimation>();

            int endAnimation = AnimationUtilities.GetModelAnimationCount(level, definition.Model) + definition.Model.Animation;
            for (int animationIndex = definition.Model.Animation; animationIndex < endAnimation; animationIndex++)
            {
                TRAnimation animation = level.Animations[animationIndex];
                TR2PackedAnimation packedAnimation = new TR2PackedAnimation
                {
                    Animation = animation,
                };
                definition.Animations[animationIndex] = packedAnimation;

                AnimationUtilities.PackStateChanges(level, animation, packedAnimation);
                AnimationUtilities.PackAnimCommands(level, animation, packedAnimation);
                AnimationUtilities.PackAnimSounds(level, packedAnimation);
            }

            foreach (TR2PackedAnimation anim in definition.Animations.Values)
            {
                anim.Animation.FrameOffset -= definition.Model.FrameOffset;
            }
            definition.AnimationFrames = AnimationUtilities.GetAnimationFrames(level, definition.Model);
        }

        public void Export(TR3Level level, TR3ModelDefinition definition)
        {
            definition.Animations = new Dictionary<int, TR3PackedAnimation>();

            int endAnimation = AnimationUtilities.GetModelAnimationCount(level, definition.Model) + definition.Model.Animation;
            for (int animationIndex = definition.Model.Animation; animationIndex < endAnimation; animationIndex++)
            {
                TRAnimation animation = level.Animations[animationIndex];
                TR3PackedAnimation packedAnimation = new TR3PackedAnimation
                {
                    Animation = animation,
                };
                definition.Animations[animationIndex] = packedAnimation;

                AnimationUtilities.PackStateChanges(level, animation, packedAnimation);
                AnimationUtilities.PackAnimCommands(level, animation, packedAnimation);
                AnimationUtilities.PackAnimSounds(level, packedAnimation);
            }

            foreach (TR3PackedAnimation anim in definition.Animations.Values)
            {
                anim.Animation.FrameOffset -= definition.Model.FrameOffset;
            }
            definition.AnimationFrames = AnimationUtilities.GetAnimationFrames(level, definition.Model);
        }

        public void Import(TRLevel level, TR1ModelDefinition definition)
        {
            Dictionary<int, TR1PackedAnimation> animations = definition.Animations;
            bool firstAnimationConfigured = false;
            Dictionary<int, int> indexMap = new Dictionary<int, int>();

            List<TRAnimDispatch> animDispatches = level.AnimDispatches.ToList();
            List<TRStateChange> stateChanges = level.StateChanges.ToList();
            List<TRAnimCommand> animCommands = level.AnimCommands.ToList();

            foreach (int oldAnimationIndex in animations.Keys)
            {
                TR1PackedAnimation packedAnimation = animations[oldAnimationIndex];
                AnimationUtilities.UnpackStateChanges(animDispatches, stateChanges, packedAnimation);
                AnimationUtilities.UnpackAnimSounds(level, packedAnimation);
                AnimationUtilities.UnpackAnimCommands(animCommands, packedAnimation);

                int newAnimationIndex = AnimationUtilities.UnpackAnimation(level, packedAnimation);
                indexMap[oldAnimationIndex] = newAnimationIndex;

                if (!firstAnimationConfigured)
                {
                    definition.Model.Animation = (ushort)newAnimationIndex;
                    firstAnimationConfigured = true;
                }
            }

            level.AnimDispatches = animDispatches.ToArray();
            level.NumAnimDispatches = (uint)animDispatches.Count;

            level.StateChanges = stateChanges.ToArray();
            level.NumStateChanges = (uint)stateChanges.Count;

            level.AnimCommands = animCommands.ToArray();
            level.NumAnimCommands = (uint)animCommands.Count;

            // Re-map the NextAnimations of each of the animation and dispatches
            // now we know the indices of each of the newly inserted animations.
            foreach (TR1PackedAnimation packedAnimation in animations.Values)
            {
                packedAnimation.Animation.NextAnimation = (ushort)indexMap[packedAnimation.Animation.NextAnimation];
                foreach (TRAnimDispatch dispatch in packedAnimation.AnimationDispatches.Values)
                {
                    if (indexMap.ContainsKey(dispatch.NextAnimation))
                    {
                        dispatch.NextAnimation = (short)indexMap[dispatch.NextAnimation];
                    }
                }
            }

            SoundUtilities.ResortSoundIndices(level);

            AnimationUtilities.ImportAnimationFrames(level, definition);
        }

        public void Import(TR2Level level, TR2ModelDefinition definition)
        {
            Dictionary<int, TR2PackedAnimation> animations = definition.Animations;
            bool firstAnimationConfigured = false;
            Dictionary<int, int> indexMap = new Dictionary<int, int>();

            List<TRAnimDispatch> animDispatches = level.AnimDispatches.ToList();
            List<TRStateChange> stateChanges = level.StateChanges.ToList();
            List<TRAnimCommand> animCommands = level.AnimCommands.ToList();

            foreach (int oldAnimationIndex in animations.Keys)
            {
                TR2PackedAnimation packedAnimation = animations[oldAnimationIndex];
                AnimationUtilities.UnpackStateChanges(animDispatches, stateChanges, packedAnimation);
                AnimationUtilities.UnpackAnimSounds(level, packedAnimation);
                AnimationUtilities.UnpackAnimCommands(animCommands, packedAnimation);

                int newAnimationIndex = AnimationUtilities.UnpackAnimation(level, packedAnimation);
                indexMap[oldAnimationIndex] = newAnimationIndex;

                if (!firstAnimationConfigured)
                {
                    definition.Model.Animation = (ushort)newAnimationIndex;
                    firstAnimationConfigured = true;
                }
            }

            level.AnimDispatches = animDispatches.ToArray();
            level.NumAnimDispatches = (uint)animDispatches.Count;

            level.StateChanges = stateChanges.ToArray();
            level.NumStateChanges = (uint)stateChanges.Count;

            level.AnimCommands = animCommands.ToArray();
            level.NumAnimCommands = (uint)animCommands.Count;

            foreach (TR2PackedAnimation packedAnimation in animations.Values)
            {
                packedAnimation.Animation.NextAnimation = (ushort)indexMap[packedAnimation.Animation.NextAnimation];
                foreach (TRAnimDispatch dispatch in packedAnimation.AnimationDispatches.Values)
                {
                    if (indexMap.ContainsKey(dispatch.NextAnimation))
                    {
                        dispatch.NextAnimation = (short)indexMap[dispatch.NextAnimation];
                    }
                }
            }

            // Inserting SampleIndices will break the game unless they are sorted numerically
            // so handle this outwith the main animation insertion loop for ease.
            SoundUtilities.ResortSoundIndices(level);

            AnimationUtilities.ImportAnimationFrames(level, definition);
        }

        public void Import(TR3Level level, TR3ModelDefinition definition)
        {
            Dictionary<int, TR3PackedAnimation> animations = definition.Animations;
            bool firstAnimationConfigured = false;
            Dictionary<int, int> indexMap = new Dictionary<int, int>();

            List<TRAnimDispatch> animDispatches = level.AnimDispatches.ToList();
            List<TRStateChange> stateChanges = level.StateChanges.ToList();
            List<TRAnimCommand> animCommands = level.AnimCommands.ToList();

            foreach (int oldAnimationIndex in animations.Keys)
            {
                TR3PackedAnimation packedAnimation = animations[oldAnimationIndex];
                AnimationUtilities.UnpackStateChanges(animDispatches, stateChanges, packedAnimation);
                AnimationUtilities.UnpackAnimSounds(level, packedAnimation);
                AnimationUtilities.UnpackAnimCommands(animCommands, packedAnimation);

                int newAnimationIndex = AnimationUtilities.UnpackAnimation(level, packedAnimation);
                indexMap[oldAnimationIndex] = newAnimationIndex;

                if (!firstAnimationConfigured)
                {
                    definition.Model.Animation = (ushort)newAnimationIndex;
                    firstAnimationConfigured = true;
                }
            }

            level.AnimDispatches = animDispatches.ToArray();
            level.NumAnimDispatches = (uint)animDispatches.Count;

            level.StateChanges = stateChanges.ToArray();
            level.NumStateChanges = (uint)stateChanges.Count;

            level.AnimCommands = animCommands.ToArray();
            level.NumAnimCommands = (uint)animCommands.Count;

            foreach (TR3PackedAnimation packedAnimation in animations.Values)
            {
                if (indexMap.ContainsKey(packedAnimation.Animation.NextAnimation))
                {
                    packedAnimation.Animation.NextAnimation = (ushort)indexMap[packedAnimation.Animation.NextAnimation];
                }
                foreach (TRAnimDispatch dispatch in packedAnimation.AnimationDispatches.Values)
                {
                    if (indexMap.ContainsKey(dispatch.NextAnimation))
                    {
                        dispatch.NextAnimation = (short)indexMap[dispatch.NextAnimation];
                    }
                }
            }

            SoundUtilities.ResortSoundIndices(level);

            AnimationUtilities.ImportAnimationFrames(level, definition);
        }
    }
}