using TRLevelControl.Model;

namespace TRLevelControl.Helpers;

public static class TRMeshUtilities
{
    public static TRMesh GetModelFirstMesh(TR1Level level, TR1Type entity)
    {
        TRModel model = level.Models.Find(e => e.ID == (uint)entity);
        if (model != null)
        {
            return GetModelFirstMesh(level, model);
        }
        return null;
    }

    public static TRMesh GetModelFirstMesh(TR2Level level, TR2Type entity)
    {
        TRModel model = level.Models.Find(e => e.ID == (uint)entity);
        if (model != null)
        {
            return GetModelFirstMesh(level, model);
        }
        return null;
    }

    public static TRMesh GetModelFirstMesh(TR3Level level, TR3Type entity)
    {
        TRModel model = level.Models.Find(e => e.ID == (uint)entity);
        if (model != null)
        {
            return GetModelFirstMesh(level, model);
        }
        return null;
    }

    public static TRMesh GetModelFirstMesh(TR1Level level, TRModel model)
    {
        return GetMesh(level, model.StartingMesh);
    }
    
    public static TRMesh GetModelFirstMesh(TR2Level level, TRModel model)
    {
        return GetMesh(level, model.StartingMesh);
    }

    public static TRMesh GetModelFirstMesh(TR3Level level, TRModel model)
    {
        return GetMesh(level, model.StartingMesh);
    }

    public static List<TRMesh> GetModelMeshes(TR1Level level, TR1Type entity)
    {
        TRModel model = level.Models.Find(e => e.ID == (uint)entity);
        if (model != null)
        {
            return GetModelMeshes(level, model);
        }
        return null;
    }

    public static List<TRMesh> GetModelMeshes(TR2Level level, TR2Type entity)
    {
        TRModel model = level.Models.Find(e => e.ID == (uint)entity);
        if (model != null)
        {
            return GetModelMeshes(level, model);
        }
        return null;
    }

    public static List<TRMesh> GetModelMeshes(TR3Level level, TR3Type entity)
    {
        TRModel model = level.Models.Find(e => e.ID == (uint)entity);
        if (model != null)
        {
            return GetModelMeshes(level, model);
        }
        return null;
    }

    public static List<TRMesh> GetModelMeshes(TR1Level level, TRModel model)
    {
        return GetModelMeshes(level.Meshes, level.MeshPointers, model);
    }

    public static List<TRMesh> GetModelMeshes(TR2Level level, TRModel model)
    {
        return GetModelMeshes(level.Meshes, level.MeshPointers, model);
    }

    public static List<TRMesh> GetModelMeshes(TR3Level level, TRModel model)
    {
        return GetModelMeshes(level.Meshes, level.MeshPointers, model);
    }

    public static List<TRMesh> GetModelMeshes(IEnumerable<TRMesh> meshes, List<uint> meshPointers, TRModel model)
    {
        List<TRMesh> modelMeshes = new();
        int meshPointer = model.StartingMesh;
        for (int j = 0; j < model.NumMeshes; j++)
        {
            modelMeshes.Add(GetMesh(meshes, meshPointers[meshPointer + j]));
        }
        return modelMeshes;
    }

    public static TRMesh GetMesh(TR1Level level, uint meshPointer)
    {
        return GetMesh(level.Meshes, level.MeshPointers[(int)meshPointer]);
    }

    public static TRMesh GetMesh(TR2Level level, uint meshPointer)
    {
        return GetMesh(level.Meshes, level.MeshPointers[(int)meshPointer]);
    }

    public static TRMesh GetMesh(TR3Level level, uint meshPointer)
    {
        return GetMesh(level.Meshes, level.MeshPointers[(int)meshPointer]);
    }

    public static TRMesh GetMesh(IEnumerable<TRMesh> meshes, uint offset)
    {
        int length = 0;
        foreach (TRMesh mesh in meshes)
        {
            if (length == offset)
            {
                return mesh;
            }
            length += mesh.Serialize().Length;
        }
        return null;
    }

    public static TRMeshTreeNode[] GetModelMeshTrees(TR1Level level, TRModel model)
    {
        return GetModelMeshTrees(level.MeshTrees, model);
    }

    public static TRMeshTreeNode[] GetModelMeshTrees(TR2Level level, TRModel model)
    {
        return GetModelMeshTrees(level.MeshTrees, model);
    }

    public static TRMeshTreeNode[] GetModelMeshTrees(TR3Level level, TRModel model)
    {
        return GetModelMeshTrees(level.MeshTrees, model);
    }

    public static TRMeshTreeNode[] GetModelMeshTrees(List<TRMeshTreeNode> meshTrees, TRModel model)
    {
        List<TRMeshTreeNode> nodes = new();
        int index = (int)model.MeshTree / 4;
        for (int i = 0; i < model.NumMeshes; i++)
        {
            int offset = index + i;
            if (offset < meshTrees.Count)
            {
                nodes.Add(meshTrees[offset]);
            }
        }
        return nodes.ToArray();
    }

    public static int InsertMesh(TR1Level level, TRMesh newMesh)
        => InsertMesh(newMesh, level.Meshes, level.MeshPointers);

    public static int InsertMesh(TR2Level level, TRMesh newMesh)
        => InsertMesh(newMesh, level.Meshes, level.MeshPointers);

