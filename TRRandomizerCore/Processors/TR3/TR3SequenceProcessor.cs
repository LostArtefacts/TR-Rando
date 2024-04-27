using Newtonsoft.Json;
using TREnvironmentEditor;
using TREnvironmentEditor.Model;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRGE.Core;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRModelTransporter.Transport;
using TRRandomizerCore.Editors;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Textures;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Processors;

public class TR3SequenceProcessor : TR3LevelProcessor
{
    private static readonly Dictionary<TR3Type, TR3Type> _artefactAssignment = new()
    {
        [TR3Type.Infada_P] = TR3Type.Key1_P,
        [TR3Type.OraDagger_P] = TR3Type.Key2_P,
        [TR3Type.EyeOfIsis_P] = TR3Type.Key3_P,
        [TR3Type.Element115_P] = TR3Type.Key4_P,
        [TR3Type.Infada_M_H] = TR3Type.Key1_M_H,
        [TR3Type.OraDagger_M_H] = TR3Type.Key2_M_H,
        [TR3Type.EyeOfIsis_M_H] = TR3Type.Key3_M_H,
        [TR3Type.Element115_M_H] = TR3Type.Key4_M_H,
    };

    private static readonly Dictionary<TR3Adventure, int> _adventureStringSequences = new()
    {
        [TR3Adventure.SouthPacific] = 87,
        [TR3Adventure.London] = 85,
        [TR3Adventure.Nevada] = 86,
        [TR3Adventure.Antarctica] = 88
    };

    private static readonly uint _defaultColdMediCount = 3;

    private Dictionary<string, List<Location>> _upvLocations;

    private Dictionary<TR3Adventure, string> _adventureNames;
    private List<string> _gameStrings;

    public RandomizerSettings Settings { get; set; }
    public TR3TextureMonitorBroker TextureMonitor { get; set; }
    public ItemFactory<TR3Entity> ItemFactory { get; set; }

    public void Run()
    {
        _upvLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR3\Locations\upv_locations.json"));

        _gameStrings = new List<string>(ScriptEditor.Script.GameStrings1);
        _adventureNames = new Dictionary<TR3Adventure, string>
        {
            [TR3Adventure.India] = "India" // Not stored in script
        };

        foreach (TR3Adventure sequence in _adventureStringSequences.Keys)
        {
            _adventureNames[sequence] = ScriptEditor.Script.GameStrings1[_adventureStringSequences[sequence]];
        }

        foreach (TR3ScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);

            AdjustLevel(_levelInstance);

            SaveLevelInstance();

            if (!TriggerProgress())
            {
                break;
            }
        }

