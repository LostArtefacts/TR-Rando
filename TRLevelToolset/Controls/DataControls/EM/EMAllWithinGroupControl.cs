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
    internal class EMAllWithinGroupControl : IDrawable
    {
        private List<EMEditorSet> _data { get; set; }
        
        private int _groupNum { get; set; }

        public EMAllWithinGroupControl(List<EMEditorSet> data, int groupNum)
        {
            _data = data;
            _groupNum = groupNum;
        }
        
        public void Draw()
        {
            int i = 0;
            
            foreach (EMEditorSet set in _data)
            {
                ImGui.Text("Group  " + _groupNum + " Set " + i);
                
                EMAllControl ctrl = new EMAllControl(set);
                
                ImGui.Indent();
                ctrl.Draw();
                ImGui.Unindent();
                
                i++;
            }
        }
    }
}