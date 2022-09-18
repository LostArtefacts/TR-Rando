using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Utilities;
using TRRandomizerCore.Zones;
using TRGE.Core;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using System.IO;

namespace TRRandomizerCore.Randomizers
{
    public class TR2SecretRandomizer : BaseTR2Randomizer
    {
        private static readonly List<int> _devRooms = null;

        private void RandomizeSecrets(List<Location> LevelLocations)
        {
            if (LevelLocations.Count > 2)
            {
                if (Settings.DevelopmentMode)
                {
                    PlaceAllSecrets(LevelLocations);
                    return;
                }

                //Apply zoning to the locations to ensure they are spread out.
                ZonedLocationCollection ZonedLocations = new ZonedLocationCollection();

                ZonedLocations.PopulateZones(GetResourcePath(@"TR2\Zones\" + _levelInstance.Name + "-Zones.json"), LevelLocations, ZonePopulationMethod.SecretsOnly);

                Location goldLocation;
                Location jadeLocation;
                Location stoneLocation;

                //Find suitable locations, ensuring they are zoned, do not share a room and difficulty.
                //Location = ZoneLocations[ZoneGroup][LocationInZoneGroup]
                do
                {
                    goldLocation = ZonedLocations.GoldZone[_generator.Next(0, ZonedLocations.GoldZone.Count)];
                } while ((goldLocation.Difficulty == Difficulty.Hard && Settings.HardSecrets == false) ||
                        (goldLocation.RequiresGlitch == true && Settings.GlitchedSecrets == false));


                do
                {
                    jadeLocation = ZonedLocations.JadeZone[_generator.Next(0, ZonedLocations.JadeZone.Count)];
                } while ((jadeLocation.Room == goldLocation.Room) ||
                        (jadeLocation.Difficulty == Difficulty.Hard && Settings.HardSecrets == false) ||
                        (jadeLocation.RequiresGlitch == true && Settings.GlitchedSecrets == false));

                do
                {
                    stoneLocation = ZonedLocations.StoneZone[_generator.Next(0, ZonedLocations.StoneZone.Count)];
                } while ((stoneLocation.Room == goldLocation.Room) ||
                        (stoneLocation.Room == jadeLocation.Room) ||
                        (stoneLocation.Difficulty == Difficulty.Hard && Settings.HardSecrets == false) ||
                        (stoneLocation.RequiresGlitch == true && Settings.GlitchedSecrets == false));

                //Due to TRMod only accepting room space coords entities are actually stored in level space. So include some
                //calls to support a transformation of any locations that are specified in room space to maintain backwards compatbility
                //with older locations and support locations that are specified in both level or room space.
                goldLocation = SpatialConverters.TransformToLevelSpace(goldLocation, _levelInstance.Data.Rooms[goldLocation.Room].Info);
                jadeLocation = SpatialConverters.TransformToLevelSpace(jadeLocation, _levelInstance.Data.Rooms[jadeLocation.Room].Info);
                stoneLocation = SpatialConverters.TransformToLevelSpace(stoneLocation, _levelInstance.Data.Rooms[stoneLocation.Room].Info);

                Dictionary<TR2Entities, Location> secretMap = new Dictionary<TR2Entities, Location>
                {
                    [TR2Entities.StoneSecret_S_P] = stoneLocation,
                    [TR2Entities.JadeSecret_S_P] = jadeLocation,
                    [TR2Entities.GoldSecret_S_P] = goldLocation
                };

                List<TR2Entity> ents = _levelInstance.Data.Entities.ToList();
                foreach (TR2Entities secretType in secretMap.Keys)
                {
                    //Does the level contain an entity for this type?
                    TR2Entity secretEntity = Array.Find(_levelInstance.Data.Entities, ent => ent.TypeID == (short)secretType);

                    //If not, create a placeholder entity for now
                    if (secretEntity == null)
                    {
                        ents.Add(secretEntity = new TR2Entity());
                    }

                    // Move it to the new location and ensure it has the correct type set
                    Location location = secretMap[secretType];
                    secretEntity.TypeID = (short)secretType;
                    secretEntity.Room = (short)location.Room;
                    secretEntity.X = location.X;
                    secretEntity.Y = location.Y;
                    secretEntity.Z = location.Z;
                    secretEntity.Intensity1 = -1;
                    secretEntity.Intensity2 = -1;
                    secretEntity.Angle = 0;
                    secretEntity.Flags = 0;
                }

                _levelInstance.Data.Entities = ents.ToArray();
                _levelInstance.Data.NumEntities = (uint)ents.Count;

                FixSecretTextures();
                CheckForSecretDamage(secretMap);
            }
        }

        private void PlaceAllSecrets(List<Location> LevelLocations)
        {
            ZonedLocationCollection ZonedLocations = new ZonedLocationCollection();

            ZonedLocations.PopulateZones(GetResourcePath(@"TR2\Zones\" + _levelInstance.Name + "-Zones.json"), LevelLocations, ZonePopulationMethod.SecretsOnly);

            List<TR2Entity> ents = _levelInstance.Data.Entities.ToList();

            // Store existing secret indices for re-use (avoids FD problems when the originals are removed)
            Queue<int> existingIndices = new Queue<int>();
            for (int i = 0; i < ents.Count; i++)
            {
                if (TR2EntityUtilities.IsSecretType((TR2Entities)ents[i].TypeID))
                {
                    existingIndices.Enqueue(i);
                }
            }

            //Add new entities
            Dictionary<TR2Entities, List<Location>> secretMap = new Dictionary<TR2Entities, List<Location>>
            {
                [TR2Entities.StoneSecret_S_P] = ZonedLocations.StoneZone,
                [TR2Entities.JadeSecret_S_P] = ZonedLocations.JadeZone,
                [TR2Entities.GoldSecret_S_P] = ZonedLocations.GoldZone
            };

            foreach (TR2Entities secretType in secretMap.Keys)
            {
                foreach (Location loc in secretMap[secretType])
                {
                    Location copy = SpatialConverters.TransformToLevelSpace(loc, _levelInstance.Data.Rooms[loc.Room].Info);

                    if (_devRooms == null || _devRooms.Contains(copy.Room))
                    {
                        TR2Entity entity;
                        if (existingIndices.Count > 0)
                        {
                            entity = ents[existingIndices.Dequeue()];
                        }
                        else
                        {
                            ents.Add(entity = new TR2Entity());
                        }

                        entity.TypeID = (short)secretType;
                        entity.Room = (short)copy.Room;
                        entity.X = copy.X;
                        entity.Y = copy.Y;
                        entity.Z = copy.Z;
                        entity.Angle = 0;
                        entity.Intensity1 = -1;
                        entity.Intensity2 = -1;
                        entity.Flags = 0;
                    }
                }
            }

            _levelInstance.Data.NumEntities = (uint)ents.Count;
            _levelInstance.Data.Entities = ents.ToArray();

            FixSecretTextures();
        }

        private void FixSecretTextures()
        {
            if (_levelInstance.Is(TR2LevelNames.FLOATER) || _levelInstance.Is(TR2LevelNames.LAIR) || _levelInstance.Is(TR2LevelNames.HOME))
            {
                // Swap Stone and Jade textures - OG has them the wrong way around.
                // SpriteSequence offsets have to remain in order, so swap the texture targets instead.
                TRSpriteSequence stoneSequence = Array.Find(_levelInstance.Data.SpriteSequences, s => s.SpriteID == (int)TR2Entities.StoneSecret_S_P);
                TRSpriteSequence jadeSequence = Array.Find(_levelInstance.Data.SpriteSequences, s => s.SpriteID == (int)TR2Entities.JadeSecret_S_P);

                TRSpriteTexture stoneTexture = _levelInstance.Data.SpriteTextures[stoneSequence.Offset];
                _levelInstance.Data.SpriteTextures[stoneSequence.Offset] = _levelInstance.Data.SpriteTextures[jadeSequence.Offset];
                _levelInstance.Data.SpriteTextures[jadeSequence.Offset] = stoneTexture;
            }
        }

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            Dictionary<string, List<Location>> Locations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR2\Locations\locations.json"));

            foreach (TR2ScriptedLevel lvl in Levels)
            {
                //Read the level into a level object
                LoadLevelInstance(lvl);

                //Apply the modifications
                RandomizeSecrets(Locations[_levelInstance.Name]);

                //Write back the level file
                SaveLevelInstance();

                if (!TriggerProgress())
                {
                    break;
                }
            }
        }

        private void CheckForSecretDamage(Dictionary<TR2Entities, Location> secretMap)
        {
            uint easyDamageCount = 0;
            uint hardDamageCount = 0;

            foreach (TR2Entities secretType in secretMap.Keys)
            {
                Location location = secretMap[secretType];
                
                if (location.RequiresDamage)
                {
                    if (location.Difficulty == Difficulty.Hard)
                        hardDamageCount++;
                    else
                        easyDamageCount++;
                }
            }

            //  If we found some secrets needing damage
            if (hardDamageCount > 0 || easyDamageCount > 0)
            {
                // If this an unarmed level I add one hard damage
                if (_levelInstance.Script.RemovesWeapons) hardDamageCount++;
                // If its one of the firsts level I add one easy damage
                if (_levelInstance.Sequence < 2) easyDamageCount++;

                _levelInstance.Script.AddStartInventoryItem(ItemUtilities.ConvertToScriptItem(TR2Entities.LargeMed_S_P), hardDamageCount);
                _levelInstance.Script.AddStartInventoryItem(ItemUtilities.ConvertToScriptItem(TR2Entities.SmallMed_S_P), easyDamageCount);
            }
        }
    }
}
