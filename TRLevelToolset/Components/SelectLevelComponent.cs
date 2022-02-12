using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Helpers;
using TRLevelToolset.Controls;
using TRLevelToolset.Interfaces;

namespace TRLevelToolset.Components
{
    public class SelectLevelComponent : IDrawable
    {
        LevelSelectListBox TR1Selector = new LevelSelectListBox { Items = TRLevelNames.AsListWithAssault.ToArray(), Game = IOLogic.TRGame.TR1 };
        LevelSelectListBox TR1GSelector = new LevelSelectListBox { Items = TRLevelNames.AsListGold.ToArray(), Game = IOLogic.TRGame.TR1 };
        LevelSelectListBox TR2Selector = new LevelSelectListBox { Items = TR2LevelNames.AsListWithAssault.ToArray(), Game = IOLogic.TRGame.TR2 };
        LevelSelectListBox TR2GSelector = new LevelSelectListBox { Items = TR2LevelNames.AsListGold.ToArray(), Game = IOLogic.TRGame.TR2 };
        LevelSelectListBox TR3Selector = new LevelSelectListBox { Items = TR3LevelNames.AsListWithAssault.ToArray(), Game = IOLogic.TRGame.TR3 };
        LevelSelectListBox TR3GSelector = new LevelSelectListBox { Items = TR3LevelNames.AsListGold.ToArray(), Game = IOLogic.TRGame.TR3 };

        public void Draw()
        {
            if (ImGui.TreeNodeEx("Level Select Tree", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.Framed))
            {
                if (ImGui.TreeNodeEx("Tomb Raider I", ImGuiTreeNodeFlags.OpenOnArrow))
                {
                    TR1Selector.Draw();

                    ImGui.TreePop();
                }

                if (ImGui.TreeNodeEx("Tomb Raider Unfinished Business", ImGuiTreeNodeFlags.OpenOnArrow))
                {
                    TR1GSelector.Draw();

                    ImGui.TreePop();
                }

                if (ImGui.TreeNodeEx("Tomb Raider II", ImGuiTreeNodeFlags.OpenOnArrow))
                {
                    TR2Selector.Draw();

                    ImGui.TreePop();
                }

                if (ImGui.TreeNodeEx("Tomb Raider The Golden Mask", ImGuiTreeNodeFlags.OpenOnArrow))
                {
                    TR2GSelector.Draw();

                    ImGui.TreePop();
                }

                if (ImGui.TreeNodeEx("Tomb Raider III", ImGuiTreeNodeFlags.OpenOnArrow))
                {
                    TR3Selector.Draw();

                    ImGui.TreePop();
                }

                if (ImGui.TreeNodeEx("Tomb Raider The Lost Artefact", ImGuiTreeNodeFlags.OpenOnArrow))
                {
                    TR3GSelector.Draw();

                    ImGui.TreePop();
                }

                if (ImGui.TreeNodeEx("Tomb Raider IV", ImGuiTreeNodeFlags.OpenOnArrow))
                {
                    ImGui.TreePop();
                }

                if (ImGui.TreeNodeEx("Tomb Raider V", ImGuiTreeNodeFlags.OpenOnArrow))
                {
                    ImGui.TreePop();
                }

                ImGui.TreePop();
            }
        }
    }
}
