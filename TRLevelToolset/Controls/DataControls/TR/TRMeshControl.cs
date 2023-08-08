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
            ImGui.Text("Mesh data count: " + IOManager.CurrentLevelAsTR1?.NumMeshData);
            ImGui.Text("Mesh pointer count: " + IOManager.CurrentLevelAsTR1?.NumMeshPointers);

            ImGui.TreePop();
        }
    }
}
