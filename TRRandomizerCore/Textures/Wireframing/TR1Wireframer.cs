using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Helpers;
using TRModelTransporter.Packing;
using TRTexture16Importer.Helpers;

namespace TRRandomizerCore.Textures
{
    public class TR1Wireframer : AbstractTRWireframer<TREntities, TRLevel>
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

        protected override AbstractTexturePacker<TREntities, TRLevel> CreatePacker(TRLevel level)
        {
            return _packer = new TR1TexturePacker(level);
        }

        protected override bool IsSkybox(TRModel model)
        {
            return false;
        }

        protected override bool ShouldSolidifyModel(TRModel model)
        {
            return _data.Has3DPickups && _pickupModels.Contains((TREntities)model.ID);
        }

        protected override int GetBlackPaletteIndex(TRLevel level)
        {
            return ImportColour(level, Color.Black);
        }

        protected override IEnumerable<int> GetInvalidObjectTextureIndices(TRLevel level)
        {
            return level.GetInvalidObjectTextureIndices();
        }

        protected override TRMesh[] GetLevelMeshes(TRLevel level)
        {
            return level.Meshes;
        }

        protected override Dictionary<TREntities, TRMesh[]> GetModelMeshes(TRLevel level)
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

        protected override TRMesh[] GetModelMeshes(TRLevel level, TRModel model)
        {
            return TRMeshUtilities.GetModelMeshes(level, model);
        }

        protected override TRModel[] GetModels(TRLevel level)
        {
            return level.Models;
        }

        protected override TRObjectTexture[] GetObjectTextures(TRLevel level)
        {
            return level.ObjectTextures;
        }

        protected override IEnumerable<IEnumerable<TRFace3>> GetRoomFace3s(TRLevel level)
        {
            List<List<TRFace3>> faces = new List<List<TRFace3>>();
            foreach (TRRoom room in level.Rooms)
            {
                faces.Add(room.RoomData.Triangles.ToList());
            }
            return faces;
        }

        protected override IEnumerable<IEnumerable<TRFace4>> GetRoomFace4s(TRLevel level)
        {
            List<List<TRFace4>> faces = new List<List<TRFace4>>();
            foreach (TRRoom room in level.Rooms)
            {
                faces.Add(room.RoomData.Rectangles.ToList());
            }
            return faces;
        }

        protected override TRMesh GetStaticMesh(TRLevel level, TRStaticMesh staticMesh)
        {
            return TRMeshUtilities.GetMesh(level, staticMesh.Mesh);
        }

        protected override TRStaticMesh[] GetStaticMeshes(TRLevel level)
        {
            return level.StaticMeshes;
        }

        protected override int ImportColour(TRLevel level, Color c)
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

        protected override void ResetPaletteTracking(TRLevel level)
        {
            if (_packer.PaletteManager != null)
            {
                _packer.PaletteManager.MergePredefinedColours();
            }
        }

        protected override void ResetUnusedTextures(TRLevel level)
        {
            level.ResetUnusedTextures();
        }

        protected override void SetObjectTextures(TRLevel level, IEnumerable<TRObjectTexture> textures)
        {
            level.ObjectTextures = textures.ToArray();
            level.NumObjectTextures = (uint)level.ObjectTextures.Length;
        }

        protected override void SetSkyboxVisible(TRLevel level) { }

        protected override Dictionary<TRFace4, List<TRVertex>> CollectLadders(TRLevel level)
        {
            return new Dictionary<TRFace4, List<TRVertex>>();
        }

        protected override TRAnimatedTexture[] GetAnimatedTextures(TRLevel level)
        {
            return level.AnimatedTextures;
        }

        protected override void SetAnimatedTextures(TRLevel level, TRAnimatedTexture[] animatedTextures, ushort length)
        {
            level.AnimatedTextures = animatedTextures;
            level.NumAnimatedTextures = length;
        }
    }
}