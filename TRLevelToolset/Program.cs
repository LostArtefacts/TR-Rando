using ImGuiNET;
using Veldrid;
using Veldrid.StartupUtilities;

public static class Program
{
    static void Main(string[] args)
    {
        VeldridStartup.CreateWindowAndGraphicsDevice(
            new WindowCreateInfo(50, 50, 960, 540, WindowState.Normal, "TR Level Toolset"),
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

        while (window.Exists)
        {
            var snapshot = window.PumpEvents();
            imguiRenderer.Update(1f / 60f, snapshot);

            // Draw whatever you want here.
            if (ImGui.Begin("Hello Rando"))
            {
                ImGui.Text("Hello Rando");
                if (ImGui.Button("Quit"))
                {
                    window.Close();
                }
            }

            cl.Begin();
            cl.SetFramebuffer(gd.MainSwapchain.Framebuffer);
            cl.ClearColorTarget(0, new RgbaFloat(0, 0, 0.2f, 1f));

            //ImGUI Stuff here

            imguiRenderer.Render(gd, cl);
            cl.End();
            gd.SubmitCommands(cl);
            gd.SwapBuffers(gd.MainSwapchain);
        }
    }
}


