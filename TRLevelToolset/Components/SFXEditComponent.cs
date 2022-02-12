using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelToolset.Interfaces;

namespace TRLevelToolset.Components
{
    public class SFXEditComponent : IDrawable
    {
        public void Draw()
        {
            if (ImGui.TreeNodeEx("SFX Resource Editor", ImGuiTreeNodeFlags.Framed))
            {

                ImGui.TreePop();
            }
        }
    }
}
