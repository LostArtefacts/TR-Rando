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
    internal class TRFloorDataControl : IDrawable
    {
        public void Draw()
        {
            ImGui.Text("Floor Data Size: " + IOManager.CurrentLevelAsTR1?.NumFloorData + " uint16s.");
        }
    }
}
