using TRLevelReader.Helpers;
using TRLevelReader.Model;

namespace TRModelTransporter.Handlers
{
    public class MeshTransportHandler : AbstractTransportHandler
    {
        public override void Export()
        {
            Definition.MeshTrees = TR2LevelUtilities.GetModelMeshTrees(Level, Definition.Model);
            Definition.Meshes = TR2LevelUtilities.GetModelMeshes(Level, Definition.Model);
        }

        public override void Import()
        {
            // Copy the MeshTreeNodes and Meshes into the level, making a note of the first
            // inserted index for each - this is used to update the Model to point to the
            // correct starting positions.
            for (int i = 0; i < Definition.MeshTrees.Length; i++)
            {
                TRMeshTreeNode tree = Definition.MeshTrees[i];
                int insertedIndex = TR2LevelUtilities.InsertMeshTreeNode(Level, tree);
                if (i == 0)
                {
                    Definition.Model.MeshTree = 4 * (uint)insertedIndex;
                }
            }

            for (int i = 0; i < Definition.Meshes.Length; i++)
            {
                TRMesh mesh = Definition.Meshes[i];
                int insertedIndex = TR2LevelUtilities.InsertMesh(Level, mesh);
                if (i == 0)
                {
                    Definition.Model.StartingMesh = (ushort)insertedIndex;
                }
            }
        }
    }
}