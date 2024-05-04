using TRDataControl.Environment;
using TRLevelToolset.Interfaces;

namespace TRLevelToolset.Controls.DataControls.EM;

internal class EMMirroredControl : IDrawable
{
    private readonly EMEditorSet _data;
    private readonly List<EMFunctionControl> _funcControls;

    public EMMirroredControl(EMEditorSet data)
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