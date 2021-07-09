using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TRModelTransporter.Handlers
{
    public class ModelTransportHandler : AbstractTransportHandler
    {
        // These will not overwrite existing models if they already exist.
        private static readonly List<TR2Entities> _ignoreEntities = new List<TR2Entities>
        {
            TR2Entities.LaraMiscAnim_H
        };

        // These are models that use Lara's hips as placeholders - see Import
        private static readonly List<TR2Entities> _laraDependentModels = new List<TR2Entities>
        {
            TR2Entities.CameraTarget_N, TR2Entities.FlameEmitter_N, TR2Entities.LaraCutscenePlacement_N,
            TR2Entities.DragonExplosionEmitter_N, TR2Entities.BartoliHideoutClock_N, TR2Entities.SingingBirds_N,
            TR2Entities.WaterfallMist_N, TR2Entities.DrippingWater_N, TR2Entities.LavaAirParticleEmitter_N,
            TR2Entities.AlarmBell_N
        };

        public TR2Entities ModelEntity { get; set; }

        public override void Export()
        {
            short entityID = (short)ModelEntity;
            Definition.Model = Level.Models[Level.Models.ToList().FindIndex(m => m.ID == entityID)];
        }

        public override void Import()
        {
            List<TRModel> levelModels = Level.Models.ToList();
            int i = levelModels.FindIndex(m => m.ID == (short)Definition.Entity);
            if (i == -1)
            {
                levelModels.Add(Definition.Model);
                Level.Models = levelModels.ToArray();
                Level.NumModels++;
            }
            else if (!_ignoreEntities.Contains(Definition.Entity))
            {
                // Replacement occurs for the likes of aliases taking the place of another
                // e.g. WhiteTiger replacing BengalTiger in GW
                Level.Models[i] = Definition.Model;
            }

            // If we have replaced Lara, we need to update models such as CameraTarget, FlameEmitter etc
            // as these use Lara's hips as placeholders. This means we can avoid texture corruption in
            // TRView but it's also needed for the shower cutscene in HSH. If these entities are found,
            // their starting mesh and mesh tree indices are just remapped to Lara's.
            if (Definition.Entity == TR2Entities.Lara)
            {
                foreach (TR2Entities dependent in _laraDependentModels)
                {
                    TRModel dependentModel = levelModels.Find(m => m.ID == (short)dependent);
                    if (dependentModel != null)
                    {
                        dependentModel.MeshTree = Definition.Model.MeshTree;
                        dependentModel.StartingMesh = Definition.Model.StartingMesh;
                    }
                }
            }
        }
    }
}