﻿using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMRemoveTriggerFunction : BaseEMFunction
{
    public List<EMLocation> Locations { get; set; }
    public List<int> Rooms { get; set; }
    public List<FDTrigType> TrigTypes { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);

        FDControl control = new();
        control.ParseFromLevel(level);

        if (Locations != null)
        {
            foreach (EMLocation location in Locations)
            {
                TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, control);
                RemoveSectorTriggers(sector, control);
            }
        }

        if (Rooms != null)
        {
            foreach (int roomNumber in Rooms)
            {
                RemoveSectorListTriggers(level.Rooms[data.ConvertRoom(roomNumber)].Sectors, control);
            }
        }

        control.WriteToLevel(level);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);

        FDControl control = new();
        control.ParseFromLevel(level);

        if (Locations != null)
        {
            foreach (EMLocation location in Locations)
            {
                TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, control);
                RemoveSectorTriggers(sector, control);
            }
        }

        if (Rooms != null)
        {
            foreach (int roomNumber in Rooms)
            {
                RemoveSectorListTriggers(level.Rooms[data.ConvertRoom(roomNumber)].SectorList, control);
            }
        }

        control.WriteToLevel(level);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);

        FDControl control = new();
        control.ParseFromLevel(level);

        if (Locations != null)
        {
            foreach (EMLocation location in Locations)
            {
                TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, control);
                RemoveSectorTriggers(sector, control);
            }
        }

        if (Rooms != null)
        {
            foreach (int roomNumber in Rooms)
            {
                RemoveSectorListTriggers(level.Rooms[data.ConvertRoom(roomNumber)].Sectors, control);
            }
        }

        control.WriteToLevel(level);
    }

    private void RemoveSectorListTriggers(IEnumerable<TRRoomSector> sectors, FDControl control)
    {
        foreach (TRRoomSector sector in sectors)
        {
            RemoveSectorTriggers(sector, control);
        }
    }

    private void RemoveSectorTriggers(TRRoomSector sector, FDControl control)
    {
        if (sector.FDIndex == 0)
        {
            return;
        }

        List<FDEntry> entries = control.Entries[sector.FDIndex];
        entries.RemoveAll(e => e is FDTriggerEntry trig && (TrigTypes?.Contains(trig.TrigType) ?? true));
        if (entries.Count == 0)
        {
            // If there isn't anything left, reset the sector to point to the dummy FD
            control.RemoveFloorData(sector);
        }
    }
}
