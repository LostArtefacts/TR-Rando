using ImGuiNET;
using TRLevelToolset.Interfaces;
using TRLevelToolset.IOLogic;

namespace TRLevelToolset.Controls.DataControls.TR;

internal class TRRoomControl : IDrawable
{
    public void Draw()
    {      
        if (ImGui.TreeNodeEx("Room Data", ImGuiTreeNodeFlags.OpenOnArrow))
        {
            ImGui.Text("Number of Rooms: " + IOManager.CurrentLevelAsTR1?.Rooms.Count);

            ImGui.TreePop();
        }
    }
}
