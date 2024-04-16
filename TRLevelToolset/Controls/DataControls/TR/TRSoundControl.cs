using ImGuiNET;
using TRLevelToolset.Interfaces;
using TRLevelToolset.IOLogic;

namespace TRLevelToolset.Controls.DataControls.TR;

internal class TRSoundControl : IDrawable
{
    public void Draw()
    {
        if (ImGui.TreeNodeEx("Sound Data", ImGuiTreeNodeFlags.OpenOnArrow))
        {
            ImGui.Text("Sound sources count: " + IOManager.CurrentLevelAsTR1?.SoundSources.Count);
            ImGui.Text("Sound details count: " + IOManager.CurrentLevelAsTR1?.SoundDetails.Count);
            ImGui.Text("Sound samples count: " + IOManager.CurrentLevelAsTR1?.Samples.Count);
            ImGui.Text("Sound sample indices count: " + IOManager.CurrentLevelAsTR1?.SampleIndices.Count);
            ImGui.Text("Sound map size: " + IOManager.CurrentLevelAsTR1?.SoundMap.Length);

            ImGui.TreePop();
        }
    }
}
