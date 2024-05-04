using ImGuiNET;
using TRDataControl.Environment;
using TRLevelToolset.Interfaces;

namespace TRLevelToolset.Controls.DataControls.EM;

internal class EMConditionalAllWithinControl : IDrawable
{
    private readonly List<EMConditionalEditorSet> _data;

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
            EMConditionalEditorSetControl ctrl = new(set);
            ctrl.Draw();
            ImGui.Unindent();
            i++;
        }
    }
}