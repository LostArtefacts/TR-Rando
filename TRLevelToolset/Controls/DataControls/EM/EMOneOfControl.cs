using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TREnvironmentEditor.Model;
using TRLevelControl.Model;
using TRLevelToolset.Interfaces;
using TRLevelToolset.IOLogic;

namespace TRLevelToolset.Controls.DataControls.EM
{
    internal class EMOneOfControl : IDrawable
    {
        private List<EMEditorGroupedSet> _data { get; set; }

        public EMOneOfControl(List<EMEditorGroupedSet> data)
        {
            _data = data;
        }

        public void Draw()
        {
            int i = 0;
            
            foreach (EMEditorGroupedSet set in _data)
            {
                ImGui.Text("Grouped Set " + i);
                ImGui.Indent();
                EMEditorGroupedSetControl ctrl = new EMEditorGroupedSetControl(set);
                ctrl.Draw();
                ImGui.Unindent();
                i++;
            }
        }
    }
}