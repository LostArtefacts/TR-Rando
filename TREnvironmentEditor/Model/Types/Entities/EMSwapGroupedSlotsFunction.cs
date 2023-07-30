using System;
using System.Collections.Generic;
using System.Linq;
using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMSwapGroupedSlotsFunction : BaseEMFunction
    {
        public Dictionary<short, short> EntityMap { get; set; }

        public override void ApplyToLevel(TR1Level level)
        {
            EMLevelData data = GetData(level);
            InitialiseEntityMap(data);

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            Dictionary<short, SlotInfo> slotInfo = new Dictionary<short, SlotInfo>();
            foreach (short entityIndex in EntityMap.Keys)
            {
                TREntity entity = level.Entities[entityIndex];
                TRRoomSector sector = FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, level, floorData);
                slotInfo[entityIndex] = new SlotInfo
                {
                    Location = GetLocation(entity),
                    FDIndex = sector.FDIndex,
                    Triggers = floorData.Entries[sector.FDIndex].FindAll(e => e is FDTriggerEntry)
                };
            }

            foreach (short entityIndex in EntityMap.Keys)
            {
                SlotInfo slotInfo1 = slotInfo[entityIndex];
                SlotInfo slotInfo2 = slotInfo[EntityMap[entityIndex]];

                SwapTriggers(slotInfo1, slotInfo2, floorData);
                MoveSlot(level.Entities[entityIndex], slotInfo2.Location);
            }

            floorData.WriteToLevel(level);
        }

        public override void ApplyToLevel(TR2Level level)
        {
            EMLevelData data = GetData(level);
            InitialiseEntityMap(data);

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            Dictionary<short, SlotInfo> slotInfo = new Dictionary<short, SlotInfo>();
            foreach (short entityIndex in EntityMap.Keys)
            {
                TR2Entity entity = level.Entities[entityIndex];
                TRRoomSector sector = FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, level, floorData);
                slotInfo[entityIndex] = new SlotInfo
                {
                    Location = GetLocation(entity),
                    FDIndex = sector.FDIndex,
                    Triggers = floorData.Entries[sector.FDIndex].FindAll(e => e is FDTriggerEntry)
                };
            }

            foreach (short entityIndex in EntityMap.Keys)
            {
                SlotInfo slotInfo1 = slotInfo[entityIndex];
                SlotInfo slotInfo2 = slotInfo[EntityMap[entityIndex]];

                SwapTriggers(slotInfo1, slotInfo2, floorData);
                MoveSlot(level.Entities[entityIndex], slotInfo2.Location);
            }

            floorData.WriteToLevel(level);
        }

        public override void ApplyToLevel(TR3Level level)
        {
            EMLevelData data = GetData(level);
            InitialiseEntityMap(data);

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            Dictionary<short, SlotInfo> slotInfo = new Dictionary<short, SlotInfo>();
            foreach (short entityIndex in EntityMap.Keys)
            {
                TR2Entity entity = level.Entities[entityIndex];
                TRRoomSector sector = FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, level, floorData);
                slotInfo[entityIndex] = new SlotInfo
                {
                    Location = GetLocation(entity),
                    FDIndex = sector.FDIndex,
                    Triggers = floorData.Entries[sector.FDIndex].FindAll(e => e is FDTriggerEntry)
                };
            }

            foreach (short entityIndex in EntityMap.Keys)
            {
                SlotInfo slotInfo1 = slotInfo[entityIndex];
                SlotInfo slotInfo2 = slotInfo[EntityMap[entityIndex]];

                SwapTriggers(slotInfo1, slotInfo2, floorData);
                MoveSlot(level.Entities[entityIndex], slotInfo2.Location);
            }

            floorData.WriteToLevel(level);
        }

        private void InitialiseEntityMap(EMLevelData data)
        {
            if (!EntityMap.Keys.All(EntityMap.Values.Contains))
            {
                throw new ArgumentException("All values must also be defined as keys to collectively move grouped slots.");
            }

            Dictionary<short, short> remap = new Dictionary<short, short>();
            foreach (short entityIndex in EntityMap.Keys)
            {
                short index1 = data.ConvertEntity(entityIndex);
                short index2 = data.ConvertEntity(EntityMap[entityIndex]);
                if (index1 != index2)
                {
                    remap[index1] = index2;
                }
            }
            EntityMap = remap;
        }

        private void SwapTriggers(SlotInfo slotInfo1, SlotInfo slotInfo2, FDControl floorData)
        {
            floorData.Entries[slotInfo1.FDIndex].RemoveAll(slotInfo1.Triggers.Contains);
            floorData.Entries[slotInfo2.FDIndex].AddRange(slotInfo1.Triggers);
        }

        private void MoveSlot(TREntity entity, EMLocation location)
        {
            entity.X = location.X;
            entity.Y = location.Y;
            entity.Z = location.Z;
            entity.Room = location.Room;
            entity.Angle = location.Angle;
        }

        private void MoveSlot(TR2Entity entity, EMLocation location)
        {
            entity.X = location.X;
            entity.Y = location.Y;
            entity.Z = location.Z;
            entity.Room = location.Room;
            entity.Angle = location.Angle;
        }

        private EMLocation GetLocation(TREntity entity)
        {
            return new EMLocation
            {
                X = entity.X,
                Y = entity.Y,
                Z = entity.Z,
                Room = entity.Room,
                Angle = entity.Angle
            };
        }

        private EMLocation GetLocation(TR2Entity entity)
        {
            return new EMLocation
            {
                X = entity.X,
                Y = entity.Y,
                Z = entity.Z,
                Room = entity.Room,
                Angle = entity.Angle
            };
        }
    }

    public class SlotInfo
    {
        public EMLocation Location { get; set; }
        public ushort FDIndex { get; set; }
        public List<FDEntry> Triggers { get; set; }
    }
}
