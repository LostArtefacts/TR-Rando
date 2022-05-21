using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Animation;
using TR2Randomizer.Utilities;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TR2Randomizer.Randomizers
{
    public class EnemyRandomizer : RandomizerBase
    {
        public EnemyRandomizer() : base()
        {
        }

        public override void Randomize(int seed)
        {
            ReplacementStatusManager.CanRandomize = false;

            _generator = new Random(seed);

            foreach (string lvl in _levels)
            {
                //Read the level into a level object
                _levelInstance = LoadLevel(lvl);

                //Apply the modifications
                RandomizeEnemyTypes(lvl);

                //Write back the level file
                SaveLevel(_levelInstance, lvl);

                ReplacementStatusManager.LevelProgress++;
            }

            ReplacementStatusManager.LevelProgress = 0;
            ReplacementStatusManager.CanRandomize = true;
        }

        private void RandomizeEnemyTypes(string lvl)
        {
            List<TR2Entities> EnemyTypes = TR2EntityUtilities.GetEnemyTypeDictionary()[lvl];

            for (int i = 0; i < _levelInstance.Entities.Count(); i++)
            {
                if (lvl == TR2LevelNames.CHICKEN && 
                    _levelInstance.Entities[i].Room == 143 &&
                    _levelInstance.Entities[i].TypeID == (int)TR2Entities.BirdMonster)
                {
                    //#60 - Ice Palace - Room 143 chicken man must remain to avoid softlock.
                    continue;
                }
                else if (lvl == TR2LevelNames.HOME && _levelInstance.Entities[i].TypeID == (int)TR2Entities.ShotgunGoon)
                {
                    //#62 - Avoid randomizing shotgun goon in HSH
                    continue;
                }

                //#45 - Check to see if any items are at the same location as the enemy.
                //If there are we need to ensure that the new random enemy type is one that can drop items.
                List<TR2Entity> SharedItems = new List<TR2Entity>(Array.FindAll(_levelInstance.Entities, e => ((e.X == _levelInstance.Entities[i].X) 
                && (e.Y == _levelInstance.Entities[i].Y) && (e.Z == _levelInstance.Entities[i].Z))));

                List<TR2Entities> DroppableEnemies = TR2EntityUtilities.DroppableEnemyTypes()[lvl];

                //Is it an entity we are keen on replacing?
                if (EnemyTypes.Contains((TR2Entities)_levelInstance.Entities[i].TypeID))
                {
                    //Do multiple entities share one location?
                    if ((SharedItems.Count > 1) && (DroppableEnemies.Count != 0))
                    {
                        //Are any entities sharing a location a droppable pickup?
                        bool IsPickupItem = false;

                        foreach (TR2Entity ent in SharedItems)
                        {
                            TR2Entities EntType = (TR2Entities)ent.TypeID;

                            IsPickupItem = (TR2EntityUtilities.IsUtilityType(EntType)) ||
                                            (TR2EntityUtilities.IsGunType(EntType)) ||
                                            (TR2EntityUtilities.IsKeyItemType(EntType));

                            if (IsPickupItem)
                                break;
                        }

                        //Generate a location
                        _levelInstance.Entities[i].TypeID = (short)EnemyTypes[_generator.Next(0, EnemyTypes.Count)];

                        //Do we need to ensure the enemy can drop the item on the same tile?
                        if (!TR2EntityUtilities.CanDropPickups((TR2Entities)_levelInstance.Entities[i].TypeID, false, false) && IsPickupItem)
                        {
                            //Ensure the new random entity can drop pickups
                            _levelInstance.Entities[i].TypeID = (short)DroppableEnemies[_generator.Next(0, DroppableEnemies.Count)];
                        }
                    }
                    else
                    {
                        _levelInstance.Entities[i].TypeID = (short)EnemyTypes[_generator.Next(0, EnemyTypes.Count)];
                    }

                    short room = _levelInstance.Entities[i].Room;

                    if (!TR2EntityUtilities.IsWaterCreature((TR2Entities)_levelInstance.Entities[i].TypeID) 
                        && _levelInstance.Rooms[room].ContainsWater)
                    {
                        if (!PerformDraining(lvl, room))
                        {
                            //Draining cannot be performed so make the entity a water creature.
                            TR2Entities ent;

                            //Make sure water creature can appear on level
                            do
                            {
                                ent = TR2EntityUtilities.WaterCreatures()[_generator.Next(0, TR2EntityUtilities.WaterCreatures().Count)];
                            } while (!EnemyTypes.Contains(ent));

                            _levelInstance.Entities[i].TypeID = (short)ent;
                        }
                    }
                }
            }
        }

        private bool PerformDraining(string lvl, short room)
        {
            foreach (List<int> area in RoomWaterUtilities.RoomRemovalWaterMap[lvl])
            {
                if (area.Contains(room))
                {
                    foreach (int filledRoom in area)
                    {
                        _levelInstance.Rooms[filledRoom].Drain();
                    }

                    return true;
                }
            }

            return false;
        }
    }
}
