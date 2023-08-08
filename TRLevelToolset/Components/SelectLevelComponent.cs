using ImGuiNET;
using TRLevelControl.Helpers;
using TRLevelToolset.Controls;
using TRLevelToolset.Interfaces;

namespace TRLevelToolset.Components;

public class SelectLevelComponent : IDrawable
{
    readonly LevelSelectListBox TR1Selector = new() { Items = TR1LevelNames.AsListWithAssault.ToArray(), Game = IOLogic.TRGame.TR1 };
    readonly LevelSelectListBox TR1GSelector = new() { Items = TR1LevelNames.AsListGold.ToArray(), Game = IOLogic.TRGame.TR1 };
    readonly LevelSelectListBox TR2Selector = new() { Items = TR2LevelNames.AsListWithAssault.ToArray(), Game = IOLogic.TRGame.TR2 };
    readonly LevelSelectListBox TR2GSelector = new() { Items = TR2LevelNames.AsListGold.ToArray(), Game = IOLogic.TRGame.TR2 };
    readonly LevelSelectListBox TR3Selector = new() { Items = TR3LevelNames.AsListWithAssault.ToArray(), Game = IOLogic.TRGame.TR3 };
    readonly LevelSelectListBox TR3GSelector = new() { Items = TR3LevelNames.AsListGold.ToArray(), Game = IOLogic.TRGame.TR3 };

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
