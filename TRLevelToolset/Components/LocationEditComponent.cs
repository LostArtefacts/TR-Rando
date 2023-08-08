using ImGuiNET;
using TRLevelToolset.Interfaces;

namespace TRLevelToolset.Components;

public class LocationEditComponent : IDrawable
{
    public void Draw()
    {
        if (ImGui.TreeNodeEx("Locations Editor", ImGuiTreeNodeFlags.Framed))
        {

            ImGui.TreePop();
        }
    }
}
