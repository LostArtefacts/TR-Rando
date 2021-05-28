using System.Collections.Generic;
using TRLevelReader.Model;

namespace TRModelTransporter.Model
{
    public class PackedAnimation
    {
        public TRAnimation Animation { get; set; }
        public Dictionary<int, TRAnimDispatch> AnimationDispatches { get; set; }
        public Dictionary<int, PackedAnimationCommand> Commands { get; set; }
        public PackedSound Sound { get; set; }

        public List<TRStateChange> StateChanges { get; set; }

        public PackedAnimation()
        {
            AnimationDispatches = new Dictionary<int, TRAnimDispatch>();
            Commands = new Dictionary<int, PackedAnimationCommand>();
            Sound = new PackedSound();
            StateChanges = new List<TRStateChange>();
        }
    }
}