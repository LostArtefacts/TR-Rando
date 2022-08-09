using System;
using System.Collections.Generic;
using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMLadderFunction : EMRefaceFunction
    {
        public EMLocation Location { get; set; }
        public bool IsPositiveX { get; set; }
        public bool IsPositiveZ { get; set; }
        public bool IsNegativeX { get; set; }
        public bool IsNegativeZ { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            throw new NotSupportedException();
        }

        public override void ApplyToLevel(TR2Level level)
        {
            EMLevelData data = GetData(level);

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            TRRoomSector sector = FDUtilities.GetRoomSector(Location.X, Location.Y, Location.Z, data.ConvertRoom(Location.Room), level, control);
            ModifyLadder(sector, control);

            control.WriteToLevel(level);

            // Unfortunately the ladder texture may not match the walls we are targeting, but
            // we can maybe look at generating a ladder texture with a transparent background for
            // each level, and then merging it with the wall in a particular room (similar to the way
            // landmarks are done), provided there is enough texture space.
            ApplyTextures(level);
        }

        public override void ApplyToLevel(TR3Level level)
        {
            EMLevelData data = GetData(level);

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            TRRoomSector sector = FDUtilities.GetRoomSector(Location.X, Location.Y, Location.Z, data.ConvertRoom(Location.Room), level, control);
            ModifyLadder(sector, control);

            control.WriteToLevel(level);

            ApplyTextures(level);
        }

        private void ModifyLadder(TRRoomSector sector, FDControl control)
        {
            // Rosetta: Collisional floordata functions should always come first in sequence, with floor
            // collision function strictly being first and ceiling collision function strictly being second.
            // The reason is hardcoded floordata collision parser which always expects these two functions
            // to be first in sequence.
            // ...so for now put the climb entry at 0 for testing in GW but FDControl should really validate
            // the order.

            bool removeAll = !IsPositiveX && !IsPositiveZ && !IsNegativeX && !IsNegativeZ;

            if (!removeAll && sector.FDIndex == 0)
            {
                control.CreateFloorData(sector);
            }

            if (removeAll)
            {
                if (sector.FDIndex != 0)
                {
                    // remove the climbable entry and if it leaves an empty list, remove the FD
                    List<FDEntry> entries = control.Entries[sector.FDIndex];
                    entries.RemoveAll(e => e is FDClimbEntry);
                    if (entries.Count == 0)
                    {
                        // If there isn't anything left, reset the sector to point to the dummy FD
                        control.RemoveFloorData(sector);
                    }
                }
            }
            else
            {
                FDClimbEntry climbEntry = new FDClimbEntry
                {
                    Setup = new FDSetup(FDFunctions.ClimbableWalls),
                    IsPositiveX = IsPositiveX,
                    IsPositiveZ = IsPositiveZ,
                    IsNegativeX = IsNegativeX,
                    IsNegativeZ = IsNegativeZ
                };

                // We have to add climbable entries after portal, slant and kill Lara entries.
                List<FDEntry> entries = control.Entries[sector.FDIndex];
                int index = entries.FindLastIndex(e => e is FDPortalEntry || e is FDSlantEntry || e is FDKillLaraEntry);

                control.Entries[sector.FDIndex].Insert(index + 1, climbEntry);
            }
        }
    }
}