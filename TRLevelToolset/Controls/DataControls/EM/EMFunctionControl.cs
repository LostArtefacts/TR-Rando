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
    internal class EMFunctionControl : IDrawable
    {
        private BaseEMFunction _data { get; set; }

        public EMFunctionControl(BaseEMFunction data)
        {
            _data = data;
        }
        
        public void Draw()
        {
            ImGui.Button(_data.GetType().Name);
        }
    }
}