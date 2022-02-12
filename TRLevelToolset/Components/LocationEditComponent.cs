using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelToolset.Interfaces;

namespace TRLevelToolset.Components
{
    public class LocationEditComponent : IDrawable
    {
        public void Draw()
        {
            if (ImGui.TreeNodeEx("Locations Editor", ImGuiTreeNodeFlags.Framed))
            {

                ImGui.TreePop();
            }
        }
    }
}
