using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRGE.Core;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers
{
    public class TR3ItemRandomizer : BaseTR3Randomizer
    {
        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            foreach (TR3ScriptedLevel lvl in Levels)
            {
                //Replace with UI option in future
                Settings.RandomizeItemTypes = true;
                Settings.RandomizeItemPositions = true;

                LoadLevelInstance(lvl);

                if (Settings.RandomizeItemTypes)
                    RandomizeItemTypes(_levelInstance);

                if (Settings.RandomizeItemPositions)
                    RandomizeItemLocations(_levelInstance);

                if (Settings.RandoItemDifficulty == ItemDifficulty.OneLimit)
                    EnforceOneLimit(_levelInstance);

                if (Settings.IncludeKeyItems)
                    RandomizeKeyItems(_levelInstance);

                SaveLevelInstance();

                if (!TriggerProgress())
                {
                    break;
                }
            }
        }

        public void RandomizeItemTypes(TR3CombinedLevel level)
        {
            List<TR3Entities> stdItemTypes = TR3EntityUtilities.GetStandardPickupTypes();

            foreach (TR2Entity ent in level.Data.Entities)
            {
                if (TR3EntityUtilities.IsStandardPickupType((TR3Entities)ent.TypeID) && 
                    (ent.Room < RoomWaterUtilities.DefaultRoomCountDictionary[level.Name] || Settings.RandomizeSecretRewardsPhysical))
                {
                    ent.TypeID = (short)stdItemTypes[_generator.Next(0, (stdItemTypes.Count - 1))];
                }
            }
        }

        public void EnforceOneLimit(TR3CombinedLevel level)
        {
            List<TR3Entities> oneOfEachType = new List<TR3Entities>();
            List<TR2Entity> allEntities = _levelInstance.Data.Entities.ToList();

            // look for extra utility/ammo items and hide them
            foreach (TR2Entity ent in allEntities)
            {
                TR3Entities eType = (TR3Entities)ent.TypeID;

                if (TR3EntityUtilities.IsStandardPickupType(eType) ||
                    TR3EntityUtilities.IsCrystalPickup(eType))
                {
                    if (oneOfEachType.Contains(eType))
                    {
                        ItemUtilities.HideEntity(ent);
                    }
                    else
                    {
                        oneOfEachType.Add((TR3Entities)ent.TypeID);
                    }     
                }
            }
        }

        public void RandomizeItemLocations(TR3CombinedLevel level)
        {
            //ToDo
        }

        public void RandomizeKeyItems(TR3CombinedLevel level)
        {
            //ToDo
        }

        private bool IsKeyItemUsedForSecret(TR2Entity ent)
        {
            return false;
        }
    }
}
