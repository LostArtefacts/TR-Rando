using ImGuiNET;
using TRLevelToolset.Interfaces;

namespace TRLevelToolset.Components;

public class StringEditComponent : IDrawable
{
    public void Draw()
    {
        if (ImGui.TreeNodeEx("String & Globalization Editor", ImGuiTreeNodeFlags.Framed))
        {

            ImGui.TreePop();
        }
    }
}
