using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using TRFDControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRLevelControl.Model.Enums;
using TRModelTransporter.Helpers;
using TRModelTransporter.Model.Textures;
using TRModelTransporter.Packing;
using TRRandomizerCore.Utilities;
using TRTexture16Importer.Helpers;

namespace TRRandomizerCore.Textures;

public class TR1Wireframer : AbstractTRWireframer<TREntities, TR1Level>
{
    private static readonly List<TREntities> _laraEntities = new List<TREntities>
    {
        TREntities.Lara, TREntities.LaraPonytail_H_U, TREntities.CutsceneActor1,
        TREntities.LaraPistolAnim_H, TREntities.LaraShotgunAnim_H, TREntities.LaraMagnumAnim_H,
        TREntities.LaraUziAnimation_H, TREntities.LaraMiscAnim_H, TREntities.CameraTarget_N,
        TREntities.FlameEmitter_N, TREntities.NonShootingAtlantean_N, TREntities.ShootingAtlantean_N,
        TREntities.MidasHand_N
    };

    private static readonly List<TREntities> _enemyPlaceholderEntities = new List<TREntities>
    {
        TREntities.NonShootingAtlantean_N, TREntities.ShootingAtlantean_N
    };

    private static readonly List<TREntities> _additionalEnemyEntities = new List<TREntities>
    {
        TREntities.Missile1_H, TREntities.Missile2_H, TREntities.Missile3_H,
        TREntities.CutsceneActor2, TREntities.CutsceneActor3, TREntities.CutsceneActor4,
        TREntities.AdamEgg, TREntities.ScionHolder, TREntities.ScionPiece3_S_P, TREntities.ScionPiece4_S_P,
        TREntities.Skateboard
    };

    private static readonly List<TREntities> _pickupModels = new List<TREntities>
    {
        TREntities.Pistols_M_H, TREntities.Shotgun_M_H, TREntities.Magnums_M_H, TREntities.Uzis_M_H,
        TREntities.ShotgunAmmo_M_H, TREntities.MagnumAmmo_M_H, TREntities.UziAmmo_M_H,
        TREntities.SmallMed_M_H, TREntities.LargeMed_M_H,
        TREntities.Puzzle1_M_H, TREntities.Puzzle2_M_H, TREntities.Puzzle3_M_H, TREntities.Puzzle4_M_H,
        TREntities.Key1_M_H, TREntities.Key2_M_H, TREntities.Key3_M_H, TREntities.Key4_M_H,
        TREntities.Quest1_M_H, TREntities.Quest2_M_H,
        TREntities.ScionPiece_M_H
    };

    public override bool Is8BitPalette => true;

    private TR1TexturePacker _packer;

    protected override AbstractTexturePacker<TREntities, TR1Level> CreatePacker(TR1Level level)
    {
        return _packer = new TR1TexturePacker(level);
    }

    protected override bool IsSkybox(TRModel model)
    {
        return false;
    }

    protected override bool IsInteractableModel(TRModel model)
    {
        TREntities type = (TREntities)model.ID;
        return TR1EntityUtilities.IsSwitchType(type)
            || TR1EntityUtilities.IsKeyholeType(type)
            || TR1EntityUtilities.IsSlotType(type)
            || TR1EntityUtilities.IsPushblockType(type)
            || type == TREntities.Barricade;
    }

    protected override bool ShouldSolidifyModel(TRModel model)
    {
        return _data.Has3DPickups && _pickupModels.Contains((TREntities)model.ID);
    }

    protected override int GetBlackPaletteIndex(TR1Level level)
    {
        return ImportColour(level, Color.Black);
    }

    protected override IEnumerable<int> GetInvalidObjectTextureIndices(TR1Level level)
    {
        return level.GetInvalidObjectTextureIndices();
    }

    protected override TRMesh[] GetLevelMeshes(TR1Level level)
    {
        return level.Meshes;
    }

