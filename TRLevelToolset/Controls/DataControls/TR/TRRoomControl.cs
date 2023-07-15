using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelControl.Model;
using TRLevelToolset.Interfaces;
using TRLevelToolset.IOLogic;

namespace TRLevelToolset.Controls.DataControls.TR
{
    internal class TRRoomControl : IDrawable
    {
        public void Draw()
        {      
            if (ImGui.TreeNodeEx("Room Data", ImGuiTreeNodeFlags.OpenOnArrow))
            {
                ImGui.Text("Number of Rooms: " + IOManager.CurrentLevelAsTR1?.NumRooms);

                ImGui.TreePop();
            }
        }
    }
}
