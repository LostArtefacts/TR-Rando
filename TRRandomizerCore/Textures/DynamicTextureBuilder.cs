using System.Drawing;
using TRDataControl;
using TRImageControl.Packing;
using TRImageControl.Textures;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Utilities;

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
        HashSet<int> defaultObjectTextures = new();
        HashSet<int> enemyObjectTextures = new();
        HashSet<int> secretObjectTextures = new();
        HashSet<int> keyItemObjectTextures = new();
        

        HashSet<TRSpriteSequence> defaultSprites = new();
        HashSet<TRSpriteSequence> secretSprites = new();
        HashSet<TRSpriteSequence> keyItemSprites = new();

        HashSet<TRMesh> modelMeshes = new();

        // Collect unique room and room sprite textures
        List<TR1Type> roomSprites = new();
        foreach (TR1Room room in level.Data.Rooms)
        {
            foreach (TRFace face in room.Mesh.Faces)
            {
                defaultObjectTextures.Add(face.Texture);
            }

            foreach (TRRoomSprite<TR1Type> sprite in room.Mesh.Sprites)
            {
                // Only add ones that aren't also pickups
                if (!level.Data.Entities.Any(e => e.TypeID == sprite.ID))
                {
                    roomSprites.Add(sprite.ID);
                }
            }
        }

        // Include all static mesh textures
        foreach (TRStaticMesh staticMesh in level.Data.StaticMeshes.Values)
        {
            AddMeshTextures(staticMesh.Mesh, defaultObjectTextures);
            if (!RetainMainTextures)
            {
                modelMeshes.Add(staticMesh.Mesh);
            }
        }

        // Collect standard sprite sequences
        foreach (TR1Type spriteID in _spriteIDs.Concat(roomSprites).Distinct())
        {
            AddSpriteTextures(level.Data, spriteID, defaultSprites);
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
            hips = level.Data.Models[TR1Type.Lara].Meshes[0];
            if (level.Data.Entities.Any(e => e.TypeID == TR1Type.MidasHand_N))
            {
                modelIDs.Add(TR1Type.LaraMiscAnim_H);
                modelIDs.Add(TR1Type.LaraPonytail_H_U);
            }
        }

        // Collect all model mesh textures, provided none use the dummy mesh, otherwise
        // Lara will be partially re-textured.
        foreach (TR1Type modelID in modelIDs)
        {
            TRModel model = level.Data.Models[modelID];
            if (model != null)
            {
                AddModelTextures(level.Data, modelID, model, hips, defaultObjectTextures, modelMeshes);
            }
        }

        foreach (TR1Type modelID in _enemyIDs)
        {
            TRModel model = level.Data.Models[modelID];
            if (model != null)
            {
                AddModelTextures(level.Data, modelID, model, hips, enemyObjectTextures, modelMeshes);
            }
        }

        // If anything we have collected so far is an animated texture, add the other
        // textures from the same animation list.
        foreach (int texture in defaultObjectTextures.ToList())
        {
            TRAnimatedTexture anim = level.Data.AnimatedTextures.Find(a => a.Textures.Contains((ushort)texture));
            if (anim != null)
            {
                foreach (ushort animTexture in anim.Textures)
                {
                    defaultObjectTextures.Add(animTexture);
                }
            }
        }

        // Key items and secrets
        Dictionary<TR1Type, TR1Type> keyItems = TR1TypeUtilities.GetKeyItemMap();
        foreach (TR1Type pickupType in keyItems.Keys)
        {
            TRModel model = level.Data.Models[keyItems[pickupType]];
            if (model == null)
            {
                continue;
            }

            // Find an entity of this type and check if it's a secret
            TR1Entity keyInstance = level.Data.Entities.Find(e => e.TypeID == pickupType);
            if (keyInstance != null)
            {
                TRRoomSector sector = level.Data.GetRoomSector(keyInstance);
                if (LocationUtilities.SectorContainsSecret(sector, level.Data.FloorData))
                {
                    AddModelTextures(level.Data, pickupType, model, hips, secretObjectTextures, modelMeshes);
                    AddSpriteTextures(level.Data, pickupType, secretSprites);
                    continue;
                }
            }
                
            // Otherwise it's a regular key item
            AddModelTextures(level.Data, pickupType, model, hips, keyItemObjectTextures, modelMeshes);
            AddSpriteTextures(level.Data, pickupType, keyItemSprites);
        }

        // Put it all together into the required format for texture re-mapping.
        TR1TexturePacker packer = new(level.Data);
        Dictionary<int, List<Rectangle>> defaultMapping = new();
        AddSegmentsToMapping(packer.GetObjectRegions(defaultObjectTextures), defaultMapping);
        AddSegmentsToMapping(packer.GetSpriteRegions(defaultSprites), defaultMapping);

        Dictionary<TextureCategory, Dictionary<int, List<Rectangle>>> optionalMapping = new()
        {
            [TextureCategory.Enemy] = new Dictionary<int, List<Rectangle>>(),
            [TextureCategory.Secret] = new Dictionary<int, List<Rectangle>>(),
            [TextureCategory.KeyItem] = new Dictionary<int, List<Rectangle>>()
        };
        AddSegmentsToMapping(packer.GetObjectRegions(enemyObjectTextures), optionalMapping[TextureCategory.Enemy]);
        AddSegmentsToMapping(packer.GetObjectRegions(secretObjectTextures), optionalMapping[TextureCategory.Secret]);
        AddSegmentsToMapping(packer.GetSpriteRegions(secretSprites), optionalMapping[TextureCategory.Secret]);
        AddSegmentsToMapping(packer.GetObjectRegions(keyItemObjectTextures), optionalMapping[TextureCategory.KeyItem]);
        AddSegmentsToMapping(packer.GetSpriteRegions(keyItemSprites), optionalMapping[TextureCategory.KeyItem]);

        return new DynamicTextureTarget
        {
            DefaultTileTargets = defaultMapping,
            OptionalTileTargets = optionalMapping,
            ModelColourTargets = modelMeshes.ToList()
        };
    }

    private void AddModelTextures(TR1Level level, TR1Type modelID, TRModel model, TRMesh dummyMesh, ISet<int> textures, ISet<TRMesh> meshCollection)
    {
        if (TextureMonitor?.RemovedTextures?.Contains(modelID) ?? false)
        {
            // Don't include textures that may have been re-assigned to other imported models (e.g. enemies).
            return;
        }

        if (modelID == TR1Type.CentaurStatue && !level.Entities.Any(e => e.TypeID == modelID))
        {
            // Can happen in ToT if the centaur statue was "removed", in which case we don't want to
            // change any object textures that were repurposed for new enemies.
            return;
        }

        if (modelID == TR1Type.Cowboy && level.Models[TR1Type.Cowboy].Meshes[2].TexturedRectangles.Count > 0)
        {
            // We only want to target LeoC's headless cowboy - in this case the cowboy is OG.
            return;
        }

        List<TRMesh> excludedMeshes = new() { dummyMesh };

        if (modelID == TR1Type.Adam)
        {                
            TR1Blob adam = new TR1DataImporter
            {
                DataFolder = @"Resources\TR1\Models"
            }.LoadBlob(modelID);

            if (model.Meshes[3].CollRadius != adam.Model.Meshes[3].CollRadius)
            {
                try
                {
                    // Adam's head may have been replaced by Lara's, Pierre's etc. Try to
                    // create duplicates of the mesh's textures so we don't corrupt the
                    // original model.
                    DuplicateMeshTextures(level, model.Meshes[3]);
                }
                catch
                {
                    // If packing failed, just exclude this particular mesh.
                    excludedMeshes.Add(model.Meshes[3]);
                }
            }
        }
        else if ((modelID == TR1Type.ScionPiece3_S_P || modelID == TR1Type.ScionPiece4_S_P)
            && model.Meshes.Count == 1 && model.Meshes[0].Normals.Count != 123)
        {
            try
            {
                // The scion is something else so try to duplicate it to avoid original
                // model issues.
                DuplicateMeshTextures(level, model.Meshes[0]);
            }
            catch
            {
                // If packing failed, just exclude this particular mesh.
                excludedMeshes.Add(model.Meshes[0]);
            }
        }
        else if (modelID == TR1Type.LaraPonytail_H_U)
        {
            // For Midas "golden" hair, we only want the additional meshes that may have
            // been created by outfit rando.
            for (int i = 0; i < 6; i++)
            {
                excludedMeshes.Add(model.Meshes[i]);
            }
        }

        foreach (TRMesh mesh in model.Meshes)
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
        TR1TexturePacker packer = new(level)
        {
            MaximumTiles = IsCommunityPatch ? 128 : 16
        };
        int maximumObjects = IsCommunityPatch ? 8192 : 2048;

        // Collect all texture pointers from each face in the mesh.
        IEnumerable<int> textures = mesh.TexturedRectangles.Select(f => (int)f.Texture);
        textures = textures.Concat(mesh.TexturedTriangles.Select(f => (int)f.Texture));
        Dictionary<TRTextile, List<TRTextileRegion>> segments = packer.GetObjectRegions(textures.ToHashSet());

        // Clone each segment ready for packing.
        List<TRTextileRegion> duplicates = new();
        foreach (List<TRTextileRegion> segs in segments.Values)
        {
            duplicates.AddRange(segs.Select(s => s.Clone()));
        }

        // Pack the clones into the tiles.
        packer.AddRectangles(duplicates);
        packer.Pack(true);

        // Map the packed segments to object textures.
        Dictionary<int, int> reindex = new();
        foreach (TRTextileSegment segment in duplicates.SelectMany(r => r.Segments))
        {
            if (segment.Texture is not TRObjectTexture objTexture)
            {
                continue;
            }

            if (level.ObjectTextures.Count >= maximumObjects)
            {
                throw new PackingException($"Limit of {maximumObjects} textures reached.");
            }

            reindex[segment.Index] = level.ObjectTextures.Count;
            level.ObjectTextures.Add(objTexture);
        }

        // Remap the mesh's faces.
        foreach (TRMeshFace face in mesh.TexturedFaces)
        {
            face.Texture = (ushort)reindex[face.Texture];
        }

        level.ResetUnusedTextures();
    }

    private static void AddMeshTextures(TRMesh mesh, ISet<int> textures)
    {
        foreach (TRMeshFace face in mesh.TexturedFaces)
        {
            textures.Add(face.Texture);
        }
    }

    private static void AddSpriteTextures(TR1Level level, TR1Type spriteID, ISet<TRSpriteSequence> sprites)
    {
        if (level.Sprites.ContainsKey(spriteID))
        {
            sprites.Add(level.Sprites[spriteID]);
        }
    }

    private static void AddSegmentsToMapping(Dictionary<TRTextile, List<TRTextileRegion>> segments, Dictionary<int, List<Rectangle>> mapping)
    {
        foreach (TRTextile tile in segments.Keys)
        {
            if (!mapping.ContainsKey(tile.Index))
            {
                mapping[tile.Index] = new List<Rectangle>();
            }
            foreach (TRTextileRegion segment in segments[tile])
            {
                mapping[tile.Index].Add(segment.Bounds);
            }
        }
    }
}
