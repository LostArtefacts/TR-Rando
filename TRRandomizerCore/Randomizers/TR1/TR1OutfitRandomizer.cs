using System.Drawing;
using TREnvironmentEditor.Helpers;
using TRGE.Core;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRModelTransporter.Helpers;
using TRModelTransporter.Packing;
using TRModelTransporter.Transport;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Meshes;
using TRRandomizerCore.Processors;
using TRRandomizerCore.Textures;
using TRTexture16Importer.Helpers;
using TRTexture16Importer.Textures;

namespace TRRandomizerCore.Randomizers;

public class TR1OutfitRandomizer : BaseTR1Randomizer
{
    private static readonly string _gymOutfitHash = "7866a39d5dd37c89a4f20df3b074788e";
    private static readonly Version _minBraidCutsceneVersion = new(2, 13, 0);
    private static readonly TR1SFX[] _barefootSfxIDs = new TR1SFX[] { TR1SFX.LaraFeet, TR1SFX.LaraLand };
    private static readonly double _mauledLaraChance = (double)1 / 3;
    private static readonly double _partialGymChance = (double)1 / 3;
    private static readonly Color _midasGoldColour = Color.FromArgb(252, 236, 136);

    internal TR1TextureMonitorBroker TextureMonitor { get; set; }

    private List<TR1ScriptedLevel> _braidLevels;
    private List<TR1ScriptedLevel> _invisibleLevels;
    private List<TR1ScriptedLevel> _gymLevels, _partialGymLevels;
    private List<TR1ScriptedLevel> _mauledLevels;

    private Dictionary<TR1SFX, TR1SoundEffect> _barefootSfx;

    public override void Randomize(int seed)
    {
        _generator = new Random(seed);

        ChooseFilteredLevels();

        List<OutfitProcessor> processors = new();
        for (int i = 0; i < _maxThreads; i++)
        {
            processors.Add(new OutfitProcessor(this));
        }

        List<TR1CombinedLevel> levels = new(Levels.Count);
        foreach (TR1ScriptedLevel lvl in Levels)
        {
            levels.Add(LoadCombinedLevel(lvl));
            if (!TriggerProgress())
            {
                return;
            }
        }

        int processorIndex = 0;
        foreach (TR1CombinedLevel level in levels)
        {
            processors[processorIndex].AddLevel(level);
            processorIndex = processorIndex == _maxThreads - 1 ? 0 : processorIndex + 1;
        }

        foreach (OutfitProcessor processor in processors)
        {
            processor.Start();
        }

        foreach (OutfitProcessor processor in processors)
        {
            processor.Join();
        }

        _processingException?.Throw();
    }

    private void ChooseFilteredLevels()
    {
        TR1ScriptedLevel assaultCourse = Levels.Find(l => l.Is(TR1LevelNames.ASSAULT));
        ISet<TR1ScriptedLevel> exlusions = new HashSet<TR1ScriptedLevel> { assaultCourse };

        // "Haircut" settings = hair extensions in TR1
        _braidLevels = Levels.RandomSelection(_generator, (int)Settings.HaircutLevelCount, exclusions: exlusions);
        if (Settings.AssaultCourseHaircut)
        {
            _braidLevels.Add(assaultCourse);
        }

        _invisibleLevels = Levels.RandomSelection(_generator, (int)Settings.InvisibleLevelCount, exclusions: exlusions);
        if (Settings.AssaultCourseInvisible)
        {
            _invisibleLevels.Add(assaultCourse);
        }

        if (Settings.AllowGymOutfit)
        {
            // Gym outfits are only available if the T-Rex isn't present because that overwrites the MiscAnim's textures.
            _gymLevels = Levels.FindAll(l => LevelSupportsGymOutfit(l) && !l.Is(TR1LevelNames.ASSAULT));
            if (_gymLevels.Count > 0)
            {
                _gymLevels = _gymLevels.RandomSelection(_generator, _generator.Next(1, _gymLevels.Count + 1));
                _partialGymLevels = new();
                for (int i = _gymLevels.Count - 1; i >= 0; i--)
                {
                    if (_generator.NextDouble() < _partialGymChance)
                    {
                        _partialGymLevels.Add(_gymLevels[i]);
                        _gymLevels.RemoveAt(i);
                    }
                }
            }

            // Cache Lara's barefoot SFX from the original Gym.
            TR1Level gym = new TR1LevelControl().Read(Path.Combine(BackupPath, TR1LevelNames.ASSAULT));
            _barefootSfx = new();
            foreach (TR1SFX sfxID in _barefootSfxIDs)
            {
                _barefootSfx[sfxID] = gym.SoundEffects[sfxID];
                _barefootSfx[sfxID].Volume /= 2;
            }
        }

        // Add a chance of Lara's mauled outfit being used.
        _mauledLevels = new();
        foreach (TR1ScriptedLevel level in Levels)
        {
            if (IsInvisibleLevel(level) || IsGymLevel(level) || level.Is(TR1LevelNames.MIDAS))
            {
                continue;
            }
            
            TextureMonitor<TR1Type> monitor = TextureMonitor.GetMonitor(level.LevelFileBaseName.ToUpper());
            if (monitor != null && monitor.PreparedLevelMapping != null)
            {
                foreach (StaticTextureSource<TR1Type> source in monitor.PreparedLevelMapping.Keys)
                {
                    if (source.TextureEntities.Contains(TR1Type.LaraMiscAnim_H_Valley) && _generator.NextDouble() < _mauledLaraChance)
                    {
                        _mauledLevels.Add(level);
                    }
                }
            }

            // Extra check for Valley and ToQ as they have this anim by default.
            if ((level.Is(TR1LevelNames.VALLEY) || level.Is(TR1LevelNames.QUALOPEC)) && !_mauledLevels.Contains(level) && _generator.NextDouble() < _mauledLaraChance)
            {
                _mauledLevels.Add(level);
            }
        }

        if (_braidLevels.Count > 0)
        {
            (ScriptEditor.Script as TR1Script).EnableBraid = true;
        }
    }

