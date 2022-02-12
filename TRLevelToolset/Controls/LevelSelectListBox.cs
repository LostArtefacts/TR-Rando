using ImGuiNET;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelToolset.Interfaces;
using TRLevelToolset.IOLogic;

namespace TRLevelToolset.Controls
{
    public class LevelSelectListBox  : IDrawable
    {
        public string[] Items { get; set; }
        public int SelectedIndex { get; set; }
        public TRGame Game { get; set; }

        public LevelSelectListBox()
        {
            Items = new string[] { "Placeholder" };
            SelectedIndex = 0;
            Game = TRGame.TR1;
        }

        public void Draw()
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
