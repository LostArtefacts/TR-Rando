using ImGuiNET;
using TRLevelToolset.Interfaces;
using TRLevelToolset.IOLogic;

namespace TRLevelToolset.Controls.DataControls.TR;

internal class TRAnimatedTextureControl : IDrawable
{
    public void Draw()
    {
        if (ImGui.TreeNodeEx("Animated Textures", ImGuiTreeNodeFlags.OpenOnArrow))
        {
            ImGui.Text("Animated Texture Count: " + IOManager.CurrentLevelAsTR1?.NumAnimatedTextures);

            ImGui.TreePop();
        }
    }
}
