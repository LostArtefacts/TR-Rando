using System.Collections.Generic;
using System.Linq;
using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TREnvironmentEditor.Model.Types
{
    public class EMMoveSlotFunction : BaseEMFunction
    {
        public int EntityIndex { get; set; }
        public EMLocation Location { get; set; }

        public override void ApplyToLevel(TR2Level level)
        {
            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            TR2Entity slot = level.Entities[EntityIndex];
            TRRoomSector currentSector = FDUtilities.GetRoomSector(slot.X, slot.Y, slot.Z, slot.Room, level, control);
            TRRoomSector newSector = FDUtilities.GetRoomSector(Location.X, Location.Y, Location.Z, Location.Room, level, control);

            // Make sure there isn't a static enemy on the same sector e.g. MorayEel
            List<TR2Entity> staticEnemies = level.Entities.ToList().FindAll(e => e.Room == Location.Room && TR2EntityUtilities.IsStaticCreature((TR2Entities)e.TypeID));
            foreach (TR2Entity staticEnemy in staticEnemies)
            {
                TRRoomSector enemySector = FDUtilities.GetRoomSector(staticEnemy.X, staticEnemy.Y, staticEnemy.Z, staticEnemy.Room, level, control);
                if (enemySector == newSector)
                {
                    // Bail out
                    return;
                }
            }

            slot.X = Location.X;
            slot.Y = Location.Y;
            slot.Z = Location.Z;
            slot.Room = Location.Room;
            slot.Angle = Location.Angle;

            if (newSector != currentSector && currentSector.FDIndex != 0)
            {
                if (newSector.FDIndex == 0)
                {
                    control.CreateFloorData(newSector);
                }

                bool keyTrigPredicate(FDEntry e) => e is FDTriggerEntry trig && trig.SwitchOrKeyRef == EntityIndex;

                // Copy the key trigger to the new sector
                List<FDEntry> keyTriggers = control.Entries[currentSector.FDIndex].FindAll(keyTrigPredicate);
                control.Entries[newSector.FDIndex].AddRange(keyTriggers);
                // Remove from the old
                control.Entries[currentSector.FDIndex].RemoveAll(keyTrigPredicate);
                if (control.Entries[currentSector.FDIndex].Count == 0)
                {
                    // If there isn't anything left, reset the sector to point to the dummy FD
                    control.RemoveFloorData(currentSector);
                }

                control.WriteToLevel(level);
            }
        }
    }
}