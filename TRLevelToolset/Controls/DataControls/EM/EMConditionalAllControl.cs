using ImGuiNET;
using TREnvironmentEditor.Model;
using TRLevelToolset.Interfaces;

namespace TRLevelToolset.Controls.DataControls.EM;

internal class EMConditionalAllControl : IDrawable
{
    private readonly List<EMConditionalSingleEditorSet> _data;

    public EMConditionalAllControl(List<EMConditionalSingleEditorSet> data)
    {
        _data = data;
    }
    
    public void Draw()
    {
        int i = 0;
        
        foreach (EMConditionalSingleEditorSet set in _data)
        {
            ImGui.Text("Set " + i);
            ImGui.Indent();
            EMConditionalSingleEditorSetControl ctrl = new(set);
            ctrl.Draw();
            ImGui.Unindent();
            i++;
        }
    }
}