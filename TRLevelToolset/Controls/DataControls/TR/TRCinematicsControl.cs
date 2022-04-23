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
    internal class TRCinematicsControl : IDrawable
    {
        public void Draw()
        {
            if (ImGui.TreeNodeEx("Cinematic Data", ImGuiTreeNodeFlags.OpenOnArrow))
            {
                ImGui.Text("Cinematic frame Count: " + IOManager.CurrentLevelAsTR1?.NumCinematicFrames);
                ImGui.TreePop();
            }
        }
    }
}
