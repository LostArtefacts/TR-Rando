using System.Collections.Generic;
using TRLevelReader.Model;

namespace TRModelTransporter.Model
{
    public class PackedAnimation
    {
        public TRAnimation Animation { get; set; }
        public Dictionary<int, TRAnimDispatch> AnimationDispatches { get; set; }
        public Dictionary<int, PackedAnimationCommand> Commands { get; set; }
        public Dictionary<ushort, uint[]> SampleIndices { get; set; }
        public Dictionary<int, TRSoundDetails> SoundDetails { get; set; }
        public Dictionary<int, short> SoundMapIndices { get; set; }

        public List<TRStateChange> StateChanges { get; set; }

        public PackedAnimation()
        {
            AnimationDispatches = new Dictionary<int, TRAnimDispatch>();
            Commands = new Dictionary<int, PackedAnimationCommand>();
            SampleIndices = new Dictionary<ushort, uint[]>();
            SoundDetails = new Dictionary<int, TRSoundDetails>();
            SoundMapIndices = new Dictionary<int, short>();
            StateChanges = new List<TRStateChange>();
        }
    }
}