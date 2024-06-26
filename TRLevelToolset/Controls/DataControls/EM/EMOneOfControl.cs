﻿using ImGuiNET;
using TRDataControl.Environment;
using TRLevelToolset.Interfaces;

namespace TRLevelToolset.Controls.DataControls.EM;

internal class EMOneOfControl : IDrawable
{
    private readonly List<EMEditorGroupedSet> _data;

    public EMOneOfControl(List<EMEditorGroupedSet> data)
    {
        _data = data;
    }

    public void Draw()
    {
        int i = 0;
        
        foreach (EMEditorGroupedSet set in _data)
        {
            ImGui.Text("Grouped Set " + i);
            ImGui.Indent();
            EMEditorGroupedSetControl ctrl = new(set);
            ctrl.Draw();
            ImGui.Unindent();
            i++;
        }
    }
}