using System.Collections.Generic;
using System.Linq;
using TRRandomizerCore.Utilities;
using TRGE.Core;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TRRandomizerCore.Levels
{
    public class TR2CombinedLevel
    {
        /// <summary>
        /// The main level data stored in the corresponding .TR2 file.
        /// </summary>
        public TR2Level Data { get; set; }

        /// <summary>
        /// The scripting information for the level stored in TOMBPC.dat.
        /// </summary>
        public TR2ScriptedLevel Script { get; set; }

        /// <summary>
        /// The uppercase base file name of the level e.g. KEEL.TR2
        /// </summary>
        public string Name => Script.LevelFileBaseName.ToUpper();

        /// <summary>
        /// The level data for the cutscene at the end of this level, if any.
        /// </summary>
        public TR2CombinedLevel CutSceneLevel { get; set; }

        /// <summary>
        /// A reference to the main level if this is a CutScene level.
        /// </summary>
        public TR2CombinedLevel ParentLevel { get; set; }

        /// <summary>
        /// True if this is a CutScene level, and so has a parent level.
        /// </summary>
        public bool IsCutScene => ParentLevel != null;

        /// <summary>
        /// Whether or not this level has a cutscene at the end.
        /// </summary>
        public bool HasCutScene => Script.HasCutScene;

        /// <summary>
        /// Gets the level's sequence in the game. If this is a CutScene level, this returns the parent sequence.
        /// </summary>
        public int Sequence => IsCutScene ? ParentLevel.Sequence : Script.Sequence;

        /// <summary>
        /// Compares the given file name or path against the base file name of the level (case-insensitive).
        /// </summary>
        public bool Is(string levelFileName) => Script.Is(levelFileName);

        /// <summary>
        /// Determines if the given level is specific to UKBox. This currently applies only to Floating Islands which differs between UKBox and EPC/Multipatch.
        /// </summary>
        // We previously checked for NumBoxes but with environment rando, this can now change. We instead look for the first
        // animated texture index (the lava) as this is 1702 in UKBox and 1686 in EPC/Multipatch. This remains consistent
        // regardless of texture deduplication.
        public bool IsUKBox => Is(TR2LevelNames.FLOATER) && Data.AnimatedTextures[0].Textures[0] == 1702;

        /// <summary>
        /// Returns {Name}-UKBox if this level is for UKBox, otherwise just {Name}.
        /// </summary>
        public string JsonID => IsUKBox ? Name + "-UKBox" : Name;

        /// <summary>
        /// Checks if the current level is the assault course.
        /// </summary>
        public bool IsAssault => Is(TR2LevelNames.ASSAULT);

        public bool CanPerformDraining(short room)
        {
            foreach (List<int> area in RoomWaterUtilities.RoomRemovalWaterMap[Name])
            {
                if (area.Contains(room))
                {
                    return true;
                }
            }

            return false;
        }

        public bool PerformDraining(short room)
        {
            foreach (List<int> area in RoomWaterUtilities.RoomRemovalWaterMap[Name])
            {
                if (area.Contains(room))
                {
                    foreach (int filledRoom in area)
                    {
                        Data.Rooms[filledRoom].Drain();
                    }

                    return true;
                }
            }

            return false;
        }

        public List<TR2Entity> GetEnemyEntities()
        {
            List<TR2Entities> allEnemies = TR2EntityUtilities.GetFullListOfEnemies();
            List<TR2Entity> levelEntities = new List<TR2Entity>();
            for (int i = 0; i < Data.NumEntities; i++)
            {
                TR2Entity entity = Data.Entities[i];
                if (allEnemies.Contains((TR2Entities)entity.TypeID))
                {
                    levelEntities.Add(entity);
                }
            }
            return levelEntities;
        }

        public int GetMaximumEntityLimit()
        {
            int limit = 256;

            // #153 The game creates a black skidoo for each skidoo driver when the level
            // is loaded, so there needs to be space in the entity array for these.
            List<TR2Entity> entities = Data.Entities.ToList();
            limit -= entities.FindAll(e => e.TypeID == (short)TR2Entities.MercSnowmobDriver).Count;

            // If there is a dragon, we need an extra 7 slots for the front bones, 
            // back bones etc. This is going by what's seen in Dragon.c
            if (entities.FindIndex(e => e.TypeID == (short)TR2Entities.MarcoBartoli) != -1)
            {
                limit -= 7;
            }

            return limit;
        }

        public int GetActualEntityCount()
        {
            int count = 0;
            foreach (TR2Entity entity in Data.Entities)
            {
                switch ((TR2Entities)entity.TypeID)
                {
                    case TR2Entities.MercSnowmobDriver:
                        count += 2;
                        break;
                    case TR2Entities.MarcoBartoli:
                        count += 7;
                        break;
                    default:
                        count++;
                        break;
                }
            }
            return count;
        }
    }
}