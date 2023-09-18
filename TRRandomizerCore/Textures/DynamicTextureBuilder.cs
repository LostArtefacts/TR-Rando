using System.Drawing;
using TRFDControl;
using TRFDControl.Utilities;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRModelTransporter.Helpers;
using TRModelTransporter.Model.Definitions;
using TRModelTransporter.Model.Textures;
using TRModelTransporter.Packing;
using TRModelTransporter.Transport;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Utilities;
using TRTexture16Importer.Textures;

namespace TRRandomizerCore.Textures;

public class DynamicTextureBuilder
{
    // Models whose mesh textures should be targeted
    private static readonly List<TR1Type> _modelIDs = new()
    {
        TR1Type.Door1, TR1Type.Door2, TR1Type.Door3, TR1Type.Door4,
        TR1Type.Door5, TR1Type.Door6, TR1Type.Door7, TR1Type.Door8,
        TR1Type.Trapdoor1, TR1Type.Trapdoor2, TR1Type.Trapdoor3, TR1Type.LiftingDoor,
        TR1Type.WallSwitch, TR1Type.UnderwaterSwitch, TR1Type.DamoclesSword,
        TR1Type.BridgeFlat, TR1Type.BridgeTilt1, TR1Type.BridgeTilt2,
        TR1Type.SwingingBlade, TR1Type.PushBlock1, TR1Type.PushBlock2,
        TR1Type.PushBlock3, TR1Type.PushBlock4, TR1Type.MovingBlock, TR1Type.RollingBall,
        TR1Type.FallingBlock, TR1Type.FallingCeiling1, TR1Type.FallingCeiling2,
        TR1Type.DartEmitter, TR1Type.Dart_H, TR1Type.TeethSpikes, TR1Type.Keyhole1,
        TR1Type.Keyhole2, TR1Type.Keyhole3, TR1Type.Keyhole4, TR1Type.PuzzleHole1,
        TR1Type.PuzzleHole2, TR1Type.PuzzleHole3, TR1Type.PuzzleHole4,
        TR1Type.PuzzleDone1,TR1Type.PuzzleDone2,TR1Type.PuzzleDone3,TR1Type.PuzzleDone4,
        TR1Type.Animating1, TR1Type.Animating2, TR1Type.Animating3,
        TR1Type.Motorboat, TR1Type.Barricade, TR1Type.ThorHammerBlock, TR1Type.ThorHammerHandle,
        TR1Type.ThorLightning, TR1Type.SlammingDoor, TR1Type.CentaurStatue, TR1Type.NatlasMineShack,
        TR1Type.ScionHolder, TR1Type.AtlanteanLava, TR1Type.AdamEgg, TR1Type.AtlanteanEgg,
        TR1Type.ScionPiece3_S_P, TR1Type.ScionPiece4_S_P, TR1Type.Gunflare_H
    };

    // Enemy models whose mesh textures should be targeted
    private static readonly List<TR1Type> _enemyIDs = new()
    {
        TR1Type.Adam, TR1Type.Missile2_H, TR1Type.Missile3_H, TR1Type.FlyingAtlantean,
        TR1Type.BandagedFlyer, TR1Type.Centaur, TR1Type.Mummy, TR1Type.Doppelganger,
        TR1Type.TRex, TR1Type.Raptor, TR1Type.Cowboy
    };

    // Sprite sequences that should be targeted
    private static readonly List<TR1Type> _spriteIDs = new()
    {
        TR1Type.LavaParticles_S_H, TR1Type.Flame_S_H, TR1Type.Explosion1_S_H,
        TR1Type.DartEffect_S_H, TR1Type.WaterRipples1_S_H, TR1Type.WaterRipples2_S_H,
        TR1Type.FontGraphics_S_H, TR1Type.Ricochet_S_H, TR1Type.Sparkles_S_H
    };

    public TextureMonitor<TR1Type> TextureMonitor { get; set; }
    public bool RetainMainTextures { get; set; }
    public bool IsCommunityPatch { get; set; }

