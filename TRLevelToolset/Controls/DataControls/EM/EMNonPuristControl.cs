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
    internal class EMNonPuristControl : IDrawable
    {
        private EMEditorSet _data { get; set; }
        private List<EMFunctionControl> _funcControls { get; set; }

        public EMNonPuristControl(EMEditorSet data)
        {
            _data = data;
            _funcControls = new List<EMFunctionControl>();

            for (int i = 0; i < _data.Count; i++)
            {
                _funcControls.Add(new EMFunctionControl(_data[i]));
            }
        }
        
        public void Draw()
        {
            foreach (var control in _funcControls)
            {
                control.Draw();
            }
        }
    }
}