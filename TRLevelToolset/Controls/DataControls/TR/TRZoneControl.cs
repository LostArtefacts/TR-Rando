using ImGuiNET;
using TRLevelToolset.Interfaces;
using TRLevelToolset.IOLogic;

namespace TRLevelToolset.Controls.DataControls.TR;

internal class TRZoneControl : IDrawable
{
    public void Draw()
    {
        if (ImGui.TreeNodeEx("Zone Data", ImGuiTreeNodeFlags.OpenOnArrow))
        {
            ImGui.Text("Box count: " + IOManager.CurrentLevelAsTR1?.Boxes.Count);
            ImGui.Text("Overlap count: " + IOManager.CurrentLevelAsTR1?.Boxes.Sum(b => b.Overlaps.Count));
            ImGui.Text("Zone Group count: " + IOManager.CurrentLevelAsTR1?.Boxes.Count);
            ImGui.TreePop();
        }
    }
}
