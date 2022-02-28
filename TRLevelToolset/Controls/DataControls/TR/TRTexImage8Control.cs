using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Model;
using TRLevelToolset.Interfaces;
using TRLevelToolset.IOLogic;

namespace TRLevelToolset.Controls.DataControls.TR
{
    internal class TRTexImage8Control : IDrawable
    {
        public void Draw()
        {
            ImGui.Text("Texture Count: " + IOManager.CurrentLevelAsTR1?.NumImages);
        }
    }
}