    protected override Dictionary<TREntities, TRMesh[]> GetModelMeshes(TR1Level level)
    {
        Dictionary<TREntities, TRMesh[]> modelMeshes = new Dictionary<TREntities, TRMesh[]>();
        foreach (TRModel model in level.Models)
        {
            TRMesh[] meshes = GetModelMeshes(level, model);
            if (meshes != null)
            {
                modelMeshes[(TREntities)model.ID] = meshes;
            }
        }
        return modelMeshes;
    }

    protected override TRMesh[] GetModelMeshes(TR1Level level, TRModel model)
    {
        return TRMeshUtilities.GetModelMeshes(level, model);
    }

    protected override TRModel[] GetModels(TR1Level level)
    {
        return level.Models;
    }

    protected override TRObjectTexture[] GetObjectTextures(TR1Level level)
    {
        return level.ObjectTextures;
    }

    protected override IEnumerable<IEnumerable<TRFace3>> GetRoomFace3s(TR1Level level)
    {
        List<List<TRFace3>> faces = new List<List<TRFace3>>();
        foreach (TRRoom room in level.Rooms)
        {
            faces.Add(room.RoomData.Triangles.ToList());
        }
        return faces;
    }

    protected override IEnumerable<IEnumerable<TRFace4>> GetRoomFace4s(TR1Level level)
    {
        List<List<TRFace4>> faces = new List<List<TRFace4>>();
        foreach (TRRoom room in level.Rooms)
        {
            faces.Add(room.RoomData.Rectangles.ToList());
        }
        return faces;
    }

    protected override TRMesh GetStaticMesh(TR1Level level, TRStaticMesh staticMesh)
    {
        return TRMeshUtilities.GetMesh(level, staticMesh.Mesh);
    }

    protected override TRStaticMesh[] GetStaticMeshes(TR1Level level)
    {
        return level.StaticMeshes;
    }

    protected override int ImportColour(TR1Level level, Color c)
    {
        if (_packer.PaletteManager == null)
        {
            _packer.PaletteManager = new TR1PaletteManager();
        }
        return _packer.PaletteManager.AddPredefinedColour(c);
    }

    protected override bool IsLaraModel(TRModel model)
    {
        return _laraEntities.Contains((TREntities)model.ID);
    }

    protected override bool IsEnemyModel(TRModel model)
    {
        TREntities id = (TREntities)model.ID;
        return TR1EntityUtilities.IsEnemyType(id) || _additionalEnemyEntities.Contains(id);
    }

    protected override bool IsEnemyPlaceholderModel(TRModel model)
    {
        TREntities id = (TREntities)model.ID;
        return _enemyPlaceholderEntities.Contains(id);
    }

    protected override void ResetPaletteTracking(TR1Level level)
    {
        if (_packer.PaletteManager != null)
        {
            _packer.PaletteManager.MergePredefinedColours();
        }
    }

    protected override void ResetUnusedTextures(TR1Level level)
    {
        level.ResetUnusedTextures();
    }

    protected override void SetObjectTextures(TR1Level level, IEnumerable<TRObjectTexture> textures)
    {
        level.ObjectTextures = textures.ToArray();
        level.NumObjectTextures = (uint)level.ObjectTextures.Length;
    }

    protected override void SetSkyboxVisible(TR1Level level) { }

    protected override Dictionary<TRFace4, List<TRVertex>> CollectLadders(TR1Level level)
    {
        return new Dictionary<TRFace4, List<TRVertex>>();
    }

    protected override List<TRFace4> CollectTriggerFaces(TR1Level level, List<FDTrigType> triggerTypes)
    {
        return FaceUtilities.GetTriggerFaces(level, triggerTypes, false);
    }

    protected override List<TRFace4> CollectDeathFaces(TR1Level level)
    {
        return FaceUtilities.GetTriggerFaces(level, new List<FDTrigType>(), true);
    }

    protected override TRAnimatedTexture[] GetAnimatedTextures(TR1Level level)
    {
        return level.AnimatedTextures;
    }

    protected override void SetAnimatedTextures(TR1Level level, TRAnimatedTexture[] animatedTextures, ushort length)
    {
        level.AnimatedTextures = animatedTextures;
        level.NumAnimatedTextures = length;
    }

