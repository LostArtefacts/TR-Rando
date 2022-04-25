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
    internal class EMConditionalAllControl : IDrawable
    {
        public List<EMConditionalSingleEditorSet> Data { get; set; }
        
        public void Draw()
        {
            ImGui.Text("Conditional All Control");
        }
    }
}