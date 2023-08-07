using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelControl.Model;
using TRLevelToolset.Interfaces;
using TRLevelToolset.IOLogic;

namespace TRLevelToolset.Controls.DataControls.TR;

internal class TRTexImage8Control : IDrawable
{
    public void Draw()
    {
        if (ImGui.TreeNodeEx("Texture Image 8 Data", ImGuiTreeNodeFlags.OpenOnArrow))
        {
            ImGui.Text("Texture Count: " + IOManager.CurrentLevelAsTR1?.NumImages);

            ImGui.TreePop();
        }
    }
}
