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
    internal class EMOneOfControl : IDrawable
    {
        private List<EMEditorGroupedSet> _data { get; set; }

        public EMOneOfControl(List<EMEditorGroupedSet> data)
        {
            _data = data;
        }

        public void Draw()
        {
            ImGui.Text("One Of Control");
        }
    }
}