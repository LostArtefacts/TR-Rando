using System;
using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Helpers;
using TRModelTransporter.Model;

namespace TRModelTransporter.Handlers
{
    public class AnimationTransportHandler : AbstractTransportHandler
    {
        private readonly SoundUnpacker _soundUnpacker;

        public AnimationTransportHandler()
        {
            _soundUnpacker = new SoundUnpacker();
        }

        #region Export
        public override void Export()
        {
            Definition.Animations = new Dictionary<int, PackedAnimation>();

            int endAnimation = GetModelAnimationCount(Level, Definition.Model) + Definition.Model.Animation;
            for (int animationIndex = Definition.Model.Animation; animationIndex < endAnimation; animationIndex++)
            {
                TRAnimation animation = Level.Animations[animationIndex];
                PackedAnimation packedAnimation = new PackedAnimation
                {
                    Animation = animation,
                };
                Definition.Animations[animationIndex] = packedAnimation;

                PackStateChanges(packedAnimation, animation);
                PackAnimCommands(packedAnimation, animation);
                PackAnimSounds(packedAnimation);
            }

            ExportAnimationFrames();
        }

        private void PackStateChanges(PackedAnimation packedAnimation, TRAnimation animation)
        {
            for (int stateChangeIndex = 0; stateChangeIndex < animation.NumStateChanges; stateChangeIndex++)
            {
                TRStateChange stateChange = Level.StateChanges[animation.StateChangeOffset + stateChangeIndex];
                packedAnimation.StateChanges.Add(stateChange);

                int dispatchOffset = stateChange.AnimDispatch;
                for (int i = 0; i < stateChange.NumAnimDispatches; i++, dispatchOffset++)
                {
                    if (!packedAnimation.AnimationDispatches.ContainsKey(dispatchOffset))
                    {
                        TRAnimDispatch dispatch = Level.AnimDispatches[dispatchOffset];
                        packedAnimation.AnimationDispatches[dispatchOffset] = dispatch;
                    }
                }
            }
        }

        private void PackAnimCommands(PackedAnimation packedAnimation, TRAnimation animation)
        {
            int cmdOffset = animation.AnimCommand;
            for (int i = 0; i < animation.NumAnimCommands; i++)
            {
                int cmdIndex = cmdOffset++;
                TRAnimCommand cmd = Level.AnimCommands[cmdIndex];

                int paramCount;
                switch ((TR2AnimCommand)cmd.Value)
                {
                    case TR2AnimCommand.SetPosition:
                        paramCount = 3;
                        break;
                    case TR2AnimCommand.JumpDistance:
                    case TR2AnimCommand.PlaySound:
                    case TR2AnimCommand.FlipEffect:
                        paramCount = 2;
                        break;
                    default:
                        paramCount = 0;
                        break;
                }

                short[] paramArr = new short[paramCount];
                for (int j = 0; j < paramCount; j++)
                {
                    paramArr[j] = Level.AnimCommands[cmdOffset++].Value;
                }

                packedAnimation.Commands[cmdIndex] = new PackedAnimationCommand
                {
                    Command = (TR2AnimCommand)cmd.Value,
                    Params = paramArr
                };
            }
        }

        private void PackAnimSounds(PackedAnimation packedAnimation)
        {
            foreach (PackedAnimationCommand cmd in packedAnimation.Commands.Values)
            {
                if (cmd.Command == TR2AnimCommand.PlaySound)
                {
                    int soundMapIndex = cmd.Params[1] & 0x3fff;
                    short soundDetailsIndex = Level.SoundMap[soundMapIndex];
                    packedAnimation.Sound.SoundMapIndices[soundMapIndex] = soundDetailsIndex;
                    if (soundDetailsIndex != -1)
                    {
                        TRSoundDetails soundDetails = Level.SoundDetails[soundDetailsIndex];
                        packedAnimation.Sound.SoundDetails[soundDetailsIndex] = soundDetails;

                        uint[] sampleIndices = new uint[soundDetails.NumSounds];
                        for (int i = 0; i < soundDetails.NumSounds; i++)
                        {
                            sampleIndices[i] = Level.SampleIndices[(ushort)(soundDetails.Sample + i)];
                        }

                        packedAnimation.Sound.SampleIndices[soundDetails.Sample] = sampleIndices;
                    }
                }
            }
        }

        private int GetModelAnimationCount(TR2Level level, TRModel model)
        {
            TRModel nextModel = model;
            int modelIndex = level.Models.ToList().IndexOf(model) + 1;
            while (modelIndex < level.NumModels)
            {
                nextModel = level.Models[modelIndex++];
                if (nextModel.Animation != ushort.MaxValue)
                {
                    break;
                }
            }

            ushort nextStartAnimation = nextModel.Animation;
            if (model == nextModel)
            {
                nextStartAnimation = (ushort)level.NumAnimations;
            }

            return model.Animation == ushort.MaxValue ? 0 : nextStartAnimation - model.Animation;
        }

