using TRLevelControl.Model;
using TRLevelControl.Model.Enums;

namespace TRLevelControl.Helpers;

public static class TRMeshUtilities
{
    public static TRMesh GetModelFirstMesh(TR1Level level, TR1Type entity)
    {
        TRModel model = level.Models.ToList().Find(e => e.ID == (uint)entity);
        if (model != null)
        {
            return GetModelFirstMesh(level, model);
        }
        return null;
    }

    public static TRMesh GetModelFirstMesh(TR2Level level, TR2Type entity)
    {
        TRModel model = level.Models.ToList().Find(e => e.ID == (uint)entity);
        if (model != null)
        {
            return GetModelFirstMesh(level, model);
        }
        return null;
    }

    public static TRMesh GetModelFirstMesh(TR3Level level, TR3Entities entity)
    {
        TRModel model = level.Models.ToList().Find(e => e.ID == (uint)entity);
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

    public static TRMesh[] GetModelMeshes(TR1Level level, TR1Type entity)
    {
        TRModel model = level.Models.ToList().Find(e => e.ID == (uint)entity);
        if (model != null)
        {
            return GetModelMeshes(level, model);
        }
        return null;
    }

    public static TRMesh[] GetModelMeshes(TR2Level level, TR2Type entity)
    {
        TRModel model = level.Models.ToList().Find(e => e.ID == (uint)entity);
        if (model != null)
        {
            return GetModelMeshes(level, model);
        }
        return null;
    }

    public static TRMesh[] GetModelMeshes(TR3Level level, TR3Entities entity)
    {
        TRModel model = level.Models.ToList().Find(e => e.ID == (uint)entity);
        if (model != null)
        {
            return GetModelMeshes(level, model);
        }
        return null;
    }

    public static TRMesh[] GetModelMeshes(TR1Level level, TRModel model)
    {
        return GetModelMeshes(level.Meshes, level.MeshPointers, model);
    }

    public static TRMesh[] GetModelMeshes(TR2Level level, TRModel model)
    {
        return GetModelMeshes(level.Meshes, level.MeshPointers, model);
    }

    public static TRMesh[] GetModelMeshes(TR3Level level, TRModel model)
    {
        return GetModelMeshes(level.Meshes, level.MeshPointers, model);
    }

    public static TRMesh[] GetModelMeshes(IEnumerable<TRMesh> meshes, uint[] meshPointers, TRModel model)
    {
        List<TRMesh> modelMeshes = new();
        uint meshPointer = model.StartingMesh;
        for (uint j = 0; j < model.NumMeshes; j++)
        {
            modelMeshes.Add(GetMesh(meshes, meshPointers[meshPointer + j]));
        }
        return modelMeshes.ToArray();
    }

    public static TRMesh GetMesh(TR1Level level, uint meshPointer)
    {
        return GetMesh(level.Meshes, level.MeshPointers[meshPointer]);
    }

    public static TRMesh GetMesh(TR2Level level, uint meshPointer)
    {
        return GetMesh(level.Meshes, level.MeshPointers[meshPointer]);
    }

    public static TRMesh GetMesh(TR3Level level, uint meshPointer)
    {
        return GetMesh(level.Meshes, level.MeshPointers[meshPointer]);
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

    public static TRMeshTreeNode[] GetModelMeshTrees(TRMeshTreeNode[] meshTrees, TRModel model)
    {
        List<TRMeshTreeNode> nodes = new();
        int index = (int)model.MeshTree / 4;
        for (int i = 0; i < model.NumMeshes; i++)
        {
            int offset = index + i;
            if (offset < meshTrees.Length)
            {
                nodes.Add(meshTrees[offset]);
            }
        }
        return nodes.ToArray();
    }

    /// <summary>
    /// Inserts a new mesh and returns its index in MeshPointers.
    /// </summary>
    public static int InsertMesh(TR1Level level, TRMesh newMesh)
    {
        //get the final mesh we currently have
        if(level.Meshes.Length > 0)
        {
            TRMesh lastMesh = level.Meshes[^1];
            //new mesh pointer will be the current final mesh's pointer plus its length
            newMesh.Pointer = lastMesh.Pointer + (uint)lastMesh.Serialize().Length;
        }
        else
        {
            newMesh.Pointer = 0;
        }

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

    public static int InsertMesh(TR2Level level, TRMesh newMesh)
    {
        //get the final mesh we currently have
        if (level.Meshes.Length > 0)
        {
            TRMesh lastMesh = level.Meshes[^1];
            //new mesh pointer will be the current final mesh's pointer plus its length
            newMesh.Pointer = lastMesh.Pointer + (uint)lastMesh.Serialize().Length;
        }
        else
        {
            newMesh.Pointer = 0;
        }

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

    public static int InsertMesh(TR3Level level, TRMesh newMesh)
    {
        //get the final mesh we currently have
        if (level.Meshes.Length > 0)
        {
            TRMesh lastMesh = level.Meshes[^1];
            //new mesh pointer will be the current final mesh's pointer plus its length
            newMesh.Pointer = lastMesh.Pointer + (uint)lastMesh.Serialize().Length;
        }
        else
        {
            newMesh.Pointer = 0;
        }

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
    public static int InsertMeshTreeNode(TR1Level level, TRMeshTreeNode newNode)
    {
        List<TRMeshTreeNode> nodes = level.MeshTrees.ToList();
        nodes.Add(newNode);
        level.MeshTrees = nodes.ToArray();
        level.NumMeshTrees++;

        return level.MeshTrees.Length - 1;
    }

    public static int InsertMeshTreeNode(TR2Level level, TRMeshTreeNode newNode)
    {
        List<TRMeshTreeNode> nodes = level.MeshTrees.ToList();
        nodes.Add(newNode);
        level.MeshTrees = nodes.ToArray();
        level.NumMeshTrees++;

        return level.MeshTrees.Length - 1;
    }

    public static int InsertMeshTreeNode(TR3Level level, TRMeshTreeNode newNode)
    {
        List<TRMeshTreeNode> nodes = level.MeshTrees.ToList();
        nodes.Add(newNode);
        level.MeshTrees = nodes.ToArray();
        level.NumMeshTrees++;

        return level.MeshTrees.Length - 1;
    }
}
