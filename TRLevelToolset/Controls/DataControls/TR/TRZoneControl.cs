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
    internal class TRZoneControl : IDrawable
    {
        public void Draw()
        {
            if (ImGui.TreeNodeEx("Zone Data", ImGuiTreeNodeFlags.OpenOnArrow))
            {
                ImGui.Text("Box count: " + IOManager.CurrentLevelAsTR1?.NumBoxes);
                ImGui.Text("Overlap count: " + IOManager.CurrentLevelAsTR1?.NumOverlaps);
                ImGui.Text("Zone Group count: " + IOManager.CurrentLevelAsTR1?.Zones.Count());
                ImGui.TreePop();
            }
        }
    }
}
