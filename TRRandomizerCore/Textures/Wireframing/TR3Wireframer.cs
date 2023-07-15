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
using TRTexture16Importer;
using TRTexture16Importer.Helpers;

namespace TRRandomizerCore.Textures
{
    public class TR3Wireframer : AbstractTRWireframer<TR3Entities, TR3Level>
    {
        private static readonly List<TR3Entities> _laraEntities = new List<TR3Entities>
        {
            TR3Entities.Lara, TR3Entities.LaraPonytail_H, TR3Entities.LaraFlareAnimation_H,
            TR3Entities.LaraPistolAnimation_H, TR3Entities.LaraShotgunAnimation_H, TR3Entities.LaraUziAnimation_H,
            TR3Entities.LaraDeagleAnimation_H, TR3Entities.LaraMP5Animation_H, TR3Entities.LaraGrenadeAnimation_H,
            TR3Entities.LaraRocketAnimation_H, TR3Entities.LaraExtraAnimation_H, TR3Entities.LaraSkin_H,
            TR3Entities.LaraHarpoonAnimation_H, TR3Entities.LaraVehicleAnimation_H
        };

        private static readonly List<TR3Entities> _additionalEnemyEntities = new List<TR3Entities>
        {
            TR3Entities.ShivaStatue, TR3Entities.MonkeyKeyMeshswap, TR3Entities.MonkeyMedMeshswap
        };

        protected override AbstractTexturePacker<TR3Entities, TR3Level> CreatePacker(TR3Level level)
        {
            return new TR3TexturePacker(level);
        }

        protected override bool IsInteractableModel(TRModel model)
        {
            TR3Entities type = (TR3Entities)model.ID;
            return TR3EntityUtilities.IsSwitchType(type)
                || TR3EntityUtilities.IsKeyholeType(type)
                || TR3EntityUtilities.IsSlotType(type)
                || TR3EntityUtilities.IsPushblockType(type);
        }

        protected override int GetBlackPaletteIndex(TR3Level level)
        {
            return level.Palette16.ToList().FindIndex(c => c.Red + c.Green + c.Blue == 0);
        }

        protected override IEnumerable<int> GetInvalidObjectTextureIndices(TR3Level level)
        {
            return level.GetInvalidObjectTextureIndices();
        }

        protected override TRMesh[] GetLevelMeshes(TR3Level level)
        {
            return level.Meshes;
        }

        protected override Dictionary<TR3Entities, TRMesh[]> GetModelMeshes(TR3Level level)
        {
            Dictionary<TR3Entities, TRMesh[]> modelMeshes = new Dictionary<TR3Entities, TRMesh[]>();
            foreach (TRModel model in level.Models)
            {
                TRMesh[] meshes = GetModelMeshes(level, model);
                if (meshes != null)
                {
                    modelMeshes[(TR3Entities)model.ID] = meshes;
                }
            }
            return modelMeshes;
        }

        protected override TRMesh[] GetModelMeshes(TR3Level level, TRModel model)
        {
            return TRMeshUtilities.GetModelMeshes(level, model);
        }

        protected override TRModel[] GetModels(TR3Level level)
        {
            return level.Models;
        }

        protected override TRObjectTexture[] GetObjectTextures(TR3Level level)
        {
            return level.ObjectTextures;
        }

        protected override IEnumerable<IEnumerable<TRFace3>> GetRoomFace3s(TR3Level level)
        {
            List<List<TRFace3>> faces = new List<List<TRFace3>>();
            foreach (TR3Room room in level.Rooms)
            {
                faces.Add(room.RoomData.Triangles.ToList());
            }
            return faces;
        }

        protected override IEnumerable<IEnumerable<TRFace4>> GetRoomFace4s(TR3Level level)
        {
            List<List<TRFace4>> faces = new List<List<TRFace4>>();
            foreach (TR3Room room in level.Rooms)
            {
                faces.Add(room.RoomData.Rectangles.ToList());
            }
            return faces;
        }

        protected override TRMesh GetStaticMesh(TR3Level level, TRStaticMesh staticMesh)
        {
            return TRMeshUtilities.GetMesh(level, staticMesh.Mesh);
        }

        protected override TRStaticMesh[] GetStaticMeshes(TR3Level level)
        {
            return level.StaticMeshes;
        }