    protected override Dictionary<ushort, TexturedTileSegment> CreateSpecialSegments(TR1Level level, Pen pen)
    {
        Dictionary<ushort, TexturedTileSegment> segments = new Dictionary<ushort, TexturedTileSegment>();
        foreach (SpecialTextureHandling special in _data.SpecialTextures)
        {
            switch (special.Type)
            {
                case SpecialTextureType.MidasDoors:
                    foreach (ushort texture in special.Textures)
                    {
                        if (CreateMidasDoor(level, pen, texture, special.Mode) is TexturedTileSegment segment)
                        {
                            segments[texture] = segment;
                        }
                    }
                    break;
            }
        }

        return segments;
    }

    private TexturedTileSegment CreateMidasDoor(TR1Level level, Pen pen, ushort textureIndex, SpecialTextureMode mode)
    {
        TRModel doorModel = FindDoorModel(level, textureIndex);
        if (doorModel == null)
        {
            return null;
        }

        TREntity doorInstance = Array.Find(level.Entities, e => e.TypeID == doorModel.ID);
        if (doorInstance == null)
        {
            return null;
        }

        const int width = 64;
        const int height = 16;
                    
        IndexedTRObjectTexture texture = CreateTexture(new Rectangle(0, 0, width, height));
        BitmapGraphics frame = CreateFrame(width, height, pen, SmoothingMode.AntiAlias, false);

        int flags = (doorInstance.Flags & 0x3E00) >> 9;
        for (int i = 0; i < 5; i++)
        {
            int x = 3 + i * 12;
            int y = 3;
            int w = 10;
            int h = 10;

            // Make a smaller rectangle
            frame.Graphics.DrawRectangle(pen, x, y, w - 1, h - 1);

            // Decorate based on the door's bits
            bool doorBitSet = (flags & (1 << i)) == 0;
            switch (mode)
            {
                case SpecialTextureMode.MidasDoorBars:
                    // Bar at top = lever up; at bottom = lever down
                    y += doorBitSet ? 5 : 1;
                    frame.Graphics.DrawLine(pen, x + 4, y, x + 4, y + 3);
                    frame.Graphics.DrawLine(pen, x + 5, y, x + 5, y + 3);
                    break;
                case SpecialTextureMode.MidasDoorFill:
                    // Empty blocks need "filled" - levers go down
                    if (!doorBitSet)
                    {
                        frame.Graphics.FillRectangle(pen.Brush, x, y, w - 1, h - 1);
                    }
                    break;
                case SpecialTextureMode.MidasDoorLines:
                    if (doorBitSet)
                    {
                        // Lever up
                        frame.Graphics.DrawLine(pen, x + 1, y + 4, x + 8, y + 4);
                        frame.Graphics.DrawLine(pen, x + 1, y + 5, x + 8, y + 5);
                    }
                    else
                    {
                        // Lever down
                        frame.Graphics.DrawLine(pen, x + 4, y + 1, x + 4, y + 8);
                        frame.Graphics.DrawLine(pen, x + 5, y + 1, x + 5, y + 8);
                    }
                    break;
                case SpecialTextureMode.MidasDoorDiagonals:
                    if (doorBitSet)
                    {
                        // Lever up \
                        frame.Graphics.DrawLine(pen, x + 1, y + 1, x + 8, y + 8);
                    }
                    else
                    {
                        // Lever down /
                        frame.Graphics.DrawLine(pen, x + 1, y + 8, x + 8, y + 1);
                    }
                    break;
            }
        }

        return new TexturedTileSegment(texture, frame.Bitmap);
    }

    private TRModel FindDoorModel(TR1Level level, ushort textureIndex)
    {
        foreach (TRModel model in level.Models)
        {
            TREntities type = (TREntities)model.ID;
            if (!TR1EntityUtilities.IsDoorType(type))
            {
                continue;
            }

            foreach (TRMesh mesh in TRMeshUtilities.GetModelMeshes(level, type))
            {
                if (mesh.TexturedRectangles.Any(f => f.Texture == textureIndex))
                {
                    return model;
                }
            }
        }

        return null;
    }
}
