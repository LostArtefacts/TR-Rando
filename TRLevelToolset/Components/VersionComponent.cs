using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ImGuiNET;
using TRLevelToolset.IOLogic;

namespace TRLevelToolset.Components
{
    internal class VersionComponent
    {
        internal void Draw()
        {
            ImGui.BeginChildFrame(0, new System.Numerics.Vector2(1280, 45));
            ImGui.Text("Level: " + IOManager.FileName);
            ImGui.Text("Version: " + IOManager.CurrentLevel?.Version.ToString("X8"));
            ImGui.EndChildFrame();
        }
    }
}
