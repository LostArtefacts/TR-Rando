using ImGuiNET;
using TRLevelToolset.Interfaces;
using TRLevelToolset.IOLogic;

namespace TRLevelToolset.Controls.DataControls.TR;

internal class TRFloorDataControl : IDrawable
{
    public void Draw()
    {
        if (ImGui.TreeNodeEx("Floor Data", ImGuiTreeNodeFlags.OpenOnArrow))
        {
            ImGui.Text("Floor Data Size: " + IOManager.CurrentLevelAsTR1?.NumFloorData + " uint16s.");

            ImGui.TreePop();
        }
    }
}
