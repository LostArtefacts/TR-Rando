using TRLevelControl.Build;
using TRLevelControl.Model;

namespace TRLevelControl.Helpers;

public static class TRMeshUtilities
{
    // Temporary - we only need to serialize to work out new pointers
    // but this will eventually be eliminated and worked out on write.
    // This passes TR1 to the builder by default but covers TR1-3.
    // TR4/5 not yet required (and shouldn't be).
    public static byte[] Serialize(TRMesh mesh, TRGameVersion version = TRGameVersion.TR1)
    {
        TRObjectMeshBuilder builder = new(version);
        return builder.Serialize(mesh);
    }

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
            length += Serialize(mesh).Length;
        }
        return null;
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
            newMesh.Pointer = lastMesh.Pointer + (uint)Serialize(lastMesh).Length;
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
        int oldLength = Serialize(originalMesh).Length;
        ReplaceMesh(originalMesh, replacementMesh);

        // The length will have changed so all pointers above the original one will need adjusting
        UpdateMeshPointers(level, originalMesh, oldLength);
    }

    public static void DuplicateMesh(TR2Level level, TRMesh originalMesh, TRMesh replacementMesh)
    {
        int oldLength = Serialize(originalMesh).Length;
        ReplaceMesh(originalMesh, replacementMesh);

        // The length will have changed so all pointers above the original one will need adjusting
        UpdateMeshPointers(level, originalMesh, oldLength);
    }

    public static void DuplicateMesh(TR3Level level, TRMesh originalMesh, TRMesh replacementMesh)
    {
        int oldLength = Serialize(originalMesh).Length;
        ReplaceMesh(originalMesh, replacementMesh);

        // The length will have changed so all pointers above the original one will need adjusting
        UpdateMeshPointers(level, originalMesh, oldLength);
    }

    private static void ReplaceMesh(TRMesh originalMesh, TRMesh replacementMesh)
    {
        replacementMesh.CopyInto(originalMesh);
    }

    /// <summary>
    /// For a given mesh that has been changed and its previous serialized length, ensures that all
    /// mesh pointers above the modified mesh are updated correctly.
    /// </summary>
    public static void UpdateMeshPointers(TR1Level level, TRMesh modifiedMesh, int previousMeshLength)
    {
        int lengthDiff = Serialize(modifiedMesh).Length - previousMeshLength;
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
        int lengthDiff = Serialize(modifiedMesh).Length - previousMeshLength;
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
        int lengthDiff = Serialize(modifiedMesh).Length - previousMeshLength;
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
}
