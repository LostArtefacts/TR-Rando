using ImGuiNET;
using TRLevelToolset.Interfaces;
using TRLevelToolset.IOLogic;

namespace TRLevelToolset.Controls.DataControls.TR;

internal class TREntityControl : IDrawable
{
    public void Draw()
    {
        if (ImGui.TreeNodeEx("Entity Data", ImGuiTreeNodeFlags.OpenOnArrow))
        {
            ImGui.Text("Entity count: " + IOManager.CurrentLevelAsTR1?.NumEntities);

            ImGui.TreePop();
        }
    }
}
