using ImGuiNET;
using TRLevelToolset.Interfaces;
using TRLevelToolset.IOLogic;

namespace TRLevelToolset.Controls.DataControls.TR;

internal class TRDemoDataControl : IDrawable
{
    public void Draw()
    {
        if (ImGui.TreeNodeEx("Demo Data", ImGuiTreeNodeFlags.OpenOnArrow))
        {
            ImGui.Text("Has Demo Data: " + IOManager.CurrentLevelAsTR1?.DemoData == null ? "No" : "Yes");
            ImGui.TreePop();
        }
    }
}
