using ImGuiNET;
using TREnvironmentEditor.Model;
using TRLevelToolset.Interfaces;

namespace TRLevelToolset.Controls.DataControls.EM;

internal class EMConditionalEditorSetControl : IDrawable
{
    private readonly EMConditionalEditorSet _data;

    public EMConditionalEditorSetControl(EMConditionalEditorSet data)
    {
        _data = data;
    }
    
    public void Draw()
    {
        EMConditionControl condCtrl = new(_data.Condition);
        ImGui.Text("Condition");
        ImGui.Indent();
        condCtrl.Draw();

        if (_data.OnTrue != null)
        {
            EMAnyControl trueList = new(_data.OnTrue);
            ImGui.Text("On True");
            ImGui.Indent();
            trueList.Draw();
            ImGui.Unindent();
        }

        if (_data.OnFalse != null)
        {
            EMAnyControl falseList = new(_data.OnFalse);
            ImGui.Text("On False");
            ImGui.Indent();
            falseList.Draw();
            ImGui.Unindent();
        }
    }
}