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
    internal class TRANimationControl : IDrawable
    {
        public void Draw()
        {
            if (ImGui.TreeNodeEx("Animations Data", ImGuiTreeNodeFlags.OpenOnArrow))
            {
                ImGui.Text("Animations count: " + IOManager.CurrentLevelAsTR1?.NumAnimations);
                ImGui.Text("State change count: " + IOManager.CurrentLevelAsTR1?.NumStateChanges);
                ImGui.Text("Animation dispatch count: " + IOManager.CurrentLevelAsTR1?.NumAnimations);
                ImGui.Text("Animation command count: " + IOManager.CurrentLevelAsTR1?.NumAnimCommands);
                ImGui.Text("Mesh tree count: " + IOManager.CurrentLevelAsTR1?.NumMeshTrees);
                ImGui.Text("Total frames count: " + IOManager.CurrentLevelAsTR1?.NumFrames);

                ImGui.TreePop();
            }
        }
    }
}
