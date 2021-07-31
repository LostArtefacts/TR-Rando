using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TRLevelReader.Helpers
{
    public static class TR2LevelUtilities
    {
        public static TRModel GetModel(TR2Level level, TR2Entities entity)
        {
            int i = level.Models.ToList().FindIndex(e => e.ID == (uint)entity);
            if (i != -1)
            {
                return level.Models[i];
            }
            return null;
        }

        public static TRMesh GetModelFirstMesh(TR2Level level, TR2Entities entity)
        {
            TRModel model = GetModel(level, entity);
            if (model != null)
            {
                return GetModelFirstMesh(level, model);
            }
            return null;
        }

        public static TRMesh GetModelFirstMesh(TR2Level level, TRModel model)
        {
            return GetMesh(level, model.StartingMesh);
        }

        public static TRMeshTreeNode[] GetModelMeshTrees(TR2Level level, TR2Entities entity)
        {
            TRModel model = GetModel(level, entity);
            if (model != null)
            {
                return GetModelMeshTrees(level, model);
            }
            return null;
        }

        public static TRMeshTreeNode[] GetModelMeshTrees(TR2Level level, TRModel model)
        {
            List<TRMeshTreeNode> nodes = new List<TRMeshTreeNode>();
            int index = (int)model.MeshTree / 4;
            for (int i = 0; i < model.NumMeshes; i++)
            {
                int offset = index + i;
                if (offset < level.MeshTrees.Length)
                {
                    nodes.Add(level.MeshTrees[offset]);
                }
            }
            return nodes.ToArray();
        }

        public static TRMesh[] GetModelMeshes(TR2Level level, TR2Entities entity)
        {
            TRModel model = GetModel(level, entity);
            if (model != null)
            {
                return GetModelMeshes(level, model);
            }
            return null;
        }

        public static TRMesh[] GetModelMeshes(TR2Level level, TRModel model)
        {
            List<TRMesh> meshes = new List<TRMesh>();
            int meshPointer = model.StartingMesh;
            for (int j = 0; j < model.NumMeshes; j++)
            {
                meshes.Add(GetMesh(level, meshPointer + j));
            }
            return meshes.ToArray();
        }

        public static TRMesh GetMesh(TR2Level level, int meshPointer)
        {
            uint offset = level.MeshPointers[meshPointer];
            int length = 0;
            foreach (TRMesh mesh in level.Meshes)
            {
                if (length == offset)
                {
                    return mesh;
                }
                length += mesh.Serialize().Length;
            }
            return null;
        }

        /// <summary>
        /// Inserts a new mesh and returns its index in MeshPointers.
        /// </summary>
        public static int InsertMesh(TR2Level level, TRMesh newMesh)
        {
            //get the final mesh we currently have
            TRMesh lastMesh = level.Meshes[level.Meshes.Length - 1];
            //new mesh pointer will be the current final mesh's pointer plus its length
            newMesh.Pointer = lastMesh.Pointer + (uint)lastMesh.Serialize().Length;

            List<TRMesh> meshes = level.Meshes.ToList();
            meshes.Add(newMesh);
            level.Meshes = meshes.ToArray();

            List<uint> pointers = level.MeshPointers.ToList();
            pointers.Add(newMesh.Pointer);
            level.MeshPointers = pointers.ToArray();
            level.NumMeshPointers++;

            //NumMeshData needs the additional mesh size added
            level.NumMeshData += (uint)newMesh.Serialize().Length / 2;

            //the pointer index will be the final index in the array
            return level.MeshPointers.Length - 1;
        }

        /// <summary>
        /// Duplicates the data from one mesh to another and ensures that the contents
        /// of MeshPointers remains consistent with respect to the mesh lengths.
        /// </summary>
        public static void DuplicateMesh(TR2Level level, TRMesh originalMesh, TRMesh replacementMesh)
        {
            int oldLength = originalMesh.Serialize().Length;

            originalMesh.Centre = replacementMesh.Centre;
            originalMesh.CollRadius = replacementMesh.CollRadius;
            originalMesh.ColouredRectangles = replacementMesh.ColouredRectangles;
            originalMesh.ColouredTriangles = replacementMesh.ColouredTriangles;
            originalMesh.Lights = replacementMesh.Lights;
            originalMesh.Normals = replacementMesh.Normals;
            originalMesh.NumColouredRectangles = replacementMesh.NumColouredRectangles;
            originalMesh.NumColouredTriangles = replacementMesh.NumColouredTriangles;
            originalMesh.NumNormals = replacementMesh.NumNormals;
            originalMesh.NumTexturedRectangles = replacementMesh.NumTexturedRectangles;
            originalMesh.NumTexturedTriangles = replacementMesh.NumTexturedTriangles;
            originalMesh.NumVertices = replacementMesh.NumVertices;
            originalMesh.TexturedRectangles = replacementMesh.TexturedRectangles;
            originalMesh.TexturedTriangles = replacementMesh.TexturedTriangles;
            originalMesh.Vertices = replacementMesh.Vertices;

            // The length will have changed so all pointers above the original one will need adjusting
            UpdateMeshPointers(level, originalMesh, oldLength);
        }

        /// <summary>
        /// For a given mesh that has been changed and its previous serialized length, ensures that all
        /// mesh pointers above the modified mesh are updated correctly.
        /// </summary>
        public static void UpdateMeshPointers(TR2Level level, TRMesh modifiedMesh, int previousMeshLength)
        {
            int lengthDiff = modifiedMesh.Serialize().Length - previousMeshLength;
            List<uint> pointers = level.MeshPointers.ToList();
            int pointerIndex = pointers.IndexOf(modifiedMesh.Pointer);
            Dictionary<uint, uint> pointerMap = new Dictionary<uint, uint>();
            for (int i = pointerIndex + 1; i < pointers.Count; i++)
            {
                if (pointers[i] > 0)
                {
                    int newPointer = (int)pointers[i] + lengthDiff;
                    pointerMap.Add(pointers[i], pointers[i] = (uint)newPointer);
                }
            }

            level.MeshPointers = pointers.ToArray();

            int numMeshData = (int)level.NumMeshData + lengthDiff / 2;
            level.NumMeshData = (uint)numMeshData;

            // While the Pointer property on meshes is only for convenience and not
            // written to the level, we need to update them regardless in case of
            // additional changes before the level is saved and re-read.
            foreach (TRMesh mesh in level.Meshes)
            {
                if (pointerMap.ContainsKey(mesh.Pointer))
                {
                    mesh.Pointer = pointerMap[mesh.Pointer];
                }
            }
        }

        /// <summary>
        /// Inserts a new mesh tree node and returns its index in MeshTrees. 
        /// </summary>
        public static int InsertMeshTreeNode(TR2Level level, TRMeshTreeNode newNode)
        {
            List<TRMeshTreeNode> nodes = level.MeshTrees.ToList();
            nodes.Add(newNode);
            level.MeshTrees = nodes.ToArray();
            level.NumMeshTrees++;

            return level.MeshTrees.Length - 1;
        }
    }
}