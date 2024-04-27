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
        throw new NotSupportedException();
    }

    public static TRMesh GetModelFirstMesh(TR1Level level, TR1Type entity)
    {
        throw new NotSupportedException();
    }

    public static TRMesh GetModelFirstMesh(TR2Level level, TR2Type entity)
    {
        throw new NotSupportedException();
    }

    public static List<TRMesh> GetModelMeshes(TR1Level level, TR1Type entity)
    {
        throw new NotSupportedException();
    }

    public static List<TRMesh> GetModelMeshes(TR2Level level, TR2Type entity)
    {
        throw new NotSupportedException();
    }

    public static List<TRMesh> GetModelMeshes(TR3Level level, TR3Type entity)
    {
        throw new NotSupportedException();
    }

    public static List<TRMesh> GetModelMeshes(TR1Level level, TRModel model)
    {
        throw new NotSupportedException();
    }

    public static List<TRMesh> GetModelMeshes(TR2Level level, TRModel model)
    {
        throw new NotSupportedException();
    }

    public static List<TRMesh> GetModelMeshes(TR3Level level, TRModel model)
    {
        throw new NotSupportedException();
    }

    public static TRMesh GetMesh(TR1Level level, uint meshPointer)
    {
        throw new NotSupportedException();
    }

    public static TRMesh GetMesh(TR2Level level, uint meshPointer)
    {
        throw new NotSupportedException();
    }

    public static TRMesh GetMesh(TR3Level level, uint meshPointer)
    {
        throw new NotSupportedException();
    }

    public static TRMesh GetMesh(IEnumerable<TRMesh> meshes, uint offset)
    {
        throw new NotSupportedException();
    }

    public static int InsertMesh(TR1Level level, TRMesh newMesh)
        => throw new NotSupportedException();

    public static int InsertMesh(TR2Level level, TRMesh newMesh)
        => throw new NotSupportedException();

    public static int InsertMesh(TR3Level level, TRMesh newMesh)
        => throw new NotSupportedException();

    /// <summary>
    /// Duplicates the data from one mesh to another and ensures that the contents
    /// of MeshPointers remains consistent with respect to the mesh lengths.
    /// </summary>
    public static void DuplicateMesh(TR1Level level, TRMesh originalMesh, TRMesh replacementMesh)
    {
        throw new NotSupportedException();
    }

    public static void DuplicateMesh(TR2Level level, TRMesh originalMesh, TRMesh replacementMesh)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// For a given mesh that has been changed and its previous serialized length, ensures that all
    /// mesh pointers above the modified mesh are updated correctly.
    /// </summary>
    public static void UpdateMeshPointers(TR1Level level, TRMesh modifiedMesh, int previousMeshLength)
    {
        throw new NotSupportedException();
    }

    public static void UpdateMeshPointers(TR2Level level, TRMesh modifiedMesh, int previousMeshLength)
    {
        throw new NotSupportedException();
    }

    public static void UpdateMeshPointers(TR3Level level, TRMesh modifiedMesh, int previousMeshLength)
    {
        throw new NotSupportedException();
    }
}
