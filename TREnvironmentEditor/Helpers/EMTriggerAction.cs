using TRLevelControl.Model;

namespace TREnvironmentEditor.Helpers;

public class EMTriggerAction
{
    public FDTrigAction Action { get; set; }
    public short Parameter { get; set; }

    public FDActionItem ToFDAction(EMLevelData levelData)
    {
        ushort parameter = (ushort)Parameter;
        if (Parameter < 0)
        {
            switch (Action)
            {
                case FDTrigAction.Camera:
                    parameter = (ushort)(levelData.NumCameras + Parameter);
                    break;
                case FDTrigAction.LookAtItem:
                case FDTrigAction.Object:
                    parameter = (ushort)(levelData.NumEntities + Parameter);
                    break;
            }
        }

        return new FDActionItem
        {
            TrigAction = Action,
            Parameter = parameter
        };
    }

    public static EMTriggerAction FromFDAction(FDActionItem action)
    {
        return new EMTriggerAction
        {
            Action = action.TrigAction,
            Parameter = (short)action.Parameter
        };
    }
}
