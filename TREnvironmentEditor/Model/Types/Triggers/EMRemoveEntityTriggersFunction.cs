using System.Collections.Generic;
using System.Linq;
using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model
{
    public class EMRemoveEntityTriggersFunction : BaseEMFunction
    {
        public List<int> Entities { get; set; }
        public List<FDTrigType> ExcludedTypes { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            EMLevelData data = GetData(level);
            List<int> entities = Entities.Select(e => (int)data.ConvertEntity(e)).ToList();

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            foreach (TRRoom room in level.Rooms)
            {
                RemoveSectorTriggers(floorData, room.Sectors, entities);
            }

            floorData.WriteToLevel(level);
        }

        public override void ApplyToLevel(TR2Level level)
        {
            EMLevelData data = GetData(level);
            List<int> entities = Entities.Select(e => (int)data.ConvertEntity(e)).ToList();

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            foreach (TR2Room room in level.Rooms)
            {
                RemoveSectorTriggers(floorData, room.SectorList, entities);
            }

            floorData.WriteToLevel(level);
        }

        public override void ApplyToLevel(TR3Level level)
        {
            EMLevelData data = GetData(level);
            List<int> entities = Entities.Select(e => (int)data.ConvertEntity(e)).ToList();

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            foreach (TR3Room room in level.Rooms)
            {
                RemoveSectorTriggers(floorData, room.Sectors, entities);
            }

            floorData.WriteToLevel(level);
        }

        private void RemoveSectorTriggers(FDControl floorData, IEnumerable<TRRoomSector> sectors, List<int> entities)
        {
            foreach (TRRoomSector sector in sectors)
            {
                if (sector.FDIndex == 0)
                {
                    continue;
                }

                List<FDEntry> entries = floorData.Entries[sector.FDIndex];
                if (entries.Find(e => e is FDTriggerEntry) is FDTriggerEntry trigger
                    && (!ExcludedTypes?.Contains(trigger.TrigType) ?? true))
                {
                    trigger.TrigActionList.RemoveAll(a => a.TrigAction == FDTrigAction.Object && entities.Contains(a.Parameter));
                    if (trigger.TrigActionList.Count == 0)
                    {
                        entries.Remove(trigger);
                    }

                    if (entries.Count == 0)
                    {
                        floorData.RemoveFloorData(sector);
                    }
                }
            }
        }
    }
}
