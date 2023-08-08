using ImGuiNET;
using TRLevelToolset.Interfaces;
using TRLevelToolset.IOLogic;

namespace TRLevelToolset.Controls.DataControls.TR;

internal class TRStaticMeshControls : IDrawable
{
    public void Draw()
    {
        if (ImGui.TreeNodeEx("Static Mesh Data", ImGuiTreeNodeFlags.OpenOnArrow))
        {
            ImGui.Text("Static mesh count: " + IOManager.CurrentLevelAsTR1?.NumStaticMeshes);

            ImGui.TreePop();
        }
    }
}
