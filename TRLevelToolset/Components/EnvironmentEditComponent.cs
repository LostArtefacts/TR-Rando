using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelToolset.Components
{
    internal class EnvironmentEditComponent
    {
        internal void Draw()
        {
            if (ImGui.TreeNodeEx("Environment Tools", ImGuiTreeNodeFlags.Framed))
            {

                ImGui.TreePop();
            }
        }
    }
}
