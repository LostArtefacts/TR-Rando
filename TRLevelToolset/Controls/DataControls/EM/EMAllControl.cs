using TREnvironmentEditor.Model;
using TRLevelToolset.Interfaces;

namespace TRLevelToolset.Controls.DataControls.EM;

internal class EMAllControl : IDrawable
{
    private EMEditorSet _data { get; set; }
    private List<EMFunctionControl> _funcControls { get; set; }

    public EMAllControl(EMEditorSet data)
    {
        _data = data;
        _funcControls = new List<EMFunctionControl>();

        for (int i = 0; i < _data.Count; i++)
        {
            _funcControls.Add(new EMFunctionControl(_data[i]));
        }
    }
    
    public void Draw()
    {
        foreach (var control in _funcControls)
        {
            control.Draw();
        }
    }
}