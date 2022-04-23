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
    internal class TRCamerasControl : IDrawable
    {
        public void Draw()
        {
            if (ImGui.TreeNodeEx("Camera Data", ImGuiTreeNodeFlags.OpenOnArrow))
            {
                ImGui.Text("Camera count: " + IOManager.CurrentLevelAsTR1?.NumCameras);

                ImGui.TreePop();
            }
        }
    }
}
