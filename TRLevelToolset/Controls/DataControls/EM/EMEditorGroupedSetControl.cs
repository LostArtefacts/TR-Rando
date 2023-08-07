using ImGuiNET;
using TREnvironmentEditor.Model;
using TRLevelToolset.Interfaces;

namespace TRLevelToolset.Controls.DataControls.EM;

internal class EMEditorGroupedSetControl : IDrawable
{
    private EMEditorGroupedSet _data { get; set; }

    public EMEditorGroupedSetControl(EMEditorGroupedSet data)
    {
        _data = data;
    }
    
    public void Draw()
    {
        EMAllControl leader = new EMAllControl(_data.Leader);
        ImGui.Text("Leader");
        ImGui.Indent();
        leader.Draw();
        ImGui.Unindent();
        
        EMAnyControl followers = new EMAnyControl(_data.Followers);
        ImGui.Text("Followers");
        ImGui.Indent();
        followers.Draw();
        ImGui.Unindent();
    }
}