using Newtonsoft.Json;
using System.Diagnostics;
using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Utilities;
using TRRandomizerCore.Zones;

namespace TRRandomizerCore.Randomizers;

public class TR2SecretRandomizer : BaseTR2Randomizer, ISecretRandomizer
{
    private static readonly List<int> _devRooms = null;
    private static readonly int _levelSecretCount = 3;

    private readonly Dictionary<string, List<Location>> _locations;
    private SecretPicker _picker;

    public IMirrorControl Mirrorer { get; set; }
    public ItemFactory ItemFactory { get; set; }

    public TR2SecretRandomizer()
    {
        _locations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR2\Locations\locations.json"));
    }

    public IEnumerable<string> GetPacks()
    {
        return _locations.Values
            .SelectMany(v => v.Select(l => l.PackID))
            .Where(a => a != Location.DefaultPackID)
            .Distinct();
    }

    private void RandomizeSecrets(List<Location> LevelLocations)
    {
        if (LevelLocations.Count > 2)
        {
            if (Settings.DevelopmentMode)
            {
                PlaceAllSecrets(LevelLocations);
                return;
            }

            Location goldLocation = null;
            Location jadeLocation = null;
            Location stoneLocation = null;

            // Applied guaranteed logic first.
            Queue<Location> guaranteedLocations = _picker.GetGuaranteedLocations(LevelLocations, false, _levelSecretCount);

            //Apply zoning to the locations to ensure they are spread out.                
            List<Location> stoneZone, jadeZone, goldZone;
            bool secretPackMode = Settings.UseSecretPack && LevelLocations.Any(l => l.PackID != Location.DefaultPackID);
            if (secretPackMode)
            {
                stoneZone = jadeZone = goldZone = LevelLocations;
                guaranteedLocations = new(guaranteedLocations.Reverse());
            }
            else
            {
                ZonedLocationCollection zones = new();
                zones.PopulateZones(GetResourcePath(@"TR2\Zones\" + _levelInstance.Name + "-Zones.json"), LevelLocations, ZonePopulationMethod.SecretsOnly);
                stoneZone = zones.StoneZone;
                jadeZone = zones.JadeZone;
                goldZone = zones.GoldZone;
            }

            bool TestLocation(Location location, params Location[] setLocations)
            {
                if (secretPackMode)
                {
                    return true;
                }

                bool valid = true;
                foreach (Location setLocation in setLocations)
                {
                    valid &= setLocation == null || setLocation.Room != location.Room;
                }

                return valid;
            }

            while (guaranteedLocations.Count > 0)
            {
                Location location = guaranteedLocations.Dequeue();
                if (goldZone.Contains(location) && goldLocation == null)
                {
                    if (TestLocation(location, stoneLocation, jadeLocation))
                    {
                        goldLocation = location;
                    }
                }
                else if (jadeZone.Contains(location) && jadeLocation == null)
                {
                    if (TestLocation(location, stoneLocation, goldLocation))
                    {
                        jadeLocation = location;
                    }
                }
                else if (stoneLocation == null)
                {
                    if (TestLocation(location, jadeLocation, goldLocation))
                    {
                        stoneLocation = location;
                    }
                }
            }

            //Find suitable locations for those not allocated yet, ensuring they are zoned, do not share a room and difficulty.

            if (goldLocation == null)
            {
                do
                {
                    goldLocation = goldZone[_generator.Next(0, goldZone.Count)];
                } while ((goldLocation.Difficulty == Difficulty.Hard && Settings.HardSecrets == false) ||
                        (goldLocation.RequiresGlitch == true && Settings.GlitchedSecrets == false));
            }

            if (jadeLocation == null)
            {
                do
                {
                    jadeLocation = jadeZone[_generator.Next(0, jadeZone.Count)];
                } while ((jadeLocation.Room == goldLocation.Room) ||
                        (jadeLocation.Difficulty == Difficulty.Hard && Settings.HardSecrets == false) ||
                        (jadeLocation.RequiresGlitch == true && Settings.GlitchedSecrets == false));
            }

            if (stoneLocation == null)
            {
                do
                {
                    stoneLocation = stoneZone[_generator.Next(0, stoneZone.Count)];
                } while ((stoneLocation.Room == goldLocation.Room) ||
                        (stoneLocation.Room == jadeLocation.Room) ||
                        (stoneLocation.Difficulty == Difficulty.Hard && Settings.HardSecrets == false) ||
                        (stoneLocation.RequiresGlitch == true && Settings.GlitchedSecrets == false));
            }

            //Due to TRMod only accepting room space coords entities are actually stored in level space. So include some
            //calls to support a transformation of any locations that are specified in room space to maintain backwards compatbility
            //with older locations and support locations that are specified in both level or room space.
            goldLocation = SpatialConverters.TransformToLevelSpace(goldLocation, _levelInstance.Data.Rooms[goldLocation.Room].Info);
            jadeLocation = SpatialConverters.TransformToLevelSpace(jadeLocation, _levelInstance.Data.Rooms[jadeLocation.Room].Info);
            stoneLocation = SpatialConverters.TransformToLevelSpace(stoneLocation, _levelInstance.Data.Rooms[stoneLocation.Room].Info);

            Dictionary<TR2Type, Location> secretMap = new()
            {
                [TR2Type.StoneSecret_S_P] = stoneLocation,
                [TR2Type.JadeSecret_S_P] = jadeLocation,
                [TR2Type.GoldSecret_S_P] = goldLocation
            };

            List<TR2Entity> ents = _levelInstance.Data.Entities.ToList();
            foreach (TR2Type secretType in secretMap.Keys)
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

            _picker.FinaliseSecretPool(secretMap.Values, _levelInstance.Name);

#if DEBUG
            Debug.WriteLine(_levelInstance.Name + ": " + SecretPicker.DescribeLocations(secretMap.Values));
#endif
        }
    }

    private void PlaceAllSecrets(List<Location> LevelLocations)
    {
        ZonedLocationCollection ZonedLocations = new();

        ZonedLocations.PopulateZones(GetResourcePath(@"TR2\Zones\" + _levelInstance.Name + "-Zones.json"), LevelLocations, ZonePopulationMethod.SecretsOnly);

        List<TR2Entity> ents = _levelInstance.Data.Entities.ToList();

        // Store existing secret indices for re-use (avoids FD problems when the originals are removed)
        Queue<int> existingIndices = new();
        for (int i = 0; i < ents.Count; i++)
        {
            if (TR2TypeUtilities.IsSecretType((TR2Type)ents[i].TypeID))
            {
                existingIndices.Enqueue(i);
            }
        }

        //Add new entities
        Dictionary<TR2Type, List<Location>> secretMap = new()
        {
            [TR2Type.StoneSecret_S_P] = ZonedLocations.StoneZone,
            [TR2Type.JadeSecret_S_P] = ZonedLocations.JadeZone,
            [TR2Type.GoldSecret_S_P] = ZonedLocations.GoldZone
        };

        foreach (TR2Type secretType in secretMap.Keys)
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
            TRSpriteSequence stoneSequence = Array.Find(_levelInstance.Data.SpriteSequences, s => s.SpriteID == (int)TR2Type.StoneSecret_S_P);
            TRSpriteSequence jadeSequence = Array.Find(_levelInstance.Data.SpriteSequences, s => s.SpriteID == (int)TR2Type.JadeSecret_S_P);

            TRSpriteTexture[] textures = _levelInstance.Data.SpriteTextures;
            (textures[jadeSequence.Offset], textures[stoneSequence.Offset]) 
                = (textures[stoneSequence.Offset], textures[jadeSequence.Offset]);
        }
    }

    public override void Randomize(int seed)
    {
        _generator = new Random(seed);
        _picker = new SecretPicker
        {
            Settings = Settings,
            Generator = _generator,
            ItemFactory = ItemFactory,
            Mirrorer = Mirrorer,
        };

        foreach (TR2ScriptedLevel lvl in Levels)
        {
            //Read the level into a level object
            LoadLevelInstance(lvl);
            if (_locations.ContainsKey(_levelInstance.Name))
            {
                //Apply the modifications
                RandomizeSecrets(_locations[_levelInstance.Name]);

                //Write back the level file
                SaveLevelInstance();
            }

            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    private void CheckForSecretDamage(Dictionary<TR2Type, Location> secretMap)
    {
        uint easyDamageCount = 0;
        uint hardDamageCount = 0;

        foreach (TR2Type secretType in secretMap.Keys)
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

            _levelInstance.Script.AddStartInventoryItem(ItemUtilities.ConvertToScriptItem(TR2Type.LargeMed_S_P), hardDamageCount);
            _levelInstance.Script.AddStartInventoryItem(ItemUtilities.ConvertToScriptItem(TR2Type.SmallMed_S_P), easyDamageCount);
        }
    }
}