        protected override int ImportColour(TR3Level level, Color c)
        {
            int index = level.Palette16.ToList().FindIndex(col => col.Red == c.R && col.Green == c.G && col.Blue == c.B);
            return index == -1 ? PaletteUtilities.Import(level, c) : index;
        }

        protected override bool IsLaraModel(TRModel model)
        {
            return _laraEntities.Contains((TR3Entities)model.ID);
        }

        protected override bool IsEnemyModel(TRModel model)
        {
            TR3Entities id = (TR3Entities)model.ID;
            return TR3EntityUtilities.IsEnemyType(id) || _additionalEnemyEntities.Contains(id);
        }

        protected override bool IsSkybox(TRModel model)
        {
            return (TR3Entities)model.ID == TR3Entities.Skybox_H;
        }

        protected override bool ShouldSolidifyModel(TRModel model)
        {
            TR3Entities type = (TR3Entities)model.ID;
            return TR3EntityUtilities.IsAnyPickupType(type) || TR3EntityUtilities.IsCrystalPickup(type);
        }

        protected override void ResetPaletteTracking(TR3Level level)
        {
            PaletteUtilities.ResetPaletteTracking(level.Palette16);
        }

        protected override void ResetUnusedTextures(TR3Level level)
        {
            level.ResetUnusedTextures();
        }

        protected override void SetObjectTextures(TR3Level level, IEnumerable<TRObjectTexture> textures)
        {
            level.ObjectTextures = textures.ToArray();
            level.NumObjectTextures = (uint)level.ObjectTextures.Length;
        }

        protected override void SetSkyboxVisible(TR3Level level)
        {
            foreach (TR3Room room in level.Rooms)
            {
                room.IsSkyboxVisible = true;
            }
        }

        protected override Dictionary<TRFace4, List<TRVertex>> CollectLadders(TR3Level level)
        {
            return FaceUtilities.GetClimbableFaces(level);
        }

        protected override List<TRFace4> CollectTriggerFaces(TR3Level level, List<FDTrigType> triggerTypes)
        {
            return FaceUtilities.GetTriggerFaces(level, triggerTypes, false);
        }

        protected override List<TRFace4> CollectDeathFaces(TR3Level level)
        {
            return FaceUtilities.GetTriggerFaces(level, new List<FDTrigType>(), true);
        }

        protected override TRAnimatedTexture[] GetAnimatedTextures(TR3Level level)
        {
            return level.AnimatedTextures;
        }

        protected override void SetAnimatedTextures(TR3Level level, TRAnimatedTexture[] animatedTextures, ushort length)
        {
            level.AnimatedTextures = animatedTextures;
            level.NumAnimatedTextures = length;
        }

        protected override Dictionary<ushort, TexturedTileSegment> CreateSpecialSegments(TR3Level level, Pen pen)
        {
            Dictionary<ushort, TexturedTileSegment> segments = new Dictionary<ushort, TexturedTileSegment>();
            foreach (SpecialTextureHandling special in _data.SpecialTextures)
            {
                switch (special.Type)
                {
                    case SpecialTextureType.CrashPads:
                        foreach (ushort texture in special.Textures)
                        {
                            if (CreateCrashPad(level, pen, texture, special.Mode) is TexturedTileSegment segment)
                            {
                                segments[texture] = segment;
                            }
                        }
                        break;
                }
            }

            return segments;
        }

        private TexturedTileSegment CreateCrashPad(TR3Level level, Pen pen, ushort textureIndex, SpecialTextureMode mode)
        {
            const int width = 64;
            const int height = 64;

            IndexedTRObjectTexture texture = CreateTexture(new Rectangle(0, 0, width, height));
            BitmapGraphics frame = CreateFrame(width, height, pen, SmoothingMode.AntiAlias, true);

            switch (mode)
            {
                case SpecialTextureMode.CrashPadCircle:
                    frame.Graphics.FillEllipse(pen.Brush, new Rectangle(16, 16, 46, 46));
                    break;
                case SpecialTextureMode.CrashPadDiamond:
                    frame.Graphics.FillPolygon(pen.Brush, new Point[]
                    {
                        new Point(32, 16),
                        new Point(48, 32),
                        new Point(32, 48),
                        new Point(16, 32),
                    });
                    break;
            }

            return new TexturedTileSegment(texture, frame.Bitmap);
        }
    }
}