    public DynamicTextureTarget Build(TR1CombinedLevel level)
    {
        ISet<int> defaultObjectTextures = new HashSet<int>();
        ISet<int> defaultSpriteTextures = new HashSet<int>();
        ISet<int> enemyObjectTextures = new HashSet<int>();
        ISet<int> secretObjectTextures = new HashSet<int>();
        ISet<int> secretSpriteTextures = new HashSet<int>();
        ISet<int> keyItemObjectTextures = new HashSet<int>();
        ISet<int> keyItemSpriteTextures = new HashSet<int>();

        ISet<TRMesh> modelMeshes = new HashSet<TRMesh>();

        // Collect unique room and room sprite textures
        foreach (TRRoom room in level.Data.Rooms)
        {
            foreach (TRFace3 f in room.RoomData.Triangles)
                defaultObjectTextures.Add(f.Texture);
            foreach (TRFace4 f in room.RoomData.Rectangles)
                defaultObjectTextures.Add(f.Texture);
            foreach (TRRoomSprite sprite in room.RoomData.Sprites)
            {
                // Only add ones that aren't also pickups
                if (Array.Find(level.Data.SpriteSequences, s => s.Offset == sprite.Texture 
                    && level.Data.Entities.Find(e => e.TypeID == s.SpriteID) != null) == null)
                {
                    defaultSpriteTextures.Add(sprite.Texture);
                }
            }
        }

        // Include all static mesh textures
        foreach (TRStaticMesh smesh in level.Data.StaticMeshes)
        {
            TRMesh mesh = TRMeshUtilities.GetMesh(level.Data, smesh.Mesh);
            AddMeshTextures(mesh, defaultObjectTextures);
            if (!RetainMainTextures)
            {
                modelMeshes.Add(mesh);
            }
        }

        // Collect standard sprite sequences
        foreach (TR1Type spriteID in _spriteIDs)
        {
            AddSpriteTextures(level.Data, spriteID, defaultSpriteTextures);
        }

        TRMesh hips = null;
        List<TR1Type> modelIDs = new(_modelIDs);
        if (level.IsCutScene)
        {
            // Cutscene actors vary between levels so we can't include all by default. These
            // are the only ones we want to change.
            if (level.Is(TR1LevelNames.MINES_CUT))
            {
                modelIDs.Add(TR1Type.CutsceneActor1); // ScionHolder
                modelIDs.Add(TR1Type.CutsceneActor3); // Scion
            }
            else if (level.Is(TR1LevelNames.ATLANTIS_CUT))
            {
                modelIDs.Add(TR1Type.CutsceneActor2); // ScionHolder
                modelIDs.Add(TR1Type.CutsceneActor4); // Scion
            }
        }
        else
        {
            hips = TRMeshUtilities.GetModelMeshes(level.Data, TR1Type.Lara)[0];
            if (level.Is(TR1LevelNames.MIDAS))
            {
                modelIDs.Add(TR1Type.LaraMiscAnim_H);
                modelIDs.Add(TR1Type.LaraPonytail_H_U);
            }
        }

        // Collect all model mesh textures, provided none use the dummy mesh, otherwise
        // Lara will be partially re-textured.
        foreach (TR1Type modelID in modelIDs)
        {
            TRModel model = Array.Find(level.Data.Models, m => m.ID == (uint)modelID);
            if (model != null)
            {
                AddModelTextures(level.Data, model, hips, defaultObjectTextures, modelMeshes);
            }
        }

        foreach (TR1Type modelID in _enemyIDs)
        {
            TRModel model = Array.Find(level.Data.Models, m => m.ID == (uint)modelID);
            if (model != null)
            {
                AddModelTextures(level.Data, model, hips, enemyObjectTextures, modelMeshes);
            }
        }

        // If anything we have collected so far is an animated texture, add the other
        // textures from the same animation list.
        foreach (int texture in defaultObjectTextures.ToList())
        {
            TRAnimatedTexture anim = Array.Find(level.Data.AnimatedTextures, a => a.Textures.Contains((ushort)texture));
            if (anim != null)
            {
                foreach (ushort animTexture in anim.Textures)
                {
                    defaultObjectTextures.Add(animTexture);
                }
            }
        }

        // Key items and secrets
        FDControl floorData = new();
        floorData.ParseFromLevel(level.Data);

        Dictionary<TR1Type, TR1Type> keyItems = TR1TypeUtilities.GetKeyItemMap();
        foreach (TR1Type pickupType in keyItems.Keys)
        {
            TRModel model = Array.Find(level.Data.Models, m => m.ID == (uint)keyItems[pickupType]);
            if (model == null)
            {
                continue;
            }

            // Find an entity of this type and check if it's a secret
            TR1Entity keyInstance = level.Data.Entities.Find(e => e.TypeID == (short)pickupType);
            if (keyInstance != null)
            {
                TRRoomSector sector = FDUtilities.GetRoomSector(keyInstance.X, keyInstance.Y, keyInstance.Z, keyInstance.Room, level.Data, floorData);
                if (LocationUtilities.SectorContainsSecret(sector, floorData))
                {
                    AddModelTextures(level.Data, model, hips, secretObjectTextures, modelMeshes);
                    AddSpriteTextures(level.Data, pickupType, secretSpriteTextures);
                    continue;
                }
            }
                
            // Otherwise it's a regular key item
            AddModelTextures(level.Data, model, hips, keyItemObjectTextures, modelMeshes);
            AddSpriteTextures(level.Data, pickupType, keyItemSpriteTextures);
        }

        // Put it all together into the required format for texture re-mapping.
        using TR1TexturePacker packer = new(level.Data);
        Dictionary<int, List<Rectangle>> defaultMapping = new();
        AddSegmentsToMapping(packer.GetObjectTextureSegments(defaultObjectTextures), defaultMapping);
        AddSegmentsToMapping(packer.GetSpriteTextureSegments(defaultSpriteTextures), defaultMapping);

        Dictionary<TextureCategory, Dictionary<int, List<Rectangle>>> optionalMapping = new()
        {
            [TextureCategory.Enemy] = new Dictionary<int, List<Rectangle>>(),
            [TextureCategory.Secret] = new Dictionary<int, List<Rectangle>>(),
            [TextureCategory.KeyItem] = new Dictionary<int, List<Rectangle>>()
        };
        AddSegmentsToMapping(packer.GetObjectTextureSegments(enemyObjectTextures), optionalMapping[TextureCategory.Enemy]);
        AddSegmentsToMapping(packer.GetObjectTextureSegments(secretObjectTextures), optionalMapping[TextureCategory.Secret]);
        AddSegmentsToMapping(packer.GetSpriteTextureSegments(secretSpriteTextures), optionalMapping[TextureCategory.Secret]);
        AddSegmentsToMapping(packer.GetObjectTextureSegments(keyItemObjectTextures), optionalMapping[TextureCategory.KeyItem]);
        AddSegmentsToMapping(packer.GetSpriteTextureSegments(keyItemSpriteTextures), optionalMapping[TextureCategory.KeyItem]);

        return new DynamicTextureTarget
        {
            DefaultTileTargets = defaultMapping,
            OptionalTileTargets = optionalMapping,
            ModelColourTargets = modelMeshes.ToList()
        };
    }

