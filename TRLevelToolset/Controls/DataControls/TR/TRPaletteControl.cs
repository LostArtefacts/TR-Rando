using ImGuiNET;
using TRLevelToolset.Interfaces;
using TRLevelToolset.IOLogic;

namespace TRLevelToolset.Controls.DataControls.TR;

internal class TRPaletteControl : IDrawable
{
    public void Draw()
    {
        if (ImGui.TreeNodeEx("Palette Data", ImGuiTreeNodeFlags.OpenOnArrow))
        {
            ImGui.Text("Light map Count: " + IOManager.CurrentLevelAsTR1?.LightMap.Length);
            ImGui.Text("Palette Count: " + IOManager.CurrentLevelAsTR1?.Palette.Count);
            ImGui.TreePop();
        }
    }
}
