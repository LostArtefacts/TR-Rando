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
    internal class TRSoundControl : IDrawable
    {
        public void Draw()
        {
            if (ImGui.TreeNodeEx("Sound Data", ImGuiTreeNodeFlags.OpenOnArrow))
            {
                ImGui.Text("Sound sources count: " + IOManager.CurrentLevelAsTR1?.NumSoundSources);
                ImGui.Text("Sound details count: " + IOManager.CurrentLevelAsTR1?.NumSoundDetails);
                ImGui.Text("Sound samples count: " + IOManager.CurrentLevelAsTR1?.NumSamples);
                ImGui.Text("Sound sample indices count: " + IOManager.CurrentLevelAsTR1?.NumSampleIndices);
                ImGui.Text("Sound map size: " + IOManager.CurrentLevelAsTR1?.SoundMap.Count());

                ImGui.TreePop();
            }
        }
    }
}
