using System;
using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Model.Definitions;

namespace TRModelTransporter.Handlers
{
    public class ModelTransportHandler
    {
        public void Export(TRLevel level, TR1ModelDefinition definition, TREntities entity)
        {
            definition.Model = GetTRModel(level.Models, (short)entity);
        }

        public void Export(TR2Level level, TR2ModelDefinition definition, TR2Entities entity)
        {
            definition.Model = GetTRModel(level.Models, (short)entity);
        }

        public void Export(TR3Level level, TR3ModelDefinition definition, TR3Entities entity)
        {
            definition.Model = GetTRModel(level.Models, (short)entity);
        }

        private TRModel GetTRModel(IEnumerable<TRModel> models, short entityID)
        {
            TRModel model = models.ToList().Find(m => m.ID == entityID);
            if (model == null)
            {
                throw new ArgumentException(string.Format("The model for {0} could not be found.", entityID));
            }
            return model;
        }

        public void Import(TRLevel level, TR1ModelDefinition definition, Dictionary<TREntities, TREntities> aliasPriority, IEnumerable<TREntities> laraDependants)
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
                if (definition.Entity == TREntities.Lara)
                {
                    ReplaceLaraDependants(levelModels, definition.Model, laraDependants.Select(e => (short)e));
                }
                else if (laraDependants.Contains((TREntities)definition.Model.ID))
                {
                    ReplaceLaraDependants(levelModels, levelModels.Find(m => m.ID == (uint)TREntities.Lara), new short[] { (short)definition.Model.ID });
                }
            }
        }

        public void Import(TR2Level level, TR2ModelDefinition definition, Dictionary<TR2Entities, TR2Entities> aliasPriority, IEnumerable<TR2Entities> laraDependants)
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
            if (definition.Entity == TR2Entities.Lara && laraDependants != null)
            {
                ReplaceLaraDependants(levelModels, definition.Model, laraDependants.Select(e => (short)e));
            }
        }

        public void Import(TR3Level level, TR3ModelDefinition definition, Dictionary<TR3Entities, TR3Entities> aliasPriority, IEnumerable<TR3Entities> laraDependants, IEnumerable<TR3Entities> unsafeReplacements)
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

            if (definition.Entity == TR3Entities.Lara && laraDependants != null)
            {
                ReplaceLaraDependants(levelModels, definition.Model, laraDependants.Select(e => (short)e));
            }
        }

        private void ReplaceLaraDependants(List<TRModel> models, TRModel lara, IEnumerable<short> entityIDs)
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
}