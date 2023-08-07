using ImGuiNET;
using TRLevelToolset.Interfaces;

namespace TRLevelToolset.Components;

public class SFXEditComponent : IDrawable
{
    public void Draw()
    {
        if (ImGui.TreeNodeEx("SFX Resource Editor", ImGuiTreeNodeFlags.Framed))
        {

            ImGui.TreePop();
        }
    }
}
