using TRLevelControl.Model;

namespace TREnvironmentEditor.Helpers;

public class EMTriggerAction
{
    public FDTrigAction Action { get; set; }
    public short Parameter { get; set; }

    public FDActionItem ToFDAction(EMLevelData levelData)
    {
        short parameter = Parameter;
        if (Parameter < 0)
        {
            switch (Action)
            {
                case FDTrigAction.Camera:
                    parameter = (short)(levelData.NumCameras + Parameter);
                    break;
                case FDTrigAction.LookAtItem:
                case FDTrigAction.Object:
                    parameter = (short)(levelData.NumEntities + Parameter);
                    break;
            }
        }

        return new FDActionItem
        {
            Action = Action,
            Parameter = parameter
        };
    }

    public static EMTriggerAction FromFDAction(FDActionItem action)
    {
        return new()
        {
            Action = action.Action,
            Parameter = action.Parameter
        };
    }
}
