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
    internal class TRPaletteControl : IDrawable
    {
        public void Draw()
        {
            if (ImGui.TreeNodeEx("Palette Data", ImGuiTreeNodeFlags.OpenOnArrow))
            {
                ImGui.Text("Light map Count: " + IOManager.CurrentLevelAsTR1?.LightMap.Count());
                ImGui.Text("Palette Count: " + IOManager.CurrentLevelAsTR1?.Palette.Count());
                ImGui.TreePop();
            }
        }
    }
}