    private bool LevelSupportsGymOutfit(TR1ScriptedLevel scriptedLevel)
    {
        if (IsInvisibleLevel(scriptedLevel))
        {
            return false;
        }

        TR1CombinedLevel level = LoadCombinedLevel(scriptedLevel);
        if (level.IsCutScene)
        {
            level = level.ParentLevel;
        }

        return !level.Data.Entities.Any(e => e.TypeID == TR1Type.TRex);
    }

    private bool IsBraidLevel(TR1ScriptedLevel lvl)
    {
        return _braidLevels.Contains(lvl);
    }

    private bool IsInvisibleLevel(TR1ScriptedLevel lvl)
    {
        return _invisibleLevels.Contains(lvl);
    }

    private bool IsGymLevel(TR1ScriptedLevel lvl)
    {
        return _gymLevels != null && _gymLevels.Contains(lvl);
    }

    private bool IsPartialGymLevel(TR1ScriptedLevel lvl)
    {
        return _partialGymLevels != null && _partialGymLevels.Contains(lvl);
    }

    private bool IsMauledLevel(TR1ScriptedLevel lvl)
    {
        return _mauledLevels.Contains(lvl);
    }

    internal class OutfitProcessor : AbstractProcessorThread<TR1OutfitRandomizer>
    {
        private static readonly List<TR1Type> _invisibleLaraEntities = new()
        {
            TR1Type.Lara, TR1Type.LaraPonytail_H_U,
            TR1Type.LaraPistolAnim_H, TR1Type.LaraShotgunAnim_H, TR1Type.LaraMagnumAnim_H,
            TR1Type.LaraUziAnimation_H, TR1Type.LaraMiscAnim_H, TR1Type.CutsceneActor1
        };

        private static readonly List<TR1Type> _ponytailEntities = new()
        {
            TR1Type.LaraPonytail_H_U
        };

        private static readonly List<TR1Type> _mauledEntities = new()
        {
            TR1Type.LaraMiscAnim_H_Valley
        };

        private static readonly Dictionary<TR1Type, Dictionary<EMTextureFaceType, int[]>> _headAmendments = new()
        {
            [TR1Type.Lara] = new Dictionary<EMTextureFaceType, int[]>
            {
                [EMTextureFaceType.Rectangles] = new int[] { 1 },
                [EMTextureFaceType.Triangles] = new int[] { 66, 67, 68, 69, 70, 71, 72, 73 }
            },
            [TR1Type.LaraUziAnimation_H] = new Dictionary<EMTextureFaceType, int[]>
            {
                [EMTextureFaceType.Rectangles] = new int[] { 6 },
                [EMTextureFaceType.Triangles] = new int[] { 56, 57, 58, 59, 60, 61, 62, 63 }
            },
            [TR1Type.CutsceneActor1] = new Dictionary<EMTextureFaceType, int[]>
            {
                // Applies only to CUT1.PHD - CUT2 and CUT4 use the Lara entry above.
                [EMTextureFaceType.Rectangles] = new int[] { 6 },
                [EMTextureFaceType.Triangles] = new int[] { 53, 54, 55, 56, 57, 58, 59, 60 }
            }
        };

        private readonly List<TR1CombinedLevel> _levels;

        internal override int LevelCount => _levels.Count;

        internal OutfitProcessor(TR1OutfitRandomizer outer)
            : base(outer)
        {
            _levels = new List<TR1CombinedLevel>();
        }

        internal void AddLevel(TR1CombinedLevel level)
        {
            _levels.Add(level);
        }

