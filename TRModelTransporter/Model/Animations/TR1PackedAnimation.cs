using System.Collections.Generic;
using TRLevelReader.Model;
using TRModelTransporter.Model.Sound;

namespace TRModelTransporter.Model.Animations
{
    public class TR1PackedAnimation
    {
        public TRAnimation Animation { get; set; }
        public Dictionary<int, TRAnimDispatch> AnimationDispatches { get; set; }
        public Dictionary<int, TR1PackedAnimationCommand> Commands { get; set; }
        public TR1PackedSound Sound { get; set; }

        public List<TRStateChange> StateChanges { get; set; }

        public TR1PackedAnimation()
        {
            AnimationDispatches = new Dictionary<int, TRAnimDispatch>();
            Commands = new Dictionary<int, TR1PackedAnimationCommand>();
            Sound = new TR1PackedSound();
            StateChanges = new List<TRStateChange>();
        }
    }
}