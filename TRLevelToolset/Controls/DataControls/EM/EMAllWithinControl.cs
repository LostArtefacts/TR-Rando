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
    internal class EMAllWithinControl : IDrawable
    {
        private List<List<EMEditorSet>> _data { get; set; }

        public EMAllWithinControl(List<List<EMEditorSet>> data)
        {
            _data = data;
        }
        
        public void Draw()
        {
            ImGui.Text("Conditional All Control");
        }
    }
}