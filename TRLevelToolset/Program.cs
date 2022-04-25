using ImGuiNET;
using System.Numerics;
using Veldrid;
using Veldrid.StartupUtilities;

using TRLevelToolset.Components;
using System.Diagnostics;

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

        SelectLevelComponent selectLevelScreen = new SelectLevelComponent();
        VersionComponent versionComponent = new VersionComponent();
        DataEditComponent dataEditComponent = new DataEditComponent();
        EnvironmentEditComponent envComponent = new EnvironmentEditComponent();

        Stopwatch sw = new Stopwatch();

        while (window.Exists)
        {
            var snapshot = window.PumpEvents();
            imguiRenderer.Update(1f / 60f, snapshot);

            bool isMainWindowOpen = false;

            ImGui.SetNextWindowPos(new Vector2(0, 0));
            ImGui.SetNextWindowSize(new Vector2(1280, 720));
            
            sw.Start();

            ImGui.Begin("Tomb Raider Level Toolset", ref isMainWindowOpen);
            
            sw.Stop();

            selectLevelScreen.Draw();
            versionComponent.Draw();
            dataEditComponent.Draw();
            envComponent.Draw();

            sw.Restart();
            Thread.Sleep((int)(16 - sw.Elapsed.TotalMilliseconds));
#if DEBUG
            ImGui.Text("Draw Time: " + sw.Elapsed.TotalMilliseconds + " ms");
#endif
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


