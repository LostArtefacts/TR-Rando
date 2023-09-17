using TRLevelControl.Model;
using TRModelTransporter.Model.Definitions;

namespace TRModelTransporter.Handlers;

public class ModelTransportHandler
{
    public static void Export(TR1Level level, TR1ModelDefinition definition, TR1Type entity)
    {
        definition.Model = GetTRModel(level.Models, (short)entity);
    }

    public static void Export(TR2Level level, TR2ModelDefinition definition, TR2Type entity)
    {
        definition.Model = GetTRModel(level.Models, (short)entity);
    }

    public static void Export(TR3Level level, TR3ModelDefinition definition, TR3Type entity)
    {
        definition.Model = GetTRModel(level.Models, (short)entity);
    }

    private static TRModel GetTRModel(IEnumerable<TRModel> models, short entityID)
    {
        TRModel model = models.ToList().Find(m => m.ID == entityID);
        return model ?? throw new ArgumentException($"The model for {entityID} could not be found.");
    }

    public static void Import(TR1Level level, TR1ModelDefinition definition, Dictionary<TR1Type, TR1Type> aliasPriority, IEnumerable<TR1Type> laraDependants)
    {
        List<TRModel> levelModels = level.Models.ToList();
        int i = levelModels.FindIndex(m => m.ID == (short)definition.Entity);
        if (i == -1)
        {
            levelModels.Add(definition.Model);
            level.Models = levelModels.ToArray();
            level.NumModels++;
        }
        else if (!aliasPriority.ContainsKey(definition.Entity) || aliasPriority[definition.Entity] == definition.Alias)
        {
            if (!definition.HasGraphics)
            {
                // The original mesh data may still be needed so don't overwrite
                definition.Model.MeshTree = level.Models[i].MeshTree;
                definition.Model.NumMeshes = level.Models[i].NumMeshes;
                definition.Model.StartingMesh = level.Models[i].StartingMesh;
            }
            level.Models[i] = definition.Model;
        }

        if (laraDependants != null)
        {
            if (definition.Entity == TR1Type.Lara)
            {
                ReplaceLaraDependants(levelModels, definition.Model, laraDependants.Select(e => (short)e));
            }
            else if (laraDependants.Contains((TR1Type)definition.Model.ID))
            {
                ReplaceLaraDependants(levelModels, levelModels.Find(m => m.ID == (uint)TR1Type.Lara), new short[] { (short)definition.Model.ID });
            }
        }
    }

    public static void Import(TR2Level level, TR2ModelDefinition definition, Dictionary<TR2Type, TR2Type> aliasPriority, IEnumerable<TR2Type> laraDependants)
    {
        List<TRModel> levelModels = level.Models.ToList();
        int i = levelModels.FindIndex(m => m.ID == (short)definition.Entity);
        if (i == -1)
        {
            levelModels.Add(definition.Model);
            level.Models = levelModels.ToArray();
            level.NumModels++;
        }
        else if (!aliasPriority.ContainsKey(definition.Entity) || aliasPriority[definition.Entity] == definition.Alias)
        {
            // Replacement occurs for the likes of aliases taking the place of another
            // e.g. WhiteTiger replacing BengalTiger in GW, or if we have a specific
            // alias that should always have a higher priority than its peers.
            level.Models[i] = definition.Model;
        }

        // If we have replaced Lara, we need to update models such as CameraTarget, FlameEmitter etc
        // as these use Lara's hips as placeholders. This means we can avoid texture corruption in
        // TRView but it's also needed for the shower cutscene in HSH. If these entities are found,
        // their starting mesh and mesh tree indices are just remapped to Lara's.
        if (definition.Entity == TR2Type.Lara && laraDependants != null)
        {
            ReplaceLaraDependants(levelModels, definition.Model, laraDependants.Select(e => (short)e));
        }
    }

    public static void Import(TR3Level level, TR3ModelDefinition definition, Dictionary<TR3Type, TR3Type> aliasPriority, IEnumerable<TR3Type> laraDependants, IEnumerable<TR3Type> unsafeReplacements)
    {
        List<TRModel> levelModels = level.Models.ToList();
        int i = levelModels.FindIndex(m => m.ID == (short)definition.Entity);
        if (i == -1)
        {
            levelModels.Add(definition.Model);
            level.Models = levelModels.ToArray();
            level.NumModels++;
        }
        else if (!aliasPriority.ContainsKey(definition.Entity) || aliasPriority[definition.Entity] == definition.Alias)
        {
            if (!unsafeReplacements.Contains(definition.Entity))
            {
                level.Models[i] = definition.Model;
            }
            else
            {
                // #234 Replacing Lara entirely can cause locking issues after pressing buttons or crouching
                // where she refuses to come out of her stance. TR3 seems bound to having Lara's animations start
                // at 0, so because these don't change per skin, we just replace the meshes and frames here.
                level.Models[i].NumMeshes = definition.Model.NumMeshes;
                level.Models[i].StartingMesh = definition.Model.StartingMesh;
                level.Models[i].MeshTree = definition.Model.MeshTree;
                level.Models[i].FrameOffset = definition.Model.FrameOffset;
            }
        }

        if (definition.Entity == TR3Type.Lara && laraDependants != null)
        {
            ReplaceLaraDependants(levelModels, definition.Model, laraDependants.Select(e => (short)e));
        }
    }

    private static void ReplaceLaraDependants(List<TRModel> models, TRModel lara, IEnumerable<short> entityIDs)
    {
        foreach (short dependant in entityIDs)
        {
            TRModel dependentModel = models.Find(m => m.ID == dependant);
            if (dependentModel != null)
            {
                dependentModel.MeshTree = lara.MeshTree;
                dependentModel.StartingMesh = lara.StartingMesh;
            }
        }
    }
}
