using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Helpers;
using TRModelTransporter.Packing;
using TRRandomizerCore.Utilities;
using TRTexture16Importer;

namespace TRRandomizerCore.Textures
{
    public class TR2Wireframer : AbstractTRWireframer<TR2Entities, TR2Level>
    {
        private static readonly List<TR2Entities> _laraEntities = new List<TR2Entities>
        {
            TR2Entities.Lara, TR2Entities.LaraPonytail_H, TR2Entities.LaraFlareAnim_H,
            TR2Entities.LaraPistolAnim_H, TR2Entities.LaraShotgunAnim_H, TR2Entities.LaraAutoAnim_H,
            TR2Entities.LaraUziAnim_H, TR2Entities.LaraM16Anim_H, TR2Entities.LaraHarpoonAnim_H,
            TR2Entities.LaraGrenadeAnim_H, TR2Entities.LaraMiscAnim_H, TR2Entities.CameraTarget_N,
            TR2Entities.FlameEmitter_N, TR2Entities.LaraCutscenePlacement_N, TR2Entities.DragonExplosionEmitter_N,
            TR2Entities.BartoliHideoutClock_N, TR2Entities.SingingBirds_N, TR2Entities.WaterfallMist_N,
            TR2Entities.DrippingWater_N, TR2Entities.LavaAirParticleEmitter_N, TR2Entities.AlarmBell_N, TR2Entities.DoorBell_N
        };

        private static readonly List<TR2Entities> _additionalEnemyEntities = new List<TR2Entities>
        {
            TR2Entities.DragonFront_H, TR2Entities.DragonBack_H, TR2Entities.XianGuardSpearStatue, TR2Entities.XianGuardSwordStatue
        };

        protected override AbstractTexturePacker<TR2Entities, TR2Level> CreatePacker(TR2Level level)
        {
            return new TR2TexturePacker(level);
        }

        protected override bool IsSkybox(TRModel model)
        {
            return (TR2Entities)model.ID == TR2Entities.Skybox_H;
        }

        protected override int GetBlackPaletteIndex(TR2Level level)
        {
            return level.Palette16.ToList().FindIndex(c => c.Red + c.Green + c.Blue == 0);
        }

        protected override IEnumerable<int> GetInvalidObjectTextureIndices(TR2Level level)
        {
            return level.GetInvalidObjectTextureIndices();
        }

        protected override TRMesh[] GetLevelMeshes(TR2Level level)
        {
            return level.Meshes;
        }

        protected override Dictionary<TR2Entities, TRMesh[]> GetModelMeshes(TR2Level level)
        {
            Dictionary<TR2Entities, TRMesh[]> modelMeshes = new Dictionary<TR2Entities, TRMesh[]>();
            foreach (TRModel model in level.Models)
            {
                TRMesh[] meshes = GetModelMeshes(level, model);
                if (meshes != null)
                {
                    modelMeshes[(TR2Entities)model.ID] = meshes;
                }
            }
            return modelMeshes;
        }

        protected override TRMesh[] GetModelMeshes(TR2Level level, TRModel model)
        {
            return TRMeshUtilities.GetModelMeshes(level, model);
        }

        protected override TRModel[] GetModels(TR2Level level)
        {
            return level.Models;
        }

        protected override TRObjectTexture[] GetObjectTextures(TR2Level level)
        {
            return level.ObjectTextures;
        }

        protected override IEnumerable<IEnumerable<TRFace3>> GetRoomFace3s(TR2Level level)
        {
            List<List<TRFace3>> faces = new List<List<TRFace3>>();
            foreach (TR2Room room in level.Rooms)
            {
                faces.Add(room.RoomData.Triangles.ToList());
            }
            return faces;
        }

        protected override IEnumerable<IEnumerable<TRFace4>> GetRoomFace4s(TR2Level level)
        {
            List<List<TRFace4>> faces = new List<List<TRFace4>>();
            foreach (TR2Room room in level.Rooms)
            {
                faces.Add(room.RoomData.Rectangles.ToList());
            }
            return faces;
        }

        protected override TRMesh GetStaticMesh(TR2Level level, TRStaticMesh staticMesh)
        {
            return TRMeshUtilities.GetMesh(level, staticMesh.Mesh);
        }

        protected override TRStaticMesh[] GetStaticMeshes(TR2Level level)
        {
            return level.StaticMeshes;
        }

        protected override int ImportColour(TR2Level level, Color c)
        {
            int index = level.Palette16.ToList().FindIndex(col => col.Red == c.R && col.Green == c.G && col.Blue == c.B);
            return index == -1 ? PaletteUtilities.Import(level, c) : index;
        }

        protected override bool IsLaraModel(TRModel model)
        {
            return _laraEntities.Contains((TR2Entities)model.ID);
        }

        protected override bool IsEnemyModel(TRModel model)
        {
            TR2Entities id = (TR2Entities)model.ID;
            return TR2EntityUtilities.IsEnemyType(id) || _additionalEnemyEntities.Contains(id);
        }

        protected override void ResetPaletteTracking(TR2Level level)
        {
            PaletteUtilities.ResetPaletteTracking(level.Palette16);
        }

        protected override void ResetUnusedTextures(TR2Level level)
        {
            level.ResetUnusedTextures();
        }

        protected override void SetObjectTextures(TR2Level level, IEnumerable<TRObjectTexture> textures)
        {
            level.ObjectTextures = textures.ToArray();
            level.NumObjectTextures = (uint)level.ObjectTextures.Length;
        }

        protected override void SetSkyboxVisible(TR2Level level)
        {
            foreach (TR2Room room in level.Rooms)
            {
                room.IsSkyboxVisible = true;
            }
        }

        protected override Dictionary<TRFace4, List<TRVertex>> CollectLadders(TR2Level level)
        {
            return LadderUtilities.GetClimbableFaces(level);
        }

        protected override TRAnimatedTexture[] GetAnimatedTextures(TR2Level level)
        {
            return level.AnimatedTextures;
        }

        protected override void SetAnimatedTextures(TR2Level level, TRAnimatedTexture[] animatedTextures, ushort length)
        {
            level.AnimatedTextures = animatedTextures;
            level.NumAnimatedTextures = length;
        }
    }
}