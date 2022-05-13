using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TREnvironmentEditor.Model;
using TRLevelReader.Model;
using TRLevelToolset.Interfaces;
using TRLevelToolset.IOLogic;

namespace TRLevelToolset.Controls.DataControls.EM
{
    internal class EMConditionalSingleEditorSetControl : IDrawable
    {
        private EMConditionalSingleEditorSet _data { get; set; }

        public EMConditionalSingleEditorSetControl(EMConditionalSingleEditorSet data)
        {
            _data = data;
        }
        
        public void Draw()
        {
            EMConditionControl condCtrl = new EMConditionControl(_data.Condition);
            ImGui.Text("Condition");
            ImGui.Indent();
            condCtrl.Draw();

            if (_data.OnTrue != null)
            {
                EMAllControl trueList = new EMAllControl(_data.OnTrue);
                ImGui.Text("On True");
                ImGui.Indent();
                trueList.Draw();
                ImGui.Unindent();
            }

            if (_data.OnFalse != null)
            {
                EMAllControl falseList = new EMAllControl(_data.OnFalse);
                ImGui.Text("On False");
                ImGui.Indent();
                falseList.Draw();
                ImGui.Unindent();
            }
            
            ImGui.Unindent();
        }
    }
}