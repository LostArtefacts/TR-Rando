using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Drawing;
using System.Linq;
using TRFDControl;
using TRFDControl.Utilities;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Packing;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Utilities;
using TRTexture16Importer.Textures;

namespace TRRandomizerCore.Textures
{
    public class DynamicTextureBuilder
    {
        // Models whose mesh textures should be targeted
        private static readonly List<TREntities> _modelIDs = new List<TREntities>
        {
            TREntities.Door1, TREntities.Door2, TREntities.Door3, TREntities.Door4,
            TREntities.Door5, TREntities.Door6, TREntities.Door7, TREntities.Door8,
            TREntities.Trapdoor1, TREntities.Trapdoor2, TREntities.Trapdoor3, TREntities.LiftingDoor,
            TREntities.WallSwitch, TREntities.UnderwaterSwitch, TREntities.DamoclesSword,
            TREntities.BridgeFlat, TREntities.BridgeTilt1, TREntities.BridgeTilt2,
            TREntities.SwingingBlade, TREntities.PushBlock1, TREntities.PushBlock2,
            TREntities.PushBlock3, TREntities.PushBlock4, TREntities.MovingBlock, TREntities.RollingBall,
            TREntities.FallingBlock, TREntities.FallingCeiling1, TREntities.FallingCeiling2,
            TREntities.DartEmitter, TREntities.Dart_H, TREntities.TeethSpikes, TREntities.Keyhole1,
            TREntities.Keyhole2, TREntities.Keyhole3, TREntities.Keyhole4, TREntities.PuzzleHole1,
            TREntities.PuzzleHole2, TREntities.PuzzleHole3, TREntities.PuzzleHole4,
            TREntities.PuzzleDone1,TREntities.PuzzleDone2,TREntities.PuzzleDone3,TREntities.PuzzleDone4,
            TREntities.Animating1, TREntities.Animating2, TREntities.Animating3,
            TREntities.Motorboat, TREntities.Barricade, TREntities.ThorHammerBlock, TREntities.ThorHammerHandle,
            TREntities.ThorLightning, TREntities.SlammingDoor, TREntities.CentaurStatue, TREntities.NatlasMineShack,
            TREntities.ScionHolder, TREntities.AtlanteanLava, TREntities.AdamEgg, TREntities.AtlanteanEgg,
            TREntities.ScionPiece3_S_P, TREntities.ScionPiece4_S_P
        };

        // Sprite sequences that should be targeted
        private static readonly List<TREntities> _spriteIDs = new List<TREntities>
        {
            TREntities.LavaParticles_S_H, TREntities.Flame_S_H, TREntities.Explosion1_S_H
        };

        public DynamicTextureTarget Build(TR1CombinedLevel level, bool retainMainTextures)
        {
            using (TR1TexturePacker packer = new TR1TexturePacker(level.Data))
            {
                ISet<int> defaultObjectTextures = new HashSet<int>();
                ISet<int> defaultSpriteTextures = new HashSet<int>();
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
                        defaultSpriteTextures.Add(sprite.Texture);
                }

                // Include all static mesh textures
                foreach (TRStaticMesh smesh in level.Data.StaticMeshes)
                {
                    TRMesh mesh = TRMeshUtilities.GetMesh(level.Data, smesh.Mesh);
                    AddMeshTextures(mesh, defaultObjectTextures);
                    if (!retainMainTextures)
                    {
                        modelMeshes.Add(mesh);
                    }
                }

                // Collect standard sprite sequences
                foreach (TREntities spriteID in _spriteIDs)
                {
                    AddSpriteTextures(level.Data, spriteID, defaultSpriteTextures);
                }

                TRMesh hips = null;
                List<TREntities> modelIDs = new List<TREntities>(_modelIDs);
                if (level.IsCutScene)
                {
                    // Cutscene actors vary between levels so we can't include all by default. These
                    // are the only ones we want to change.
                    if (level.Is(TRLevelNames.MINES_CUT))
                    {
                        modelIDs.Add(TREntities.CutsceneActor1); // ScionHolder
                        modelIDs.Add(TREntities.CutsceneActor3); // Scion
                    }
                    else if (level.Is(TRLevelNames.ATLANTIS_CUT))
                    {
                        modelIDs.Add(TREntities.CutsceneActor2); // ScionHolder
                        modelIDs.Add(TREntities.CutsceneActor4); // Scion
                    }
                }
                else
                {
                    hips = TRMeshUtilities.GetModelMeshes(level.Data, TREntities.Lara)[0];
                }

                // Collect all model mesh textures, provided none use the dummy mesh, otherwise
                // Lara will be partially re-textured.
                foreach (TREntities modelID in modelIDs)
                {
                    TRModel model = Array.Find(level.Data.Models, m => m.ID == (uint)modelID);
                    if (model != null)
                    {
                        AddModelTextures(level.Data, model, hips, defaultObjectTextures, modelMeshes);
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
                FDControl floorData = new FDControl();
                floorData.ParseFromLevel(level.Data);

                Dictionary<TREntities, TREntities> keyItems = TR1EntityUtilities.GetKeyItemMap();
                foreach (TREntities pickupType in keyItems.Keys)
                {
                    TRModel model = Array.Find(level.Data.Models, m => m.ID == (uint)keyItems[pickupType]);
                    if (model == null)
                    {
                        continue;
                    }

                    // Find an entity of this type and check if it's a secret
                    TREntity keyInstance = Array.Find(level.Data.Entities, e => e.TypeID == (short)pickupType);
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
                Dictionary<int, List<Rectangle>> defaultMapping = new Dictionary<int, List<Rectangle>>();
                AddSegmentsToMapping(packer.GetObjectTextureSegments(defaultObjectTextures), defaultMapping);
                AddSegmentsToMapping(packer.GetSpriteTextureSegments(defaultSpriteTextures), defaultMapping);

                Dictionary<TextureCategory, Dictionary<int, List<Rectangle>>> optionalMapping = new Dictionary<TextureCategory, Dictionary<int, List<Rectangle>>>
                {
                    [TextureCategory.Secret] = new Dictionary<int, List<Rectangle>>(),
                    [TextureCategory.KeyItem] = new Dictionary<int, List<Rectangle>>()
                };
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
        }

        private void AddModelTextures(TRLevel level, TRModel model, TRMesh dummyMesh, ISet<int> textures, ISet<TRMesh> meshCollection)
        {
            if ((TREntities)model.ID == TREntities.CentaurStatue && Array.Find(level.Entities, e => e.TypeID == model.ID) == null)
            {
                // Can happen in ToT if the centaur statue was "removed", in which case we don't want to
                // change any object textures that were repurposed for new enemies.
                return;
            }

            TRMesh[] meshes = TRMeshUtilities.GetModelMeshes(level, model);
            foreach (TRMesh mesh in meshes)
            {
                if (mesh != dummyMesh)
                {
                    AddMeshTextures(mesh, textures);
                    meshCollection.Add(mesh);
                }
            }
        }

        private void AddMeshTextures(TRMesh mesh, ISet<int> textures)
        {
            foreach (TRFace3 f in mesh.TexturedTriangles)
                textures.Add(f.Texture);
            foreach (TRFace4 f in mesh.TexturedRectangles)
                textures.Add(f.Texture);
        }

        private void AddSpriteTextures(TRLevel level, TREntities spriteID, ISet<int> textures)
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

        private void AddSegmentsToMapping(Dictionary<TexturedTile, List<TexturedTileSegment>> segments, Dictionary<int, List<Rectangle>> mapping)
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
}