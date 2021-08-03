using System.Collections.Generic;
using TR2RandomizerCore.Helpers;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelReader.Model;

namespace TR2RandomizerCore.Environment.Types
{
    public class EMLadder : BaseEnvironmentModification
    {
        public Location Location { get; set; }
        public ushort LadderTexture { get; set; }
        public Dictionary<int, int[]> TileMap { get; set; }

        public override void ApplyToLevel(TR2Level level)
        {
            FDControl fdc = new FDControl();
            fdc.ParseFromLevel(level);

            // Rosetta: Collisional floordata functions should always come first in sequence, with floor
            // collision function strictly being first and ceiling collision function strictly being second.
            // The reason is hardcoded floordata collision parser which always expects these two functions
            // to be first in sequence.
            // ...so for now put the climb entry at 0 for testing in GW but FDControl should really validate
            // the order.
            TRRoomSector sector = FDUtilities.GetRoomSector(Location.X, Location.Y, Location.Z, (short)Location.Room, level, fdc);
            if (sector.FDIndex == 0)
            {
                fdc.CreateFloorData(sector);
            }

            fdc.Entries[sector.FDIndex].Insert(0, new FDClimbEntry
            {
                Setup = new FDSetup { Value = 518 } // Positive X (TODO: make these properties changeable)
            });

            fdc.WriteToLevel(level);

            // Unfortunately the ladder texture may not match the wall we are targeting, but
            // we can maybe look at generating a ladder texture with a transparent background for
            // each level, and then merging it with the wall in a particular room (similar to the way
            // landmarks are done), provided there is enough texture space.
            // TODO: automatically determine the wall faces to target from the floor of the room to the 
            // ceiling, and repeat for the room above (and so on) to avoid having to define manually.
            foreach (int roomIndex in TileMap.Keys)
            {
                foreach (int rectIndex in TileMap[roomIndex])
                {
                    level.Rooms[roomIndex].RoomData.Rectangles[rectIndex].Texture = LadderTexture;
                }
            }
        }
    }
}