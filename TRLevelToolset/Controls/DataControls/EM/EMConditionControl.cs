using ImGuiNET;
using TREnvironmentEditor.Model;
using TRLevelToolset.Interfaces;

namespace TRLevelToolset.Controls.DataControls.EM;

internal class EMConditionControl : IDrawable
{
    private BaseEMCondition _data { get; set; }

    public EMConditionControl(BaseEMCondition data)
    {
        _data = data;
    }
    
    public void Draw()
    {
        ImGui.Button(_data.GetType().Name);
    }
}