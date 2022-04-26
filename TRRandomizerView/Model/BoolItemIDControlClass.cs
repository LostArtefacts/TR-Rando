using System;

namespace TRRandomizerView.Model
{
    public class BoolItemIDControlClass : BoolItemControlClass, ICloneable
    {
        public int ID { get; set; }

        public BoolItemIDControlClass Clone()
        {
            return (BoolItemIDControlClass)MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}