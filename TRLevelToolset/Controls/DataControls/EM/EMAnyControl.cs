using ImGuiNET;
using TREnvironmentEditor.Model;
using TRLevelToolset.Interfaces;

namespace TRLevelToolset.Controls.DataControls.EM;

internal class EMAnyControl : IDrawable
{
    private List<EMEditorSet> _data { get; set; }

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
            
            EMAllControl ctrl = new EMAllControl(set);
            
            ImGui.Indent();
            ctrl.Draw();
            ImGui.Unindent();
            
            i++;
        }
    }
}