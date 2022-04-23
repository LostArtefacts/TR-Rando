using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Model;
using TRLevelToolset.Interfaces;
using TRLevelToolset.IOLogic;

namespace TRLevelToolset.Controls.DataControls.TR
{
    internal class TRModelControl : IDrawable
    {
        public void Draw()
        {
            if (ImGui.TreeNodeEx("Model Data", ImGuiTreeNodeFlags.OpenOnArrow))
            {
                ImGui.Text("Model count: " + IOManager.CurrentLevelAsTR1?.NumModels);

                ImGui.TreePop();
            }
        }
    }
}