        ScriptEditor.Script.GameStrings1 = _gameStrings.ToArray();
        SaveScript();
    }

    private void AdjustLevel(TR3CombinedLevel level)
    {
        if (level.HasExposureMeter)
        {
            if (!level.Is(TR3LevelNames.ANTARC) && !level.Is(TR3LevelNames.RXTECH))
            {
                // The UPV keeps Lara warm underwater so make this available for
                // those levels that have long underwater sections.
                ImportUPV(level);
                AddColdLevelMedis(level);
            }
        }
        else if (level.Is(TR3LevelNames.ANTARC))
        {
            // Add a KillLara FD entry to room 185, where she would normally freeze
            // to death anyway.
            AmendAntarctica(level);
        }

        if (level.Is(TR3LevelNames.WILLIE))
        {
            if (!level.IsWillardSequence)
            {
                // Picking-up the artefacts will end the level for all sequences != 19 so
                // re-assign the items as keys, which Cavern doesn't use.
                AmendWillardBoss(level);
            }
        }
        else if (level.IsWillardSequence || (level.Is(TR3LevelNames.CITY) && !level.IsSophiaSequence))
        {
            // Because the stones don't end the level on sequence 19, make any required mods
            // to make end level triggers.
            // #231 the electric fields in City can't be triggered when it's off-sequence, so
            // the mods are applied in this instance too.
            AmendBossFight(level);
        }
        else if ((level.Is(TR3LevelNames.COASTAL) || level.Is(TR3LevelNames.MADUBU)) && !level.IsCoastalSequence && !level.IsMadubuSequence)
        {
            // Spikes are only triggered in Coastal Village and Madubu, so we have to raise them
            // out of the ground when their code bits are set, so effectively initialising them
            // as the game does. The only ones lost are in room 128 in Coastal - these remain
            // underground as there is no way to pass otherwise.
            AmendSouthPacificSpikes(level);
        }
        else if (level.Is(TR3LevelNames.CRASH) && !level.IsCrashSequence)
        {
            // Piranhas don't attack the raptor when it falls into the pool, so Lara can't (easily) pull the lever.
            AmendCrashSitePiranhas(level);
        }

        if (Settings.RandomizeSequencing)
        {
            // #277 Make sure levels have artefact menu models because these vary based on original sequencing.
            ImportArtefactMenuModels(level);

            // If this level is the first in an adventure, update the globe string to match
            if (_adventureStringSequences.ContainsKey((TR3Adventure)level.Sequence))
            {
                switch (Settings.GlobeDisplay)
                {
                    case GlobeDisplayOption.Area:
                        _gameStrings[_adventureStringSequences[(TR3Adventure)level.Sequence]] = _adventureNames[level.Adventure];
                        break;
                    case GlobeDisplayOption.Level:
                        _gameStrings[_adventureStringSequences[(TR3Adventure)level.Sequence]] = level.Script.Name;
                        break;
                }
            }
        }
    }

    private void ImportUPV(TR3CombinedLevel level)
    {
        if (!_upvLocations.ContainsKey(level.Name) || _upvLocations[level.Name].Count == 0)
        {
            return;
        }

        List<TR3Type> upvImport = new() { TR3Type.UPV };
        TR3ModelImporter importer = new()
        {
            Level = level.Data,
            LevelName = level.Name,
            EntitiesToImport = upvImport,
            DataFolder = GetResourcePath(@"TR3\Models"),
            TexturePositionMonitor = TextureMonitor.CreateMonitor(level.Name, upvImport)
        };

        importer.Import();

        foreach (Location location in _upvLocations[level.Name])
        {
            TR3Entity entity = ItemFactory.CreateItem(level.Name, level.Data.Entities, location);
            if (entity == null)
            {
                break;
            }

            entity.TypeID = TR3Type.UPV;
        }

        // We can only have one vehicle type per level because LaraVehicleAnimation_H is tied to
        // each, so for the likes of Nevada, replace the quad with another UPV to fly into HSC.
        level.Data.Entities
            .FindAll(e => e.TypeID == TR3Type.Quad)
            .ForEach(e => e.TypeID = TR3Type.UPV);

        // If we're not randomizing enemies, we have to perform the monkey/tiger/vehicle crash
        // test here for the likes of Jungle.
        if (!Settings.RandomizeEnemies
            && level.Data.Entities.Any(e => e.TypeID == TR3Type.Monkey)
            && level.Data.Models.Any(m => (TR3Type)m.ID == TR3Type.Tiger))
        {
            level.RemoveModel(TR3Type.Tiger);
            level.Data.Entities.Where(e => e.TypeID == TR3Type.Tiger)
                .ToList()
                .ForEach(e => e.TypeID = TR3Type.Monkey);
        }
    }

    private static void AddColdLevelMedis(TR3CombinedLevel level)
    {
        if (!level.Data.Entities.Any(e => e.TypeID == TR3Type.UPV))
        {
            return;
        }
        
        uint largeMediCount = _defaultColdMediCount;
        uint smallMediCount = (uint)Math.Ceiling(level.Data.Entities.Where(e => e.TypeID == TR3Type.UnderwaterSwitch).Count() / 2d);
        if (smallMediCount > 0)
        {
            largeMediCount++;
        }
        if (level.Script.RemovesAmmo)
        {
            largeMediCount *= 2;
            smallMediCount *= 2;
        }

        level.Script.AddStartInventoryItem(ItemUtilities.ConvertToScriptItem(TR3Type.LargeMed_P), largeMediCount);
        level.Script.AddStartInventoryItem(ItemUtilities.ConvertToScriptItem(TR3Type.SmallMed_P), smallMediCount);
    }

    private static void AmendAntarctica(TR3CombinedLevel level)
    {
        FDControl floorData = new();
        floorData.ParseFromLevel(level.Data);
        TRRoomSector sector = FDUtilities.GetRoomSector(53760, -3328, 28160, 185, level.Data, floorData);
        if (sector.FDIndex == 0)
        {
            floorData.CreateFloorData(sector);
        }

        floorData.Entries[sector.FDIndex].Add(new FDKillLaraEntry
        {
            Setup = new FDSetup(FDFunctions.KillLara)
        });

        floorData.WriteToLevel(level.Data);
    }

    private void ImportArtefactMenuModels(TR3CombinedLevel level)
    {
        List<TR3Type> imports = new();
        foreach (TR3Type artefactMenuModel in TR3TypeUtilities.GetArtefactMenuModels())
        {
            if (level.Data.Models.Find(m => m.ID == (uint)artefactMenuModel) == null)
            {
                imports.Add(artefactMenuModel);
            }
        }

        if (imports.Count > 0)
        {
            TR3ModelImporter importer = new()
            {
                Level = level.Data,
                LevelName = level.Name,
                EntitiesToImport = imports,
                DataFolder = GetResourcePath(@"TR3\Models")
            };

            importer.Import();
        }
    }

    private void AmendWillardBoss(TR3CombinedLevel level)
    {
        // Add new duplicate models for keys, so secret rando doesn't replace the originals.
        foreach (TR3Type artefact in _artefactAssignment.Keys)
        {
            TR3Type replacement = _artefactAssignment[artefact];
            TRModel artefactModel = level.Data.Models.Find(m => m.ID == (uint)artefact);
            TRModel replacementModel = artefactModel.Clone();
            replacementModel.ID = (uint)replacement;
            level.Data.Models.Add(replacementModel);

            level.Data.Entities
                .FindAll(e => e.TypeID == artefact)
                .ForEach(e => e.TypeID = replacement);
        }

        // Copy the artefact names into the keys
        for (int i = 0; i < 4; i++)
        {
            level.Script.Keys[i] = ScriptEditor.Script.GameStrings1[80 + i];
        }

        // Apply any changes needed for the boss fight
        AmendBossFight(level);

        // Hide the old Willie AI pathing
        for (int i = 0; i < level.Data.Entities.Count; i++)
        {
            TR3Entity entity = level.Data.Entities[i];
            TR3Type type = entity.TypeID;
            if (type == TR3Type.AIPath_N || type == TR3Type.AICheck_N)
            {
                entity.TypeID = TR3Type.PistolAmmo_M_H;
                entity.X = 66048;
                entity.Y = 768;
                entity.Z = 67072;
                entity.Room = 5;
                ItemUtilities.HideEntity(entity);
                ItemFactory.FreeItem(level.Name, i);
            }
        }
    }

    private void AmendBossFight(TR3CombinedLevel level)
    {
        string mappingPath = @"TR3\BossMapping\" + level.Name + "-BossMapping.json";
        if (ResourceExists(mappingPath))
        {
            EMEditorSet mods = JsonConvert.DeserializeObject<EMEditorSet>(ReadResource(mappingPath), EMEditorMapping.Converter);
            mods.ApplyToLevel(level.Data);
        }
    }

    private static void AmendSouthPacificSpikes(TR3CombinedLevel level)
    {
        List<TR3Entity> entities = level.Data.Entities.FindAll(e => e.TypeID == TR3Type.TeethSpikesOrBarbedWire);
        foreach (TR3Entity entity in entities)
        {
            if (level.Is(TR3LevelNames.MADUBU) || entity.CodeBits == 31)
            {
                entity.Y -= TRConsts.Step3;
            }
            if (level.Is(TR3LevelNames.MADUBU))
            {
                entity.Invisible = false;
            }
        }
    }

    private static void AmendCrashSitePiranhas(TR3CombinedLevel level)
    {
        TR3Entity piranhas = level.Data.Entities.Find(e => e.TypeID == TR3Type.Piranhas_N && e.Room == 61);
        if (piranhas != null)
        {
            // Move them behind the gate, which is an unreachable room
            piranhas.X = 59904;
            piranhas.Room = 65;
        }
    }
}
