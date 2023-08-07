using ImGuiNET;
using TREnvironmentEditor.Model;
using TRLevelToolset.Interfaces;

namespace TRLevelToolset.Controls.DataControls.EM;

internal class EMAllWithinControl : IDrawable
{
    private List<List<EMEditorSet>> _data { get; set; }

    public EMAllWithinControl(List<List<EMEditorSet>> data)
    {
        _data = data;
    }
    
    public void Draw()
    {
        int i = 0;

        foreach (List<EMEditorSet> set in _data)
        {
            EMAllWithinGroupControl ctrl = new EMAllWithinGroupControl(set, i);

            ImGui.Text("Group " + i);

            ImGui.Indent();
            ctrl.Draw();
            ImGui.Unindent();

            i++;
        }
    }
}