    private void AddModelTextures(TR1Level level, TRModel model, TRMesh dummyMesh, ISet<int> textures, ISet<TRMesh> meshCollection)
    {
        TR1Type modelID = (TR1Type)model.ID;
        if (TextureMonitor?.RemovedTextures?.Contains(modelID) ?? false)
        {
            // Don't include textures that may have been re-assigned to other imported models (e.g. enemies).
            return;
        }

        if (modelID == TR1Type.CentaurStatue && level.Entities.Find(e => e.TypeID == model.ID) == null)
        {
            // Can happen in ToT if the centaur statue was "removed", in which case we don't want to
            // change any object textures that were repurposed for new enemies.
            return;
        }

        if (modelID == TR1Type.Cowboy && TRMeshUtilities.GetModelMeshes(level, TR1Type.Cowboy)[2].NumTexturedRectangles > 0)
        {
            // We only want to target LeoC's headless cowboy - in this case the cowboy is OG.
            return;
        }

        TRMesh[] meshes = TRMeshUtilities.GetModelMeshes(level, model);
        List<TRMesh> excludedMeshes = new() { dummyMesh };

        if (modelID == TR1Type.Adam)
        {                
            TR1ModelDefinition adam = new TR1ModelImporter
            {
                DataFolder = @"Resources\TR1\Models"
            }.LoadDefinition(modelID);

            if (meshes[3].CollRadius != adam.Meshes[3].CollRadius)
            {
                try
                {
                    // Adam's head may have been replaced by Lara's, Pierre's etc. Try to
                    // create duplicates of the mesh's textures so we don't corrupt the
                    // original model.
                    DuplicateMeshTextures(level, meshes[3]);
                }
                catch
                {
                    // If packing failed, just exclude this particular mesh.
                    excludedMeshes.Add(meshes[3]);
                }
            }
        }
        else if ((modelID == TR1Type.ScionPiece3_S_P || modelID == TR1Type.ScionPiece4_S_P)
            && meshes.Length == 1 && meshes[0].NumNormals != 123)
        {
            try
            {
                // The scion is something else so try to duplicate it to avoid original
                // model issues.
                DuplicateMeshTextures(level, meshes[0]);
            }
            catch
            {
                // If packing failed, just exclude this particular mesh.
                excludedMeshes.Add(meshes[0]);
            }
        }
        else if (modelID == TR1Type.LaraPonytail_H_U)
        {
            // For Midas "golden" hair, we only want the additional meshes that may have
            // been created by outfit rando.
            for (int i = 0; i < 6; i++)
            {
                excludedMeshes.Add(meshes[i]);
            }
        }

        foreach (TRMesh mesh in meshes)
        {
            if (!excludedMeshes.Contains(mesh))
            {
                AddMeshTextures(mesh, textures);
                meshCollection.Add(mesh);
            }
        }
    }

