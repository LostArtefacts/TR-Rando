using System.Collections.Generic;
using System.Linq;
using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.Utilities;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TREnvironmentEditor.Model.Types
{
    public class EMMovePickupFunction : BaseEMFunction
    {
        public List<TR2Entities> Types { get; set; }
        public List<EMLocation> SectorLocations { get; set; }
        public EMLocation TargetLocation { get; set; }
        public bool MatchY { get; set; }

        public override void ApplyToLevel(TR2Level level)
        {
            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            // Store the sectors we are interested in
            Dictionary<TRRoomSector, EMLocation> sectors = new Dictionary<TRRoomSector, EMLocation>();
            foreach (EMLocation location in SectorLocations)
            {
                TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, location.Room, level, control);
                sectors[sector] = location;
            }

            // Scan for each entity type and if it's found, find its sector location. If it matches
            // any we are interested in, move the item to the new location. If we haven't defined a
            // manual target location, the one used to locate the sector will be used.
            List<TR2Entity> entities = level.Entities.ToList();
            foreach (TR2Entities entityType in Types)
            {
                List<TR2Entity> matchingEntities = entities.FindAll(e => e.TypeID == (short)entityType);
                foreach (TR2Entity match in matchingEntities)
                {
                    TRRoomSector matchSector = FDUtilities.GetRoomSector(match.X, match.Y, match.Z, match.Room, level, control);
                    // MatchY means the defined sector location's Y val should be compared with the entity's Y val, for
                    // instances where an item may be in mid-air (i.e. underwater) and another may be on the floor below it.
                    if (sectors.ContainsKey(matchSector) && (!MatchY || sectors[matchSector].Y == match.Y))
                    {
                        EMLocation location = TargetLocation ?? sectors[matchSector];
                        match.X = location.X;
                        match.Y = location.Y;
                        match.Z = location.Z;
                        match.Room = location.Room;
                    }
                }
            }
        }
    }
}