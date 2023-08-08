using ImGuiNET;
using TREnvironmentEditor.Model;
using TRLevelToolset.Interfaces;

namespace TRLevelToolset.Controls.DataControls.EM;

internal class EMEditorGroupedSetControl : IDrawable
{
    private readonly EMEditorGroupedSet _data;

    public EMEditorGroupedSetControl(EMEditorGroupedSet data)
    {
        _data = data;
    }
    
    public void Draw()
    {
        EMAllControl leader = new(_data.Leader);
        ImGui.Text("Leader");
        ImGui.Indent();
        leader.Draw();
        ImGui.Unindent();
        
        EMAnyControl followers = new(_data.Followers);
        ImGui.Text("Followers");
        ImGui.Indent();
        followers.Draw();
        ImGui.Unindent();
    }
}