        protected override void ProcessImpl()
        {
            foreach (TR1CombinedLevel level in _levels)
            {
                if (_outer.IsBraidLevel(level.Script))
                {
                    // Only import the braid if Lara is visible. Note that it will automatically replace the model in Lost Valley.
                    if (!_outer.IsInvisibleLevel(level.Script))
                    {
                        ImportBraid(level);
                    }
                }
                else if (level.Is(TR1LevelNames.VALLEY))
                {
                    // The global setting may be on so we need to hide the OG braid
                    HideEntities(level, _ponytailEntities);
                }

                if (_outer.IsInvisibleLevel(level.Script))
                {
                    HideEntities(level, _invisibleLaraEntities);
                }
                else if (_outer.IsGymLevel(level.Script))
                {
                    ConvertToGymOutfit(level);
                }                   
                else if (_outer.IsPartialGymLevel(level.Script))
                {
                    ConvertToPartialGymOutfit(level);
                }
                else if (_outer.IsMauledLevel(level.Script))
                {
                    ConvertToMauledOutfit(level);
                }

                AmendBackpack(level);

                _outer.SaveLevel(level);

                if (!_outer.TriggerProgress())
                {
                    break;
                }
            }
        }

        private void ImportBraid(TR1CombinedLevel level)
        {
            TR1ModelImporter importer = new(true)
            {
                Level = level.Data,
                LevelName = level.Name,
                ClearUnusedSprites = false,
                EntitiesToImport = _ponytailEntities,
                TexturePositionMonitor = _outer.TextureMonitor.CreateMonitor(level.Name, _ponytailEntities),
                DataFolder = _outer.GetResourcePath(@"TR1\Models")
            };

            string remapPath = _outer.GetResourcePath(@"TR1\Textures\Deduplication\" + level.Name + "-TextureRemap.json");
            if (File.Exists(remapPath))
            {
                importer.TextureRemapPath = remapPath;
            }

            importer.Import();

            if (level.Data.Entities.Any(e => e.TypeID == TR1Type.MidasHand_N))
            {
                CreateGoldenBraid(level);
            }

            // Find the texture references for the plain parts of imported hair
            List<TRMesh> ponytailMeshes = TRMeshUtilities.GetModelMeshes(level.Data, TR1Type.LaraPonytail_H_U);

            ushort plainHairQuad = ponytailMeshes[0].TexturedRectangles[0].Texture;
            ushort plainHairTri = ponytailMeshes[5].TexturedTriangles[0].Texture;

            Dictionary<TR1Type, Dictionary<EMTextureFaceType, int[]>> headAmendments = _headAmendments;
            if (level.Is(TR1LevelNames.TIHOCAN_CUT) || level.Is(TR1LevelNames.ATLANTIS_CUT))
            {
                headAmendments = new Dictionary<TR1Type, Dictionary<EMTextureFaceType, int[]>>
                {
                    [TR1Type.CutsceneActor1] = _headAmendments[TR1Type.Lara]
                };
            }

            foreach (TR1Type laraType in headAmendments.Keys)
            {
                List<TRMesh> meshes = TRMeshUtilities.GetModelMeshes(level.Data, laraType);
                if (meshes == null || meshes.Count < 15)
                {
                    continue;
                }

                TRMesh headMesh = meshes[14];

                // Replace the hairband with plain hair - the imported ponytail has its own band so this is much tidier
                foreach (int face in headAmendments[laraType][EMTextureFaceType.Rectangles])
                {
                    headMesh.TexturedRectangles[face].Texture = plainHairQuad;
                }
                foreach (int face in headAmendments[laraType][EMTextureFaceType.Triangles])
                {
                    headMesh.TexturedTriangles[face].Texture = plainHairTri;
                }

                // Move the base of Lara's bun up so the ponytail looks more natural
                headMesh.Vertices[^1].Y = 36;
                headMesh.Vertices[^2].Y = 38;
                headMesh.Vertices[^3].Y = 38;
            }

            if (CutsceneSupportsBraid(level))
            {
                ImportBraid(level.CutSceneLevel);
                level.CutSceneLevel.Script.LaraType = (uint)TR1Type.CutsceneActor1;
            }
        }
        
        private static void CreateGoldenBraid(TR1CombinedLevel level)
        {
            TRMesh goldenHips = TRMeshUtilities.GetModelFirstMesh(level.Data, TR1Type.LaraMiscAnim_H);
            ushort goldPalette = goldenHips.ColouredRectangles[0].Texture;

            TRModel ponytail = level.Data.Models.Find(m => m.ID == (uint)TR1Type.LaraPonytail_H_U);
            List<TRMesh> ponytailMeshes = TRMeshUtilities.GetModelMeshes(level.Data, ponytail);
            MeshEditor editor = new();
            foreach (TRMesh mesh in ponytailMeshes)
            {
                TRMesh clonedMesh = MeshEditor.CloneMeshAsColoured(mesh, goldPalette);
                TRMeshUtilities.InsertMesh(level.Data, clonedMesh);
            }

            ponytail.MeshTrees.AddRange(ponytail.MeshTrees);
            ponytail.NumMeshes *= 2;
        }

        private static void HideEntities(TR1CombinedLevel level, IEnumerable<TR1Type> entities)
        {
            MeshEditor editor = new();
            foreach (TR1Type ent in entities)
            {
                List<TRMesh> meshes = TRMeshUtilities.GetModelMeshes(level.Data, ent);
                if (meshes != null)
                {
                    foreach (TRMesh mesh in meshes)
                    {
                        editor.Mesh = mesh;
                        editor.ClearAllPolygons();
                        editor.WriteToLevel(level.Data);
                    }
                }
            }

            // Repeat the process if there is a cutscene after this level.
            // Skip Mines because CutsceneActor1 is Natla, not Lara.
            if (level.HasCutScene && !level.CutSceneLevel.Is(TR1LevelNames.MINES_CUT))
            {
                HideEntities(level.CutSceneLevel, entities);
            }
        }

        private void AmendBackpack(TR1CombinedLevel level)
        {
            bool trexPresent = level.Data.Entities.Any(e => e.TypeID == TR1Type.TRex)
                || (level.IsCutScene && level.ParentLevel.Data.Entities.Any(e => e.TypeID == TR1Type.TRex));
            bool braidLevel = _outer.IsBraidLevel(level.Script) || (level.IsCutScene && _outer.IsBraidLevel(level.ParentLevel.Script));
            bool invisibleLevel = _outer.IsInvisibleLevel(level.Script) || (level.IsCutScene && _outer.IsInvisibleLevel(level.ParentLevel.Script));
            bool gymLevel = _outer.IsGymLevel(level.Script) || _outer.IsPartialGymLevel(level.Script) ||
                (level.IsCutScene && (_outer.IsGymLevel(level.ParentLevel.Script) || _outer.IsPartialGymLevel(level.ParentLevel.Script)));

            if (!braidLevel || invisibleLevel || (gymLevel && !trexPresent))
            {
                return;
            }

            List<TR1Type> laraEntities = new();
            if (level.IsCutScene)
            {
                laraEntities.Add(TR1Type.CutsceneActor1);
            }
            else
            {
                laraEntities.Add(TR1Type.Lara);
                laraEntities.Add(TR1Type.LaraShotgunAnim_H);
                if (trexPresent)
                {
                    laraEntities.Add(TR1Type.LaraMiscAnim_H);
                }
            }

            // Make the backpack shallower so the braid doesn't smash into it
            foreach (TR1Type ent in laraEntities)
            {
                TRMesh mesh = TRMeshUtilities.GetModelMeshes(level.Data, ent)[7];
                for (int i = 26; i < 30; i++)
                {
                    mesh.Vertices[i].Z += 12;
                }
            }

            if (CutsceneSupportsBraid(level))
            {
                AmendBackpack(level.CutSceneLevel);
            }
        }

        private bool CutsceneSupportsBraid(TR1CombinedLevel parentLevel)
        {
            if (!parentLevel.HasCutScene || parentLevel.Is(TR1LevelNames.MINES))
            {
                return false;
            }

            // Not supported before 2.13, so don't make any changes to Lara here.
            Version tr1xVersion = _outer.ScriptEditor.Edition.ExeVersion;
            if (tr1xVersion == null || tr1xVersion < _minBraidCutsceneVersion)
            {
                return false;
            }

            if (parentLevel.Is(TR1LevelNames.ATLANTIS))
            {
                // Lara's head may be Natla's or Pierre's, so only support the braid if
                // the mesh is the original.
                TRMesh larasHead = TRMeshUtilities.GetModelMeshes(parentLevel.CutSceneLevel.Data, TR1Type.CutsceneActor1)[14];
                return larasHead.CollRadius == 68;
            }

            return true;
        }

        private void ConvertToGymOutfit(TR1CombinedLevel level)
        {
            if (!ImportGymOutfit(level))
            {
                return;
            }

            List<TRMesh> lara = TRMeshUtilities.GetModelMeshes(level.Data, level.IsCutScene ? TR1Type.CutsceneActor1 : TR1Type.Lara);
            List<TRMesh> laraPistol = TRMeshUtilities.GetModelMeshes(level.Data, TR1Type.LaraPistolAnim_H);
            List<TRMesh> laraShotgun = TRMeshUtilities.GetModelMeshes(level.Data, TR1Type.LaraShotgunAnim_H);
            List<TRMesh> laraMagnums = TRMeshUtilities.GetModelMeshes(level.Data, TR1Type.LaraMagnumAnim_H);
            List<TRMesh> laraUzis = TRMeshUtilities.GetModelMeshes(level.Data, TR1Type.LaraUziAnimation_H);
            List<TRMesh> laraMisc = TRMeshUtilities.GetModelMeshes(level.Data, TR1Type.LaraMiscAnim_H);

            // Basic meshes to take from LaraMiscAnim. We don't replace Lara's gloves
            // or thighs (at this stage - handled below with gun swaps).
            int[] basicLaraIndices = new int[] { 0, 2, 3, 5, 6, 7, 8, 9, 11, 12 };
            foreach (int index in basicLaraIndices)
            {
                TRMeshUtilities.DuplicateMesh(level.Data, lara[index], laraMisc[index]);
            }

            // Copy the guns and holsters from the original models and paste them
            // onto each of Lara's thighs. The ranges are the specific faces we want
            // to copy from each.
            int[] thighs = new int[] { 1, 4 };
            foreach (int thigh in thighs)
            {
                // Empty holsters. The ToQ cutscene actor model is notoriously awkward, hence
                // special handling here to ensure we pick the right quads.
                bool qualopec_cut = level.Is(TR1LevelNames.QUALOPEC_CUT) && thigh == 1;
                CopyMeshParts(level.Data, new MeshCopyData
                {
                    BaseMesh = lara[thigh],
                    NewMesh = laraMisc[thigh],
                    ColourFaceCopies = Enumerable.Range(qualopec_cut ? 2 : 8, 6),
                    TextureFaceCopies = qualopec_cut ? Enumerable.Range(1, 5) : null
                });

                // Holstered pistols
                if (laraPistol != null)
                {
                    CopyMeshParts(level.Data, new MeshCopyData
                    {
                        BaseMesh = laraPistol[thigh],
                        NewMesh = laraMisc[thigh],
                        ColourFaceCopies = Enumerable.Range(4, 6),
                        TextureFaceCopies = Enumerable.Range(5, 5)
                    });
                }

                // Holstered magnums
                if (laraMagnums != null)
                {
                    CopyMeshParts(level.Data, new MeshCopyData
                    {
                        BaseMesh = laraMagnums[thigh],
                        NewMesh = laraMisc[thigh],
                        ColourFaceCopies = Enumerable.Range(4, 6),
                        TextureFaceCopies = Enumerable.Range(3, 5)
                    });
                }

                // Holstered uzis
                if (laraUzis != null)
                {
                    CopyMeshParts(level.Data, new MeshCopyData
                    {
                        BaseMesh = laraUzis[thigh],
                        NewMesh = laraMisc[thigh],
                        ColourFaceCopies = Enumerable.Range(4, 7),
                        TextureFaceCopies = Enumerable.Range(3, 19)
                    });
                }
            }

            // Don't forget the shotgun on her back
            if (laraShotgun != null)
            {
                CopyMeshParts(level.Data, new MeshCopyData
                {
                    BaseMesh = laraShotgun[7],
                    NewMesh = laraMisc[7],
                    TextureFaceCopies = Enumerable.Range(8, 12)
                });
            }

            // Replace Lara's footstep SFX. This is done independently of Audio rando in case that is not enabled.
            foreach (TR1SFX sfxID in _outer._barefootSfx.Keys)
            {
                level.Data.SoundEffects[sfxID] = _outer._barefootSfx[sfxID];
            }

            // If there is a Midas hand present, convert the LaraMiscAnim gym textures to gold. For meshes 1, 4, and 14
            // we use Lara's main model as a base to ensure it matches previous edits.
            List<int> laraBaseIndices = new() { 1, 4, 14 };
            CreateGoldGymOutfit(level, laraMisc, i => laraBaseIndices.Contains(i) ? lara[i] : laraMisc[i]);

            if (level.HasCutScene)
            {
                ConvertToGymOutfit(level.CutSceneLevel);
            }
        }

        private void ConvertToPartialGymOutfit(TR1CombinedLevel level)
        {
            if (!ImportGymOutfit(level))
            {
                return;
            }

            List<TRMesh> lara = TRMeshUtilities.GetModelMeshes(level.Data, level.IsCutScene ? TR1Type.CutsceneActor1 : TR1Type.Lara);
            List<TRMesh> laraShotgun = TRMeshUtilities.GetModelMeshes(level.Data, TR1Type.LaraShotgunAnim_H);
            List<TRMesh> laraMisc = TRMeshUtilities.GetModelMeshes(level.Data, TR1Type.LaraMiscAnim_H);

            // Just the torso
            TRMeshUtilities.DuplicateMesh(level.Data, lara[7], laraMisc[7]);
            
            using (TR1TexturePacker packer = new(level.Data))
            {
                // Replace the blue parts on Lara's hips with skin tone
                List<int> faces = new() { 5, 6, 7 };
                foreach (int face in faces)
                {
                    Dictionary<TexturedTile, List<TexturedTileSegment>> segments = packer.GetObjectTextureSegments(new List<int> { lara[0].TexturedRectangles[face].Texture });
                    foreach (TexturedTile tile in segments.Keys)
                    {
                        int index = -1;
                        Rectangle rect = segments[tile].First().Bounds;
                        tile.BitmapGraphics.Scan(rect, (c, x, y) =>
                        {
                            if (y - rect.Y < 2 || face == 7)
                            {
                                if (c.A != 0 && index == -1)
                                {
                                    // Top-left is skin tone in each of these cases.
                                    index = level.Data.Images8[tile.Index].Pixels[y * TRConsts.TPageWidth + x];
                                }

                                level.Data.Images8[tile.Index].Pixels[y * TRConsts.TPageWidth + x] = (byte)(c.A == 0 ? 0 : index);
                            }
                            return c;
                        });
                    }
                }
            }

            if (laraShotgun != null)
            {
                CopyMeshParts(level.Data, new MeshCopyData
                {
                    BaseMesh = laraShotgun[7],
                    NewMesh = laraMisc[7],
                    TextureFaceCopies = Enumerable.Range(8, 12)
                });
            }

            // Avoid remnants of Lara's default outfit affecting this partial outfit
            // in texture randomization.
            _outer.TextureMonitor.CreateMonitor(level.Name).UseLaraOutfitTextures = false;

            // If there is a Midas hand present, revert the LaraMiscAnim gym textures to standard gold.
            CreateGoldGymOutfit(level, laraMisc, i => lara[i]);

            if (level.HasCutScene)
            {
                ConvertToPartialGymOutfit(level.CutSceneLevel);
            }
        }

        private static void CreateGoldGymOutfit(TR1CombinedLevel level, List<TRMesh> laraMisc, Func<int, TRMesh> cloneBaseFunc)
        {
            if (!level.Data.Entities.Any(e => e.TypeID == TR1Type.MidasHand_N))
            {
                return;
            }

            int goldIndex = level.Data.Palette.Select(c => c.ToTR1Color())
                    .FindClosest(_midasGoldColour, 1);
            MeshEditor editor = new();

            for (int i = 0; i < laraMisc.Count; i++)
            {
                TRMesh mesh = laraMisc[i];
                TRMeshUtilities.DuplicateMesh(level.Data, mesh, MeshEditor.CloneMesh(cloneBaseFunc(i)));
                editor.Mesh = mesh;

                foreach (TRMeshFace face in mesh.TexturedRectangles)
                {
                    editor.AddColouredRectangle(face);
                }
                foreach (TRMeshFace face in mesh.TexturedTriangles)
                {
                    editor.AddColouredTriangle(face);
                }

                editor.ClearTexturedRectangles();
                editor.ClearTexturedTriangles();

                editor.Mesh.ColouredRectangles.ForEach(f => f.Texture = (ushort)goldIndex);
                editor.Mesh.ColouredTriangles.ForEach(f => f.Texture = (ushort)goldIndex);
            }
        }

        private bool ImportGymOutfit(TR1CombinedLevel level)
        {
            if (level.Is(TR1LevelNames.MINES_CUT))
            {
                // Lara isn't here.
                return false;
            }

            TRModel existingModel = level.Data.Models.Find(m => m.ID == (uint)TR1Type.LaraMiscAnim_H);
            if (existingModel != null)
            {
                // If we already have the gym outfit available, we're done.
                List<TRMesh> meshes = TRMeshUtilities.GetModelMeshes(level.Data, existingModel);
                if (meshes.ComputeSkeletonHash() == _gymOutfitHash)
                {
                    return true;
                }
            }

            TR1ModelImporter importer = new(true)
            {
                Level = level.Data,
                LevelName = level.Name,
                ClearUnusedSprites = false,
                EntitiesToImport = new List<TR1Type> { TR1Type.LaraMiscAnim_H_General },
                DataFolder = _outer.GetResourcePath(@"TR1\Models")
            };

            string remapPath = _outer.GetResourcePath($@"TR1\Textures\Deduplication\{level.Name}-TextureRemap.json");
            if (File.Exists(remapPath))
            {
                importer.TextureRemapPath = remapPath;
            }

            try
            {
                importer.Import();
                // If we already have an existing model, restore the animation and frames
                // e.g. for Adam death animation and scion pickups.
                if (existingModel != null)
                {
                    TRModel newModel = level.Data.Models.Find(m => m.ID == (uint)TR1Type.LaraMiscAnim_H);
                    newModel.Animation = existingModel.Animation;
                    newModel.FrameOffset = existingModel.FrameOffset;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void CopyMeshParts(TR1Level level, MeshCopyData data)
        {
            MeshEditor editor = new();
            TRMeshUtilities.InsertMesh(level, editor.Mesh = MeshEditor.CloneMesh(data.NewMesh));

            if (data.TextureFaceCopies != null)
            {
                foreach (int faceIndex in data.TextureFaceCopies)
                {
                    TRMeshFace face = data.BaseMesh.TexturedRectangles[faceIndex];
                    ushort[] vertexPointers = new ushort[4];
                    for (int j = 0; j < vertexPointers.Length; j++)
                    {
                        TRVertex origVertex = data.BaseMesh.Vertices[face.Vertices[j]];
                        int newVertIndex = editor.Mesh.Vertices.FindIndex(v => v.X == origVertex.X && v.Y == origVertex.Y && v.Z == origVertex.Z);
                        if (newVertIndex == -1)
                        {
                            newVertIndex = editor.Mesh.Vertices.Count;
                            editor.Mesh.Vertices.Add(origVertex);
                            if (face.Vertices[j] < data.BaseMesh.Normals.Count)
                            {
                                editor.Mesh.Normals.Add(data.BaseMesh.Normals[face.Vertices[j]]);
                            }
                        }
                        vertexPointers[j] = (ushort)newVertIndex;
                    }

                    editor.Mesh.TexturedRectangles.Add(new()
                    {
                        Texture = face.Texture,
                        Vertices = vertexPointers.ToList()
                    });
                }
            }

            if (data.ColourFaceCopies != null)
            {
                foreach (int faceIndex in data.ColourFaceCopies)
                {
                    TRMeshFace face = data.BaseMesh.ColouredRectangles[faceIndex];
                    ushort[] vertexPointers = new ushort[4];
                    for (int j = 0; j < vertexPointers.Length; j++)
                    {
                        TRVertex origVertex = data.BaseMesh.Vertices[face.Vertices[j]];
                        int newVertIndex = editor.Mesh.Vertices.FindIndex(v => v.X == origVertex.X && v.Y == origVertex.Y && v.Z == origVertex.Z);
                        if (newVertIndex == -1)
                        {
                            newVertIndex = editor.Mesh.Vertices.Count;
                            editor.Mesh.Vertices.Add(origVertex);
                            if (face.Vertices[j] < data.BaseMesh.Normals.Count)
                            {
                                editor.Mesh.Normals.Add(data.BaseMesh.Normals[face.Vertices[j]]);
                            }
                        }
                        vertexPointers[j] = (ushort)newVertIndex;
                    }

                    editor.Mesh.ColouredRectangles.Add(new()
                    {
                        Texture = face.Texture,
                        Vertices = vertexPointers.ToList()
                    });
                }
            }

            editor.Mesh.CollRadius = data.BaseMesh.CollRadius;
            editor.WriteToLevel(level);

            TRMeshUtilities.DuplicateMesh(level, data.BaseMesh, editor.Mesh);
        }

        private void ConvertToMauledOutfit(TR1CombinedLevel level)
        {
            List<TRMesh> lara = TRMeshUtilities.GetModelMeshes(level.Data, level.IsCutScene ? TR1Type.CutsceneActor1 : TR1Type.Lara);
            List<TRMesh> laraShotgun = TRMeshUtilities.GetModelMeshes(level.Data, TR1Type.LaraShotgunAnim_H);
            List<TRMesh> laraMisc = TRMeshUtilities.GetModelMeshes(level.Data, TR1Type.LaraMiscAnim_H);

            if (level.Is(TR1LevelNames.QUALOPEC_CUT))
            {
                // This model is completely different to all others, so just
                // duplicate the mauled meshes in this case.
                List<int> meshIndices = new() { 1, 4, 5, 7, 9, 11, 12 };
                foreach (int index in meshIndices)
                {
                    int colRad = lara[index].CollRadius;
                    TRMeshUtilities.DuplicateMesh(level.Data, lara[index], laraMisc[index]);
                    lara[index].CollRadius = colRad;
                }
            }
            else
            {
                // Left leg
                ReplaceTexture(lara[1], laraMisc[1], 1, 2, 0);
                ConvertColourToTexture(lara[1], laraMisc[1], 3, 1, 0);
                ConvertColourToTexture(lara[1], laraMisc[1], 5, 5, 0);

                // Right leg
                ConvertColourToTexture(lara[4], laraMisc[4], 5, 3, 1);
                ConvertColourToTexture(lara[5], laraMisc[5], 1, 0, 1);

                // Torso
                ConvertColourToTexture(lara[7], laraMisc[7], 1, 2, 2);
                ReplaceTexture(lara[7], laraMisc[7], 0, 0, 0);
                ReplaceTexture(lara[7], laraMisc[7], 5, 8, 0);
                ReplaceTexture(lara[7], laraMisc[7], 3, 6, 0);

                // Left arm
                ConvertColourToTexture(lara[9], laraMisc[9], 2, 0, 1);
                ConvertColourToTexture(lara[9], laraMisc[9], 3, 1, 1);

                // Right arm
                ConvertColourToTexture(lara[11], laraMisc[11], 3, 0, 0);
                ConvertColourToTexture(lara[12], laraMisc[12], 0, 0, 1);

                // Shotgun - Torso
                if (laraShotgun != null)
                {
                    ConvertColourToTexture(laraShotgun[7], laraMisc[7], 3, 2, 2);
                    ReplaceTexture(laraShotgun[7], laraMisc[7], 0, 0, 0);
                    ReplaceTexture(laraShotgun[7], laraMisc[7], 7, 8, 0);
                    ReplaceTexture(laraShotgun[7], laraMisc[7], 5, 6, 0);
                }
            }

            // Some commonality between the holstered guns
            List<TR1Type> gunAnims = new()
            {
                TR1Type.LaraPistolAnim_H, TR1Type.LaraMagnumAnim_H, TR1Type.LaraUziAnimation_H
            };
            foreach (TR1Type gunAnimType in gunAnims)
            {
                List<TRMesh> meshes = TRMeshUtilities.GetModelMeshes(level.Data, gunAnimType);
                if (meshes == null)
                    continue;

                // Left leg
                ReplaceTexture(meshes[1], laraMisc[1], 1, 2, 0);
                ConvertColourToTexture(meshes[1], laraMisc[1], 1, 1, 0);
                MergeColouredTrianglesToTexture(level.Data, meshes[1], laraMisc[1], new int[] { 13, 9 }, 5, 2);

                // Right leg
                MergeColouredTrianglesToTexture(level.Data, meshes[4], laraMisc[4], new int[] { 12, 8 }, 3, 3);
            }

            if (level.HasCutScene && !level.Is(TR1LevelNames.MINES))
            {
                TR1ModelImporter importer = new(true)
                {
                    Level = level.CutSceneLevel.Data,
                    LevelName = level.CutSceneLevel.Name,
                    ClearUnusedSprites = false,
                    EntitiesToImport = _mauledEntities,
                    TexturePositionMonitor = _outer.TextureMonitor.CreateMonitor(level.CutSceneLevel.Name, _mauledEntities),
                    DataFolder = _outer.GetResourcePath(@"TR1\Models")
                };

                string remapPath = _outer.GetResourcePath(@"TR1\Textures\Deduplication\" + level.CutSceneLevel.Name + "-TextureRemap.json");
                if (File.Exists(remapPath))
                {
                    importer.TextureRemapPath = remapPath;
                }

                importer.Import();
                ConvertToMauledOutfit(level.CutSceneLevel);
            }
        }

        private static void ReplaceTexture(TRMesh baseMesh, TRMesh copyMesh, int baseIndex, int copyIndex, int rotations)
        {
            TRMeshFace face = baseMesh.TexturedRectangles[baseIndex];
            face.Texture = copyMesh.TexturedRectangles[copyIndex].Texture;

            RotateFace(face, rotations);
        }

        private static void ConvertColourToTexture(TRMesh baseMesh, TRMesh copyMesh, int baseIndex, int copyIndex, int rotations)
        {
            TRMeshFace face = baseMesh.ColouredRectangles[baseIndex];
            baseMesh.ColouredRectangles.Remove(face);
            baseMesh.TexturedRectangles.Add(face);
            face.Texture = copyMesh.TexturedRectangles[copyIndex].Texture;

            RotateFace(face, rotations);
        }

        private static void MergeColouredTrianglesToTexture(TR1Level level, TRMesh baseMesh, TRMesh copyMesh, int[] triangleIndices, int copyIndex, int rotations)
        {
            MeshEditor editor = new()
            {
                Mesh = baseMesh
            };

            List<int> indices = triangleIndices.ToList();
            indices.Sort();

            List<ushort> vertices = new();
            foreach (int index in indices)
            {
                TRMeshFace face = baseMesh.ColouredTriangles[index];
                foreach (ushort vert in face.Vertices)
                {
                    if (!vertices.Contains(vert))
                    {
                        vertices.Add(vert);
                    }
                }
            }

            indices.Reverse();
            foreach (int index in indices)
            {
                baseMesh.ColouredTriangles.RemoveAt(index);
            }

            baseMesh.ColouredRectangles.Add(new()
            {
                Vertices = vertices
            });

            editor.WriteToLevel(level);

            ConvertColourToTexture(baseMesh, copyMesh, baseMesh.ColouredRectangles.Count - 1, copyIndex, rotations);
        }

        private static void RotateFace(TRMeshFace face, int rotations)
        {
            if (rotations > 0)
            {
                Queue<ushort> queue = new(face.Vertices);
                Stack<ushort> stack = new();

                while (rotations > 0)
                {
                    stack.Push(queue.Dequeue());
                    queue.Enqueue(stack.Pop());
                    rotations--;
                }

                face.Vertices = queue.ToList();
            }
        }
    }
}
