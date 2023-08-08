using ImGuiNET;
using TRLevelToolset.Interfaces;
using TRLevelToolset.IOLogic;

namespace TRLevelToolset.Controls.DataControls.TR;

internal class TRSpriteAndObjTextureControl : IDrawable
{
    public void Draw()
    {
        if (ImGui.TreeNodeEx("Sprite and Object Texture Data", ImGuiTreeNodeFlags.OpenOnArrow))
        {
            ImGui.Text("Object texture count: " + IOManager.CurrentLevelAsTR1?.NumObjectTextures);
            ImGui.Text("Sprite texture count: " + IOManager.CurrentLevelAsTR1?.NumSpriteTextures);
            ImGui.Text("Sprite sequence count: " + IOManager.CurrentLevelAsTR1?.NumSpriteSequences);

            ImGui.TreePop();
        }
    }
}
