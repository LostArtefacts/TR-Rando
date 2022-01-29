using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelToolset.Components
{
    internal class SFXEditComponent
    {
        internal void Draw()
        {
            if (ImGui.TreeNodeEx("SFX Resource Editor", ImGuiTreeNodeFlags.Framed))
            {

                ImGui.TreePop();
            }
        }
    }
}
