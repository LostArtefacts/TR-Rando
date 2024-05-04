using ImGuiNET;
using TRDataControl.Environment;
using TRLevelToolset.Interfaces;

namespace TRLevelToolset.Controls.DataControls.EM;

internal class EMConditionalSingleEditorSetControl : IDrawable
{
    private readonly EMConditionalSingleEditorSet _data;

    public EMConditionalSingleEditorSetControl(EMConditionalSingleEditorSet data)
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
            EMAllControl trueList = new(_data.OnTrue);
            ImGui.Text("On True");
            ImGui.Indent();
            trueList.Draw();
            ImGui.Unindent();
        }

        if (_data.OnFalse != null)
        {
            EMAllControl falseList = new(_data.OnFalse);
            ImGui.Text("On False");
            ImGui.Indent();
            falseList.Draw();
            ImGui.Unindent();
        }
        
        ImGui.Unindent();
    }
}