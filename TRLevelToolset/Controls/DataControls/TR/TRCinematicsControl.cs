using ImGuiNET;
using TRLevelToolset.Interfaces;
using TRLevelToolset.IOLogic;

namespace TRLevelToolset.Controls.DataControls.TR;

internal class TRCinematicsControl : IDrawable
{
    public void Draw()
    {
        if (ImGui.TreeNodeEx("Cinematic Data", ImGuiTreeNodeFlags.OpenOnArrow))
        {
            ImGui.Text("Cinematic frame Count: " + IOManager.CurrentLevelAsTR1?.CinematicFrames.Count);
            ImGui.TreePop();
        }
    }
}
