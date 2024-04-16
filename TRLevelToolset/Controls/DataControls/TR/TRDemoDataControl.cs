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
            ImGui.Text("Demo Data Size: " + IOManager.CurrentLevelAsTR1?.DemoData.Length);
            ImGui.TreePop();
        }
    }
}
