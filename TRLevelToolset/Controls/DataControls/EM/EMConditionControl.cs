using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TREnvironmentEditor.Model;
using TRLevelReader.Model;
using TRLevelToolset.Interfaces;
using TRLevelToolset.IOLogic;

namespace TRLevelToolset.Controls.DataControls.EM
{
    internal class EMConditionControl : IDrawable
    {
        private BaseEMCondition _data { get; set; }

        public EMConditionControl(BaseEMCondition data)
        {
            _data = data;
        }
        
        public void Draw()
        {
            ImGui.Button(_data.GetType().Name);
        }
    }
}