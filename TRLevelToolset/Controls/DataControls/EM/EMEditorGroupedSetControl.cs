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
    internal class EMEditorGroupedSetControl : IDrawable
    {
        private EMEditorGroupedSet _data { get; set; }

        public EMEditorGroupedSetControl(EMEditorGroupedSet data)
        {
            _data = data;
        }
        
        public void Draw()
        {
            EMAllControl leader = new EMAllControl(_data.Leader);
            ImGui.Text("Leader");
            ImGui.Indent();
            leader.Draw();
            ImGui.Unindent();
            
            EMAnyControl followers = new EMAnyControl(_data.Followers);
            ImGui.Text("Followers");
            ImGui.Indent();
            followers.Draw();
            ImGui.Unindent();
        }
    }
}