    private void DuplicateMeshTextures(TR1Level level, TRMesh mesh)
    {
        using TR1TexturePacker packer = new(level);
        packer.MaximumTiles = IsCommunityPatch ? 128 : 16;
        int maximumObjects = IsCommunityPatch ? 8192 : 2048;

        // Collect all texture pointers from each face in the mesh.
        IEnumerable<int> textures = mesh.TexturedRectangles.Select(f => (int)f.Texture);
        textures = textures.Concat(mesh.TexturedTriangles.Select(f => (int)f.Texture));
        Dictionary<TexturedTile, List<TexturedTileSegment>> segments = packer.GetObjectTextureSegments(textures.ToHashSet());

        // Clone each segment ready for packing.
        List<TexturedTileSegment> duplicates = new();
        foreach (List<TexturedTileSegment> segs in segments.Values)
        {
            duplicates.AddRange(segs.Select(s => s.Clone()));
        }

        // Pack the clones into the tiles.
        packer.AddRectangles(duplicates);
        packer.Pack(true);

        // Map the packed segments to object textures.
        List<TRObjectTexture> levelObjectTextures = level.ObjectTextures.ToList();
        Queue<int> reusableIndices = new(level.GetInvalidObjectTextureIndices());
        Dictionary<int, int> reindex = new();
        foreach (TexturedTileSegment segment in duplicates)
        {
            foreach (AbstractIndexedTRTexture texture in segment.Textures)
            {
                if (texture is not IndexedTRObjectTexture objTexture)
                {
                    continue;
                }

                int newIndex;
                if (reusableIndices.Count > 0)
                {
                    newIndex = reusableIndices.Dequeue();
                    levelObjectTextures[newIndex] = objTexture.Texture;
                }
                else if (levelObjectTextures.Count < maximumObjects)
                {
                    levelObjectTextures.Add(objTexture.Texture);
                    newIndex = levelObjectTextures.Count - 1;
                }
                else
                {
                    throw new PackingException(string.Format("Limit of {0} textures reached.", maximumObjects));
                }

                reindex[objTexture.Index] = newIndex;
            }
        }

        // Remap the mesh's faces.
        foreach (TRFace4 f in mesh.TexturedRectangles)
        {
            f.Texture = (ushort)reindex[f.Texture];
        }
        foreach (TRFace3 f in mesh.TexturedTriangles)
        {
            f.Texture = (ushort)reindex[f.Texture];
        }

        level.ObjectTextures = levelObjectTextures.ToArray();
        level.NumObjectTextures = (uint)levelObjectTextures.Count;
        level.ResetUnusedTextures();
    }

    private static void AddMeshTextures(TRMesh mesh, ISet<int> textures)
    {
        foreach (TRFace3 f in mesh.TexturedTriangles)
            textures.Add(f.Texture);
        foreach (TRFace4 f in mesh.TexturedRectangles)
            textures.Add(f.Texture);
    }

    private static void AddSpriteTextures(TR1Level level, TR1Type spriteID, ISet<int> textures)
    {
        TRSpriteSequence sequence = Array.Find(level.SpriteSequences, s => s.SpriteID == (int)spriteID);
        if (sequence != null)
        {
            for (int i = 0; i < sequence.NegativeLength * -1; i++)
            {
                textures.Add(sequence.Offset + i);
            }
        }
    }

    private static void AddSegmentsToMapping(Dictionary<TexturedTile, List<TexturedTileSegment>> segments, Dictionary<int, List<Rectangle>> mapping)
    {
        foreach (TexturedTile tile in segments.Keys)
        {
            if (!mapping.ContainsKey(tile.Index))
            {
                mapping[tile.Index] = new List<Rectangle>();
            }
            foreach (TexturedTileSegment segment in segments[tile])
            {
                mapping[tile.Index].Add(segment.Bounds);
            }
        }
    }
}
