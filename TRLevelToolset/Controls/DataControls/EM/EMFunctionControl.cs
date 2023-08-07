using ImGuiNET;
using TREnvironmentEditor.Model;
using TRLevelToolset.Interfaces;

namespace TRLevelToolset.Controls.DataControls.EM;

internal class EMFunctionControl : IDrawable
{
    private BaseEMFunction _data { get; set; }

    public EMFunctionControl(BaseEMFunction data)
    {
        _data = data;
    }
    
    public void Draw()
    {
        ImGui.Button(_data.GetType().Name);
    }
}