        private void ExportAnimationFrames()
        {
            int modelIndex = Level.Models.ToList().IndexOf(Definition.Model);
            uint endFrame = 0;
            if (modelIndex == Level.NumModels - 1)
            {
                endFrame = Level.NumFrames;
            }
            else
            {
                while (endFrame == 0 && modelIndex < Level.NumModels)
                {
                    endFrame = Level.Models[++modelIndex].FrameOffset / 2;
                }
            }

            List<ushort> frames = new List<ushort>();
            for (uint i = Definition.Model.FrameOffset / 2; i < endFrame; i++)
            {
                frames.Add(Level.Frames[i]);
            }

            foreach (PackedAnimation anim in Definition.Animations.Values)
            {
                anim.Animation.FrameOffset -= Definition.Model.FrameOffset;
            }
            Definition.AnimationFrames = frames.ToArray();
        }
        #endregion

        #region Import
        public override void Import()
        {
            Dictionary<int, PackedAnimation> animations = Definition.Animations;
            bool firstAnimationConfigured = false;
            Dictionary<int, int> indexMap = new Dictionary<int, int>();
            foreach (int oldAnimationIndex in animations.Keys)
            {
                PackedAnimation packedAnimation = animations[oldAnimationIndex];
                UnpackStateChanges(packedAnimation);
                UnpackAnimSounds(packedAnimation);
                UnpackAnimCommands(packedAnimation);

                int newAnimationIndex = UnpackAnimation(packedAnimation);
                indexMap[oldAnimationIndex] = newAnimationIndex;

                if (!firstAnimationConfigured)
                {
                    Definition.Model.Animation = (ushort)newAnimationIndex;
                    firstAnimationConfigured = true;
                }
            }

            // Re-map the NextAnimations of each of the animation and dispatches
            // now we know the indices of each of the newly inserted animations.
            //List<short> missingAnimations = new List<short>();
            foreach (PackedAnimation packedAnimation in animations.Values)
            {
                packedAnimation.Animation.NextAnimation = (ushort)indexMap[packedAnimation.Animation.NextAnimation];
                foreach (TRAnimDispatch dispatch in packedAnimation.AnimationDispatches.Values)
                {
                    if (indexMap.ContainsKey(dispatch.NextAnimation))
                    {
                        dispatch.NextAnimation = (short)indexMap[dispatch.NextAnimation];
                    }
                    else
                    {
                        // I think this happens for such things as returning to a default stance e.g.
                        // after dismounting the Snowmobile, so it will be different in every level (e.g.
                        // Lara default animations).
                    }
                }
            }

            // Inserting SampleIndices will break the game unless they are sorted numerically
            // so handle this outwith the main animation insertion loop for ease.
            ResortSoundIndices();

            ImportAnimationFrames();
        }

        private void UnpackStateChanges(PackedAnimation packedAnimation)
        {
            if (packedAnimation.Animation.NumStateChanges == 0)
            {
                if (packedAnimation.AnimationDispatches.Count != 0)
                {
                    throw new Exception();
                }
                return;
            }

            // Import the AnimDispatches first, noting their new indices
            List<TRAnimDispatch> animDispatches = Level.AnimDispatches.ToList();
            Dictionary<int, int> indexMap = new Dictionary<int, int>();
            foreach (int oldDispatchIndex in packedAnimation.AnimationDispatches.Keys)
            {
                TRAnimDispatch dispatch = packedAnimation.AnimationDispatches[oldDispatchIndex];
                indexMap[oldDispatchIndex] = animDispatches.Count;
                animDispatches.Add(dispatch);
                // The dispatch's NextAnimation will need to be remapped, but this is handled in Import above
                // once all animations are in place.
            }

            // The animation's StateChangeOffset will be the current length of level StateChanges
            List<TRStateChange> stateChanges = Level.StateChanges.ToList();
            packedAnimation.Animation.StateChangeOffset = (ushort)stateChanges.Count;

            // Import Each state change, re-mapping AnimDispatch to new index
            foreach (TRStateChange stateChange in packedAnimation.StateChanges)
            {
                stateChange.AnimDispatch = (ushort)indexMap[stateChange.AnimDispatch];
                stateChanges.Add(stateChange);
            }

            // Save back to the level
            Level.AnimDispatches = animDispatches.ToArray();
            Level.NumAnimDispatches = (uint)animDispatches.Count;

            Level.StateChanges = stateChanges.ToArray();
            Level.NumStateChanges = (uint)stateChanges.Count;
        }

