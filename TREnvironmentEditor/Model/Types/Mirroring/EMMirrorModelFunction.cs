using System;
using System.Collections.Generic;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Model.Textures;

namespace TREnvironmentEditor.Model.Types
{
    public class EMMirrorModelFunction : BaseEMFunction
    {
        public uint[] ModelIDs { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            List<TRMesh> meshes = new List<TRMesh>();
            foreach (uint modelID in ModelIDs)
            {
                TRMesh[] modelMeshes = TRMeshUtilities.GetModelMeshes(level, (TREntities)modelID);
                if (modelMeshes == null || modelMeshes.Length > 1)
                {
                    throw new NotSupportedException("Only models with single meshes can be mirrored.");
                }

                meshes.Add(modelMeshes[0]);
            }

            MirrorObjectTextures(MirrorMeshes(meshes), level.ObjectTextures);
        }

        public override void ApplyToLevel(TR2Level level)
        {
            List<TRMesh> meshes = new List<TRMesh>();
            foreach (uint modelID in ModelIDs)
            {
                TRMesh[] modelMeshes = TRMeshUtilities.GetModelMeshes(level, (TR2Entities)modelID);
                if (modelMeshes == null || modelMeshes.Length > 1)
                {
                    throw new NotSupportedException("Only models with single meshes can be mirrored.");
                }

                meshes.Add(modelMeshes[0]);
            }

            MirrorObjectTextures(MirrorMeshes(meshes), level.ObjectTextures);
        }

        public override void ApplyToLevel(TR3Level level)
        {
            List<TRMesh> meshes = new List<TRMesh>();
            foreach (uint modelID in ModelIDs)
            {
                TRMesh[] modelMeshes = TRMeshUtilities.GetModelMeshes(level, (TR3Entities)modelID);
                if (modelMeshes == null || modelMeshes.Length > 1)
                {
                    throw new NotSupportedException("Only models with single meshes can be mirrored.");
                }

                meshes.Add(modelMeshes[0]);
            }

            MirrorObjectTextures(MirrorMeshes(meshes), level.ObjectTextures);
        }

        private ISet<ushort> MirrorMeshes(List<TRMesh> meshes)
        {
            ISet<ushort> textureReferences = new HashSet<ushort>();

            foreach (TRMesh mesh in meshes)
            {
                foreach (TRVertex vert in mesh.Vertices)
                {
                    vert.X *= -1;
                }

                if (mesh.Normals != null)
                {
                    foreach (TRVertex norm in mesh.Normals)
                    {
                        norm.X *= -1;
                    }
                }

                foreach (TRFace4 f in mesh.TexturedRectangles)
                {
                    Swap(f.Vertices, 0, 3);
                    Swap(f.Vertices, 1, 2);
                    textureReferences.Add((ushort)(f.Texture & 0x0fff));
                }

                foreach (TRFace4 f in mesh.ColouredRectangles)
                {
                    Swap(f.Vertices, 0, 3);
                    Swap(f.Vertices, 1, 2);
                }

                foreach (TRFace3 f in mesh.TexturedTriangles)
                {
                    Swap(f.Vertices, 0, 2);
                    textureReferences.Add((ushort)(f.Texture & 0x0fff));
                }

                foreach (TRFace3 f in mesh.ColouredTriangles)
                {
                    Swap(f.Vertices, 0, 2);
                }
            }

            return textureReferences;
        }

        private void MirrorObjectTextures(ISet<ushort> textureReferences, TRObjectTexture[] objectTextures)
        {
            foreach (ushort textureRef in textureReferences)
            {
                IndexedTRObjectTexture texture = new IndexedTRObjectTexture
                {
                    Texture = objectTextures[textureRef]
                };

                if (texture.IsTriangle)
                {
                    Swap(texture.Texture.Vertices, 0, 2);
                }
                else
                {
                    Swap(texture.Texture.Vertices, 0, 3);
                    Swap(texture.Texture.Vertices, 1, 2);
                }
            }
        }

        private static void Swap<T>(T[] arr, int pos1, int pos2)
        {
            T temp = arr[pos1];
            arr[pos1] = arr[pos2];
            arr[pos2] = temp;
        }
    }
}