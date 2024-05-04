using System.Diagnostics;
using TRLevelControl.Model;
using TRModelTransporter.Model.Definitions;

namespace TRModelTransporter.Handlers;

public class ModelTransportHandler
{
    public static void Export(TR1Level level, TR1Blob definition, TR1Type entity)
    {
        definition.Model = level.Models[entity];
    }

    public static void Export(TR2Level level, TR2Blob definition, TR2Type entity)
    {
        definition.Model = level.Models[entity];
    }

    public static void Export(TR3Level level, TR3Blob definition, TR3Type entity)
    {
        definition.Model = level.Models[entity];
    }

    public static void Import(TR1Level level, TR1Blob definition, Dictionary<TR1Type, TR1Type> aliasPriority, IEnumerable<TR1Type> laraDependants)
    {
        if (!level.Models.ContainsKey(definition.Entity))
        {
            level.Models[definition.Entity] = definition.Model;
        }
        else if (!aliasPriority.ContainsKey(definition.Entity) || aliasPriority[definition.Entity] == definition.Alias)
        {
            if (!definition.HasGraphics)
            {
                // The original mesh data may still be needed so don't overwrite
                definition.Model.MeshTrees = level.Models[definition.Entity].MeshTrees;
                definition.Model.Meshes = level.Models[definition.Entity].Meshes;
            }
            level.Models[definition.Entity] = definition.Model;
        }

        if (laraDependants != null)
        {
            if (definition.Entity == TR1Type.Lara)
            {
                ReplaceLaraDependants(level.Models, definition.Model, laraDependants);
            }
            else if (laraDependants.Contains(definition.Entity))
            {
                ReplaceLaraDependants(level.Models, level.Models[TR1Type.Lara], new TR1Type[] { definition.Entity });
            }
        }
    }

    public static void Import(TR2Level level, TR2Blob definition, Dictionary<TR2Type, TR2Type> aliasPriority, IEnumerable<TR2Type> laraDependants)
    {
        if (!level.Models.ContainsKey(definition.Entity))
        {
            level.Models[definition.Entity] = definition.Model;
        }
        else if (!aliasPriority.ContainsKey(definition.Entity) || aliasPriority[definition.Entity] == definition.Alias)
        {
            // Replacement occurs for the likes of aliases taking the place of another
            // e.g. WhiteTiger replacing BengalTiger in GW, or if we have a specific
            // alias that should always have a higher priority than its peers.
            level.Models[definition.Entity] = definition.Model;
        }

        // If we have replaced Lara, we need to update models such as CameraTarget, FlameEmitter etc
        // as these use Lara's hips as placeholders. This means we can avoid texture corruption in
        // TRView but it's also needed for the shower cutscene in HSH. If these entities are found,
        // their starting mesh and mesh tree indices are just remapped to Lara's.
        if (definition.Entity == TR2Type.Lara && laraDependants != null)
        {
            ReplaceLaraDependants(level.Models, definition.Model, laraDependants);
        }
    }

    public static void Import(TR3Level level, TR3Blob definition, Dictionary<TR3Type, TR3Type> aliasPriority, IEnumerable<TR3Type> laraDependants, IEnumerable<TR3Type> unsafeReplacements)
    {
        if (!level.Models.ContainsKey(definition.Entity))
        {
            level.Models[definition.Entity] = definition.Model;
        }
        else if (!aliasPriority.ContainsKey(definition.Entity) || aliasPriority[definition.Entity] == definition.Alias)
        {
            if (!unsafeReplacements.Contains(definition.Entity))
            {
                level.Models[definition.Entity] = definition.Model;
            }
            else
            {
                // #234 Replacing Lara entirely can cause locking issues after pressing buttons or crouching
                // where she refuses to come out of her stance. TR3 seems bound to having Lara's animations start
                // at 0, so because these don't change per skin, we just replace the meshes and frames here.
                level.Models[definition.Entity].Meshes = definition.Model.Meshes;
                level.Models[definition.Entity].MeshTrees = definition.Model.MeshTrees;
            }
        }

        if (definition.Entity == TR3Type.Lara && laraDependants != null)
        {
            ReplaceLaraDependants(level.Models, definition.Model, laraDependants);
        }
    }

    private static void ReplaceLaraDependants<T>(SortedDictionary<T, TRModel> models, TRModel lara, IEnumerable<T> entityIDs)
        where T : Enum
    {
        foreach (T dependant in entityIDs)
        {
            models.TryGetValue(dependant, out TRModel dependentModel);
            if (dependentModel != null)
            {
                Debug.Assert(dependentModel.Meshes.Count == 1);
                dependentModel.MeshTrees = lara.MeshTrees;
                dependentModel.Meshes = new() { lara.Meshes.First() };
            }
        }
    }
}