    public static int InsertMesh(TR3Level level, TRMesh newMesh)
        => InsertMesh(newMesh, level.Meshes, level.MeshPointers);

    private static int InsertMesh(TRMesh newMesh, List<TRMesh> meshes, List<uint> meshPointers)
    {
        if (meshes.Count > 0)
        {
            TRMesh lastMesh = meshes[^1];
            newMesh.Pointer = lastMesh.Pointer + (uint)lastMesh.Serialize().Length;
        }
        else
        {
            newMesh.Pointer = 0;
        }

        meshes.Add(newMesh);
        meshPointers.Add(newMesh.Pointer);

        return meshPointers.Count - 1;
    }

    /// <summary>
    /// Duplicates the data from one mesh to another and ensures that the contents
    /// of MeshPointers remains consistent with respect to the mesh lengths.
    /// </summary>
    public static void DuplicateMesh(TR1Level level, TRMesh originalMesh, TRMesh replacementMesh)
    {
        int oldLength = originalMesh.Serialize().Length;
        ReplaceMesh(originalMesh, replacementMesh);

        // The length will have changed so all pointers above the original one will need adjusting
        UpdateMeshPointers(level, originalMesh, oldLength);
    }

    public static void DuplicateMesh(TR2Level level, TRMesh originalMesh, TRMesh replacementMesh)
    {
        int oldLength = originalMesh.Serialize().Length;
        ReplaceMesh(originalMesh, replacementMesh);

        // The length will have changed so all pointers above the original one will need adjusting
        UpdateMeshPointers(level, originalMesh, oldLength);
    }

    public static void DuplicateMesh(TR3Level level, TRMesh originalMesh, TRMesh replacementMesh)
    {
        int oldLength = originalMesh.Serialize().Length;
        ReplaceMesh(originalMesh, replacementMesh);

        // The length will have changed so all pointers above the original one will need adjusting
        UpdateMeshPointers(level, originalMesh, oldLength);
    }

    private static void ReplaceMesh(TRMesh originalMesh, TRMesh replacementMesh)
    {
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
    }

    /// <summary>
    /// For a given mesh that has been changed and its previous serialized length, ensures that all
    /// mesh pointers above the modified mesh are updated correctly.
    /// </summary>
    public static void UpdateMeshPointers(TR1Level level, TRMesh modifiedMesh, int previousMeshLength)
    {
        int lengthDiff = modifiedMesh.Serialize().Length - previousMeshLength;
        List<uint> pointers = level.MeshPointers.ToList();
        int pointerIndex = pointers.IndexOf(modifiedMesh.Pointer);
        Dictionary<uint, uint> pointerMap = new();
        for (int i = pointerIndex + 1; i < pointers.Count; i++)
        {
            if (pointers[i] > 0)
            {
                int newPointer = (int)pointers[i] + lengthDiff;
                pointerMap.Add(pointers[i], pointers[i] = (uint)newPointer);
            }
        }

        level.MeshPointers.Clear();
        level.MeshPointers.AddRange(pointers);

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

    public static void UpdateMeshPointers(TR2Level level, TRMesh modifiedMesh, int previousMeshLength)
    {
        int lengthDiff = modifiedMesh.Serialize().Length - previousMeshLength;
        List<uint> pointers = level.MeshPointers.ToList();
        int pointerIndex = pointers.IndexOf(modifiedMesh.Pointer);
        Dictionary<uint, uint> pointerMap = new();
        for (int i = pointerIndex + 1; i < pointers.Count; i++)
        {
            if (pointers[i] > 0)
            {
                int newPointer = (int)pointers[i] + lengthDiff;
                pointerMap.Add(pointers[i], pointers[i] = (uint)newPointer);
            }
        }

        level.MeshPointers.Clear();
        level.MeshPointers.AddRange(pointers);

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

    public static void UpdateMeshPointers(TR3Level level, TRMesh modifiedMesh, int previousMeshLength)
    {
        int lengthDiff = modifiedMesh.Serialize().Length - previousMeshLength;
        List<uint> pointers = level.MeshPointers.ToList();
        int pointerIndex = pointers.IndexOf(modifiedMesh.Pointer);
        Dictionary<uint, uint> pointerMap = new();
        for (int i = pointerIndex + 1; i < pointers.Count; i++)
        {
            if (pointers[i] > 0)
            {
                int newPointer = (int)pointers[i] + lengthDiff;
                pointerMap.Add(pointers[i], pointers[i] = (uint)newPointer);
            }
        }

        level.MeshPointers.Clear();
        level.MeshPointers.AddRange(pointers);

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
    public static int InsertMeshTreeNode(TR1Level level, TRMeshTreeNode newNode)
    {
        level.MeshTrees.Add(newNode);
        return level.MeshTrees.Count - 1;
    }

    public static int InsertMeshTreeNode(TR2Level level, TRMeshTreeNode newNode)
    {
        level.MeshTrees.Add(newNode);
        return level.MeshTrees.Count - 1;
    }

    public static int InsertMeshTreeNode(TR3Level level, TRMeshTreeNode newNode)
    {
        level.MeshTrees.Add(newNode);
        return level.MeshTrees.Count - 1;
    }
}
