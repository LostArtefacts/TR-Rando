using ImGuiNET;
using TRDataControl.Environment;
using TRLevelToolset.Interfaces;

namespace TRLevelToolset.Controls.DataControls.EM;

internal class EMFunctionControl : IDrawable
{
    private readonly BaseEMFunction _data;

    public EMFunctionControl(BaseEMFunction data)
    {
        _data = data;
    }
    
    public void Draw()
    {
        ImGui.Button(_data.GetType().Name);
    }
}