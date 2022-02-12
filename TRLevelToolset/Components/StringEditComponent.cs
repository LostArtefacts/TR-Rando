using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelToolset.Interfaces;

namespace TRLevelToolset.Components
{
    public class StringEditComponent : IDrawable
    {
        public void Draw()
        {
            if (ImGui.TreeNodeEx("String & Globalization Editor", ImGuiTreeNodeFlags.Framed))
            {

                ImGui.TreePop();
            }
        }
    }
}
