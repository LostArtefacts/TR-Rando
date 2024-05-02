using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMLadderFunction : EMRefaceFunction
{
    public EMLocation Location { get; set; }
    public bool IsPositiveX { get; set; }
    public bool IsPositiveZ { get; set; }
    public bool IsNegativeX { get; set; }
    public bool IsNegativeZ { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        throw new NotSupportedException();
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);

        TRRoomSector sector = level.FloorData.GetRoomSector(Location.X, Location.Y, Location.Z, data.ConvertRoom(Location.Room), level);
        ModifyLadder(sector, level.FloorData);

        base.ApplyToLevel(level);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);

        TRRoomSector sector = level.FloorData.GetRoomSector(Location.X, Location.Y, Location.Z, data.ConvertRoom(Location.Room), level);
        ModifyLadder(sector, level.FloorData);

        base.ApplyToLevel(level);
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
                List<FDEntry> entries = control[sector.FDIndex];
                entries.RemoveAll(e => e is FDClimbEntry);
            }
        }
        else
        {
            FDClimbEntry climbEntry = new()
            {
                IsPositiveX = IsPositiveX,
                IsPositiveZ = IsPositiveZ,
                IsNegativeX = IsNegativeX,
                IsNegativeZ = IsNegativeZ
            };

            // We have to add climbable entries after portal, slant and kill Lara entries.
            List<FDEntry> entries = control[sector.FDIndex];
            int index = entries.FindLastIndex(e => e is FDPortalEntry || e is FDSlantEntry || e is FDKillLaraEntry || e is FDTriangulationEntry);

            control[sector.FDIndex].Insert(index + 1, climbEntry);
        }
    }
}
