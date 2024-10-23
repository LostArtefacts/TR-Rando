using TRDataControl;
using TRGE.Core;
using TRImageControl.Packing;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR3ItemRandomizer : BaseTR3Randomizer
{
    private static readonly List<TR3Type> _assaultPickupModels = new()
    {
        TR3Type.PistolAmmo_P,
        TR3Type.Shotgun_P,
        TR3Type.Deagle_P,
        TR3Type.Uzis_P,
        TR3Type.MP5_P,
        TR3Type.RocketLauncher_P,
        TR3Type.GrenadeLauncher_P,
        TR3Type.Harpoon_P,
        TR3Type.SmallMed_P,
        TR3Type.LargeMed_P,
        TR3Type.LaraDeagleAnimation_H_Nevada,
        TR3Type.LaraUziAnimation_H_Nevada,
    };

    private TR3ItemAllocator _allocator;

    public ItemFactory<TR3Entity> ItemFactory { get; set; }

    public override void Randomize(int seed)
    {
        _generator = new(seed);
        _allocator = new()
        {
            Generator = _generator,
            Settings = Settings,
            ItemFactory = ItemFactory,
        };

        _allocator.AllocateWeapons(Levels.Where(l => !l.Is(TR3LevelNames.ASSAULT)));

        foreach (TR3ScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);

            if (_levelInstance.IsAssault && !ImportAssaultModels(_levelInstance))
            {
                TriggerProgress();
                continue;
            }

            _allocator.RandomizeItems(_levelInstance.Name, _levelInstance.Data, 
                _levelInstance.Script.RemovesWeapons || _levelInstance.IsAssault, _levelInstance.Script.OriginalSequence, _levelInstance.HasExposureMeter);

            if (_levelInstance.IsAssault)
            {
                AddAssaultCourseAmmo(_levelInstance);
            }

            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    public void FinalizeRandomization()
    {
        foreach (TR3ScriptedLevel lvl in Levels)
        {
            if (Settings.ItemMode == ItemMode.Shuffled || Settings.IncludeKeyItems)
            {
                LoadLevelInstance(lvl);

                if (Settings.ItemMode == ItemMode.Shuffled)
                {
                    _allocator.ApplyItemSwaps(_levelInstance.Name, _levelInstance.Data.Entities);
                }
                else
                {
                    _allocator.RandomizeKeyItems(_levelInstance.Name, _levelInstance.Data,
                        _levelInstance.Script.OriginalSequence, _levelInstance.HasExposureMeter);
                }

                SaveLevelInstance();
            }

            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    private bool ImportAssaultModels(TR3CombinedLevel level)
    {
        if (Settings.ItemMode == ItemMode.Shuffled)
        {
            return true;
        }

        TR3DataImporter importer = new()
        {
            Level = level.Data,
            LevelName = level.Name,
            TypesToImport = _assaultPickupModels,
            DataFolder = GetResourcePath("TR3/Objects")
        };

        string remapPath = $"TR3/Textures/Deduplication/{level.Name}-TextureRemap.json";
        if (ResourceExists(remapPath))
        {
            importer.TextureRemapPath = GetResourcePath(remapPath);
        }

        try
        {
            importer.Import();

            // Manipulate the Nevada leg meshes to match the home outfit.
            // Anything using vertex 26 and above is a holstered gun face.
            TRModel pistolModel = level.Data.Models[TR3Type.LaraPistolAnimation_H];
            TRModel deagleModel = level.Data.Models[TR3Type.LaraDeagleAnimation_H];
            TRModel uzisModel = level.Data.Models[TR3Type.LaraUziAnimation_H];

            static void CopyFaces(TRMesh baseMesh, TRMesh targetMesh)
            {
                static bool regularFace(TRMeshFace f) => f.Vertices.All(v => v < 26);
                targetMesh.TexturedRectangles.RemoveAll(regularFace);
                targetMesh.TexturedTriangles.RemoveAll(regularFace);
                targetMesh.TexturedRectangles.AddRange(baseMesh.TexturedRectangles.FindAll(regularFace));
                targetMesh.TexturedTriangles.AddRange(baseMesh.TexturedTriangles.FindAll(regularFace));
            }

            deagleModel.Meshes[1] = pistolModel.Meshes[1];
            CopyFaces(pistolModel.Meshes[4], deagleModel.Meshes[4]);
            CopyFaces(pistolModel.Meshes[1], uzisModel.Meshes[1]);
            CopyFaces(pistolModel.Meshes[4], uzisModel.Meshes[4]);

            return true;
        }
        catch (PackingException)
        {
            return false;
        }
    }

    private void AddAssaultCourseAmmo(TR3CombinedLevel level)
    {
        TR3Entity weapon = _allocator.GetUnarmedLevelPistols(_levelInstance.Name, _levelInstance.Data.Entities);
        if (weapon == null || Settings.ItemMode == ItemMode.Shuffled)
        {
            return;
        }

        level.Script.AddStartInventoryItem(ItemUtilities.ConvertToScriptItem(TR3TypeUtilities.GetWeaponAmmo(weapon.TypeID)), 20);
    }
}
