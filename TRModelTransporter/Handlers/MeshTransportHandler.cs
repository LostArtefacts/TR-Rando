using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRModelTransporter.Model.Definitions;

namespace TRModelTransporter.Handlers;

public class MeshTransportHandler
{
    public static void Export(TR1Level level, TR1ModelDefinition definition)
    {
        definition.MeshTrees = TRMeshUtilities.GetModelMeshTrees(level, definition.Model);
        definition.Meshes = TRMeshUtilities.GetModelMeshes(level, definition.Model);
    }

    public static void Export(TR2Level level, TR2ModelDefinition definition)
    {
        definition.MeshTrees = TRMeshUtilities.GetModelMeshTrees(level, definition.Model);
        definition.Meshes = TRMeshUtilities.GetModelMeshes(level, definition.Model);
    }

    public static void Export(TR3Level level, TR3ModelDefinition definition)
    {
        definition.MeshTrees = TRMeshUtilities.GetModelMeshTrees(level, definition.Model);
        definition.Meshes = TRMeshUtilities.GetModelMeshes(level, definition.Model);
    }

    public static void Import(TR1Level level, TR1ModelDefinition definition)
    {
        // Copy the MeshTreeNodes and Meshes into the level, making a note of the first
        // inserted index for each - this is used to update the Model to point to the
        // correct starting positions.
        for (int i = 0; i < definition.MeshTrees.Length; i++)
        {
            int insertedIndex = TRMeshUtilities.InsertMeshTreeNode(level, definition.MeshTrees[i]);
            if (i == 0)
            {
                definition.Model.MeshTree = 4 * (uint)insertedIndex;
            }
        }

        for (int i = 0; i < definition.Meshes.Count; i++)
        {
            int insertedIndex = TRMeshUtilities.InsertMesh(level, definition.Meshes[i]);
            if (i == 0)
            {
                definition.Model.StartingMesh = (ushort)insertedIndex;
            }
            SetFaceTypes(definition.Meshes[i]);
        }
    }

    public static void Import(TR2Level level, TR2ModelDefinition definition)
    {
        for (int i = 0; i < definition.MeshTrees.Length; i++)
        {
            int insertedIndex = TRMeshUtilities.InsertMeshTreeNode(level, definition.MeshTrees[i]);
            if (i == 0)
            {
                definition.Model.MeshTree = 4 * (uint)insertedIndex;
            }
        }

        for (int i = 0; i < definition.Meshes.Count; i++)
        {
            int insertedIndex = TRMeshUtilities.InsertMesh(level, definition.Meshes[i]);
            if (i == 0)
            {
                definition.Model.StartingMesh = (ushort)insertedIndex;
            }
            SetFaceTypes(definition.Meshes[i]);
        }
    }

    public static void Import(TR3Level level, TR3ModelDefinition definition)
    {
        for (int i = 0; i < definition.MeshTrees.Length; i++)
        {
            int insertedIndex = TRMeshUtilities.InsertMeshTreeNode(level, definition.MeshTrees[i]);
            if (i == 0)
            {
                definition.Model.MeshTree = 4 * (uint)insertedIndex;
            }
        }

        for (int i = 0; i < definition.Meshes.Count; i++)
        {
            int insertedIndex = TRMeshUtilities.InsertMesh(level, definition.Meshes[i]);
            if (i == 0)
            {
                definition.Model.StartingMesh = (ushort)insertedIndex;
            }
            SetFaceTypes(definition.Meshes[i]);
        }
    }

    private static void SetFaceTypes(TRMesh mesh)
    {
        // Temporary until this library is redesigned, so to avoid re-writing
        // exported data.
        mesh.ColouredRectangles.Concat(mesh.TexturedRectangles)
            .ToList()
            .ForEach(face => face.Type = TRFaceType.Rectangle);
        mesh.ColouredTriangles.Concat(mesh.TexturedTriangles)
            .ToList()
            .ForEach(face => face.Type = TRFaceType.Triangle);
    }
}
