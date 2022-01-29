using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelToolset.Components
{
    internal class StringEditComponent
    {
        internal void Draw()
        {
            if (ImGui.TreeNodeEx("String & Globalization Editor", ImGuiTreeNodeFlags.Framed))
            {

                ImGui.TreePop();
            }
        }
    }
}
