using ImGuiNET;
using TRLevelToolset.Interfaces;
using TRLevelToolset.IOLogic;

namespace TRLevelToolset.Components;

public class VersionComponent : IDrawable
{
    public void Draw()
    {
        ImGui.BeginChild(0, new System.Numerics.Vector2(1280, 45), ImGuiChildFlags.FrameStyle);
        ImGui.Text("Level: " + IOManager.FileName);
        ImGui.Text("Version: " + IOManager.CurrentLevel?.Version.File.ToString("X8"));
        ImGui.EndChild();
    }
}
