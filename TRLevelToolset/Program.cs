using ImGuiNET;
using System.Numerics;
using Veldrid;
using Veldrid.StartupUtilities;

using TRLevelToolset.Controls;

using TRLevelReader.Helpers;

public static class Program
{
    static void Main(string[] args)
    {
        VeldridStartup.CreateWindowAndGraphicsDevice(
            new WindowCreateInfo(50, 50, 1280, 720, WindowState.Normal, "TR Level Toolset"),
            out var window,
            out var gd);

        var cl = gd.ResourceFactory.CreateCommandList();

        var imguiRenderer = new ImGuiRenderer(
            gd,
            gd.MainSwapchain.Framebuffer.OutputDescription,
            window.Width,
            window.Height);

        window.Resized += () =>
        {
            imguiRenderer.WindowResized(window.Width, window.Height);
            gd.MainSwapchain.Resize((uint)window.Width, (uint)window.Height);
        };

        LevelSelectListBox TR1Selector = new LevelSelectListBox { Items = TRLevelNames.AsListWithAssault.ToArray(), Game = TRLevelToolset.IOLogic.TRGame.TR1 };
        LevelSelectListBox TR1GSelector = new LevelSelectListBox { Items = TRLevelNames.AsListGold.ToArray(), Game = TRLevelToolset.IOLogic.TRGame.TR1 };
        LevelSelectListBox TR2Selector = new LevelSelectListBox { Items = TR2LevelNames.AsListWithAssault.ToArray(), Game = TRLevelToolset.IOLogic.TRGame.TR2 };
        LevelSelectListBox TR2GSelector = new LevelSelectListBox { Items = TR2LevelNames.AsListGold.ToArray(), Game = TRLevelToolset.IOLogic.TRGame.TR2 };
        LevelSelectListBox TR3Selector = new LevelSelectListBox { Items = TR3LevelNames.AsListWithAssault.ToArray(), Game = TRLevelToolset.IOLogic.TRGame.TR3 };
        LevelSelectListBox TR3GSelector = new LevelSelectListBox { Items = TR3LevelNames.AsListGold.ToArray(), Game = TRLevelToolset.IOLogic.TRGame.TR3 };

        while (window.Exists)
        {
            var snapshot = window.PumpEvents();
            imguiRenderer.Update(1f / 60f, snapshot);

            bool isMainWindowOpen = false;

            ImGui.SetNextWindowPos(new Vector2(0, 0));
            ImGui.SetNextWindowSize(new Vector2(1280, 720));

            ImGui.Begin("Tomb Raider Level Toolset", ref isMainWindowOpen);

            if (ImGui.TreeNodeEx("Level", ImGuiTreeNodeFlags.OpenOnArrow))
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
            
            ImGui.End();

            cl.Begin();
            cl.SetFramebuffer(gd.MainSwapchain.Framebuffer);
            cl.ClearColorTarget(0, new RgbaFloat(0, 0, 0.2f, 1f));
            imguiRenderer.Render(gd, cl);
            cl.End();
            gd.SubmitCommands(cl);
            gd.SwapBuffers(gd.MainSwapchain);
        }
    }
}


