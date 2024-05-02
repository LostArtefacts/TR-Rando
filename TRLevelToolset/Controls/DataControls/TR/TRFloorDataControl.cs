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
            ImGui.Text("Floor Data Size: " + IOManager.CurrentLevelAsTR1?.FloorData.SelectMany(f => f.Value).Count() + " floor functions.");

            ImGui.TreePop();
        }
    }
}
