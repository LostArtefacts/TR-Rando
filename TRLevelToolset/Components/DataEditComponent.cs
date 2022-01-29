using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelToolset.Components
{
    internal class DataEditComponent
    {
        internal void Draw()
        {
            if (ImGui.TreeNodeEx("Level Data Editor", ImGuiTreeNodeFlags.Framed))
            {

                ImGui.TreePop();
            }
        }
    }
}
