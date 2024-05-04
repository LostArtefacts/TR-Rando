using ImGuiNET;
using TRDataControl.Environment;
using TRLevelToolset.Interfaces;

namespace TRLevelToolset.Controls.DataControls.EM;

internal class EMAnyControl : IDrawable
{
    private readonly List<EMEditorSet> _data;

    public EMAnyControl(List<EMEditorSet> data)
    {
        _data = data;
    }
    
    public void Draw()
    {
        int i = 0;
        
        foreach (EMEditorSet set in _data)
        {
            ImGui.Text("Set " + i);
            
            EMAllControl ctrl = new(set);
            
            ImGui.Indent();
            ctrl.Draw();
            ImGui.Unindent();
            
            i++;
        }
    }
}