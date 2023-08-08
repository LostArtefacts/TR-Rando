using ImGuiNET;
using TREnvironmentEditor.Model;
using TRLevelToolset.Interfaces;

namespace TRLevelToolset.Controls.DataControls.EM;

internal class EMConditionControl : IDrawable
{
    private readonly BaseEMCondition _data;

    public EMConditionControl(BaseEMCondition data)
    {
        _data = data;
    }
    
    public void Draw()
    {
        ImGui.Button(_data.GetType().Name);
    }
}