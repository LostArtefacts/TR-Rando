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
            ImGui.Text("Mesh count: " + IOManager.CurrentLevelAsTR1?.Models.SelectMany(m => m.Meshes)
                .Concat(IOManager.CurrentLevelAsTR1.StaticMeshes.Select(s => s.Mesh)).Count());

            ImGui.TreePop();
        }
    }
}
