using System.Collections.Generic;
using TRLevelReader.Model.Base.Enums;

namespace TRLevelReader.Model
{
    public class TRZoneGroup : Dictionary<FlipStatus, TRZone>
    {
        /// <summary>
        /// Zone values when flipmap is off.
        /// </summary>
        public TRZone NormalZone
        {
            get => this[FlipStatus.Off];
            set => this[FlipStatus.Off] = value;
        }
        /// <summary>
        /// Zone values when flipmap is on.
        /// </summary>
        public TRZone AlternateZone
        {
            get => this[FlipStatus.On];
            set => this[FlipStatus.On] = value;
        }
    }
}