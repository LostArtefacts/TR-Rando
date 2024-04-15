using ImGuiNET;
using TRLevelToolset.Interfaces;
using TRLevelToolset.IOLogic;

namespace TRLevelToolset.Controls.DataControls.TR;

internal class TRModelControl : IDrawable
{
    public void Draw()
    {
        if (ImGui.TreeNodeEx("Model Data", ImGuiTreeNodeFlags.OpenOnArrow))
        {
            ImGui.Text("Model count: " + IOManager.CurrentLevelAsTR1?.Models.Count);

            ImGui.TreePop();
        }
    }
}