        /**
         * SampleIndices has to remain in numerical order, but rather than dealing with it after inserting
         * each new sample, ImportAnimations will handle reorganising the list and remapping SoundDetails
         * as necessary.
         */
        private void UnpackAnimSounds(PackedAnimation packedAnimation)
        {
            _soundUnpacker.Unpack(packedAnimation.Sound, Level, false);
            IReadOnlyDictionary<int, int> soundIndexMap = _soundUnpacker.SoundIndexMap;

            // Change the Params[1] value of each PlaySound AnimCommand to point to the
            // new index in SoundMap.
            foreach (PackedAnimationCommand cmd in packedAnimation.Commands.Values)
            {
                if (cmd.Command == TR2AnimCommand.PlaySound)
                {
                    int oldSoundMapIndex = cmd.Params[1] & 0x3fff;
                    int newSoundMapIndex = soundIndexMap[oldSoundMapIndex];

                    int param = cmd.Params[1] & ~oldSoundMapIndex;
                    param |= newSoundMapIndex;
                    cmd.Params[1] = (short)param;
                }
            }
        }

        /**
         * Commands are packed into CmdType + Params, but these are just 
         * translated straight back into normal TRAnimCommands.
         */
        private void UnpackAnimCommands(PackedAnimation packedAnimation)
        {
            if (packedAnimation.Commands.Count == 0)
            {
                return;
            }

            List<TRAnimCommand> levelAnimCommands = Level.AnimCommands.ToList();
            packedAnimation.Animation.AnimCommand = (ushort)levelAnimCommands.Count;
            foreach (PackedAnimationCommand cmd in packedAnimation.Commands.Values)
            {
                levelAnimCommands.Add(new TRAnimCommand { Value = (short)cmd.Command });
                foreach (short param in cmd.Params)
                {
                    levelAnimCommands.Add(new TRAnimCommand { Value = param });
                }
            }

            // Save back to the level
            Level.AnimCommands = levelAnimCommands.ToArray();
            Level.NumAnimCommands = (uint)levelAnimCommands.Count;
        }

        private int UnpackAnimation(PackedAnimation animation)
        {
            List<TRAnimation> levelAnimations = Level.Animations.ToList();
            levelAnimations.Add(animation.Animation);
            Level.Animations = levelAnimations.ToArray();
            Level.NumAnimations++;

            return levelAnimations.Count - 1;
        }

        private void ResortSoundIndices()
        {
            // Store the values from SampleIndices against their current positions
            // in the list.
            List<uint> sampleIndices = Level.SampleIndices.ToList();
            Dictionary<int, uint> indexMap = new Dictionary<int, uint>();
            for (int i = 0; i < sampleIndices.Count; i++)
            {
                indexMap[i] = sampleIndices[i];
            }

            // Sort the indices to avoid the game crashing
            sampleIndices.Sort();

            // Remap each SoundDetail to use the new index of the sample it points to
            foreach (TRSoundDetails soundDetails in Level.SoundDetails)
            {
                soundDetails.Sample = (ushort)sampleIndices.IndexOf(indexMap[soundDetails.Sample]);
            }

            // Save the samples back to the level
            Level.SampleIndices = sampleIndices.ToArray();

            // Repeat for SoundMap -> SoundDetails
            Dictionary<int, TRSoundDetails> soundMapIndices = new Dictionary<int, TRSoundDetails>();
            List<short> soundMap = Level.SoundMap.ToList();
            for (int i = 0; i < soundMap.Count; i++)
            {
                if (soundMap[i] != -1)
                {
                    soundMapIndices[i] = Level.SoundDetails[soundMap[i]];
                }
            }

            List<TRSoundDetails> soundDetailsList = Level.SoundDetails.ToList();
            soundDetailsList.Sort(delegate (TRSoundDetails d1, TRSoundDetails d2)
            {
                return d1.Sample.CompareTo(d2.Sample);
            });

            foreach (int mapIndex in soundMapIndices.Keys)
            {
                TRSoundDetails details = soundMapIndices[mapIndex];
                soundMap[mapIndex] = (short)soundDetailsList.IndexOf(details);
            }

            Level.SoundDetails = soundDetailsList.ToArray();
            Level.SoundMap = soundMap.ToArray();
        }

        private void ImportAnimationFrames()
        {
            List<ushort> levelFrames = Level.Frames.ToList();
            Definition.Model.FrameOffset = (uint)levelFrames.Count * 2;

            levelFrames.AddRange(Definition.AnimationFrames);
            Level.Frames = levelFrames.ToArray();
            Level.NumFrames = (uint)levelFrames.Count;

            foreach (PackedAnimation packedAnimation in Definition.Animations.Values)
            {
                packedAnimation.Animation.FrameOffset += Definition.Model.FrameOffset;
            }
        }
        #endregion
    }
}