using ImGuiNET;
using TRLevelToolset.Interfaces;
using TRLevelToolset.IOLogic;

namespace TRLevelToolset.Controls.DataControls.TR;

internal class TRMeshControl : IDrawable
{
    public void Draw()
    {
        if (ImGui.TreeNodeEx("Mesh Data", ImGuiTreeNodeFlags.OpenOnArrow))
        {
            ImGui.Text("Mesh count: " + IOManager.CurrentLevelAsTR1?.DistinctMeshes.Count());

            ImGui.TreePop();
        }
    }
}
