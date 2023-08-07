using ImGuiNET;
using TREnvironmentEditor.Model;
using TRLevelToolset.Interfaces;

namespace TRLevelToolset.Controls.DataControls.EM;

internal class EMConditionalAllWithinControl : IDrawable
{
    private List<EMConditionalEditorSet> _data { get; set; }

    public EMConditionalAllWithinControl(List<EMConditionalEditorSet> data)
    {
        _data = data;
    }
    
    public void Draw()
    {
        int i = 0;
        
        foreach (EMConditionalEditorSet set in _data)
        {
            ImGui.Text("Set " + i);
            ImGui.Indent();
            EMConditionalEditorSetControl ctrl = new EMConditionalEditorSetControl(set);
            ctrl.Draw();
            ImGui.Unindent();
            i++;
        }
    }
}