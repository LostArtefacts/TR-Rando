using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR2Randomizer
{
    public static class ReplacementStatusManager
    {
        public static event EventHandler LevelProgressChanged;
        public static event EventHandler CanRandomizeChanged;

        private static int _LevelProgress;
        public static int LevelProgress 
        { 
            get { return _LevelProgress; } 
            set 
            { 
                if (value != _LevelProgress)
                {
                    _LevelProgress = value;

                    LevelProgressChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }

        private static bool _CanRandomize;
        public static bool CanRandomize 
        {
            get { return _CanRandomize; }
            set
            {
                if (value != _CanRandomize)
                {
                    _CanRandomize = value;

                    CanRandomizeChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }
    }
}
