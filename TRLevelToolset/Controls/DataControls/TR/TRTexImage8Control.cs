﻿using ImGuiNET;
using TRLevelToolset.Interfaces;
using TRLevelToolset.IOLogic;

namespace TRLevelToolset.Controls.DataControls.TR;

internal class TRTexImage8Control : IDrawable
{
    public void Draw()
    {
        if (ImGui.TreeNodeEx("Texture Image 8 Data", ImGuiTreeNodeFlags.OpenOnArrow))
        {
            ImGui.Text("Texture Count: " + IOManager.CurrentLevelAsTR1?.Images8.Count);

            ImGui.TreePop();
        }
    }
}
