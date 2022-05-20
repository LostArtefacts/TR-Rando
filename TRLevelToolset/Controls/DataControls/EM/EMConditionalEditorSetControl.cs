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
    internal class EMConditionalEditorSetControl : IDrawable
    {
        private EMConditionalEditorSet _data { get; set; }

        public EMConditionalEditorSetControl(EMConditionalEditorSet data)
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
                EMAnyControl trueList = new EMAnyControl(_data.OnTrue);
                ImGui.Text("On True");
                ImGui.Indent();
                trueList.Draw();
                ImGui.Unindent();
            }

            if (_data.OnFalse != null)
            {
                EMAnyControl falseList = new EMAnyControl(_data.OnFalse);
                ImGui.Text("On False");
                ImGui.Indent();
                falseList.Draw();
                ImGui.Unindent();
            }
        }
    }
}