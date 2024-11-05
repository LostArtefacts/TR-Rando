using TRGE.Core;
using TRLevelControl;
using TRLevelControl.Model;

namespace TRRandomizerCore.Editors;

public class TRRCheatEditor<T>
    where T : Enum
{
    private readonly string _wipDirectory;
    private readonly TRPDPControlBase<T> _control;

    public TRRCheatEditor(string wipDirectory, TRPDPControlBase<T> control)
    {
        _wipDirectory = wipDirectory;
        _control = control;
    }

    public void AddCheats(TRRScriptedLevel level)
    {
        // TODO make this a configurable option for testers.
#if DEBUG
        string pdpPath = Path.Combine(_wipDirectory, level.PdpFileBaseName);
        TRDictionary<T, TRModel> pdpData = _control.Read(pdpPath);

        TRFXCommand endLevelCmd = new()
        {
            FrameNumber = 1,
            EffectID = (short)TR1FX.EndLevel,
        };

        // Side-step left to end level
        pdpData[default].Animations[65].Commands.Add(endLevelCmd);

        // Underwater roll to end level
        int uwRollAnim = level.Version == TRGE.Core.TRVersion.TR1 ? 99 : 203;
        pdpData[default].Animations[uwRollAnim].Commands.Add(endLevelCmd);

        _control.Write(pdpData, pdpPath);
#endif
    }
}
