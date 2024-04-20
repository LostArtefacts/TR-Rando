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
            ImGui.Text("Sound effects count: " + IOManager.CurrentLevelAsTR1?.SoundEffects.Count);

            ImGui.TreePop();
        }
    }
}
