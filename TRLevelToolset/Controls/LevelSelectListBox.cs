using ImGuiNET;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelToolset.IOLogic;

namespace TRLevelToolset.Controls
{
    internal class LevelSelectListBox
    {
        internal string[] Items { get; set; }
        internal int SelectedIndex { get; set; }
        internal TRGame Game { get; set; }

        internal LevelSelectListBox()
        {
            Items = new string[] { "Placeholder" };
            SelectedIndex = 0;
            Game = TRGame.TR1;
        }

        internal void Draw()
        {
            if (ImGui.BeginListBox(""))
            {
                for (int i = 0; i < Items.Count(); i++)
                {
                    bool isSelected = (SelectedIndex == i);

                    if (ImGui.Selectable(Items[i], isSelected))
                        SelectedIndex = i;

                    if (isSelected)
                        ImGui.SetItemDefaultFocus();
                }

                ImGui.EndListBox();
            }

            if (ImGui.Button("Load"))
            {
                Load();
            }
        }

        private void Load()
        {
            IOManager.Load(Items[SelectedIndex], Game);
        }
    }
}
