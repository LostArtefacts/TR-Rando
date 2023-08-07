using ImGuiNET;
using TREnvironmentEditor.Model;
using TRLevelToolset.Interfaces;

namespace TRLevelToolset.Controls.DataControls.EM;

internal class EMAllWithinControl : IDrawable
{
    private readonly List<List<EMEditorSet>> _data;

    public EMAllWithinControl(List<List<EMEditorSet>> data)
    {
        _data = data;
    }
    
    public void Draw()
    {
        int i = 0;

        foreach (List<EMEditorSet> set in _data)
        {
            EMAllWithinGroupControl ctrl = new(set, i);

            ImGui.Text("Group " + i);

            ImGui.Indent();
            ctrl.Draw();
            ImGui.Unindent();

            i++;
        }
    }
}