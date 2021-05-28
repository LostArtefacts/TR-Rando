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
        }
    }
}