using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMRemoveStaticCollisionFunction : BaseEMFunction
{
    public int[] StaticIDs { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        RemoveCollision(StaticIDs.Select(i =>
            level.StaticMeshes[(TR1Type)((int)TR1Type.SceneryBase + i)]));
    }

    public override void ApplyToLevel(TR2Level level)
    {
        RemoveCollision(StaticIDs.Select(i => 
            level.StaticMeshes[(TR2Type)((int)TR2Type.SceneryBase + i)]));
    }

    public override void ApplyToLevel(TR3Level level)
    {
        RemoveCollision(StaticIDs.Select(i =>
            level.StaticMeshes[(TR3Type)((int)TR3Type.SceneryBase + i)]));
    }

    private static void RemoveCollision(IEnumerable<TRStaticMesh> meshes)
    {
        foreach (var mesh in meshes)
        {
            mesh.CollisionBox = new();
            mesh.NonCollidable = true;
        }
    }
}
