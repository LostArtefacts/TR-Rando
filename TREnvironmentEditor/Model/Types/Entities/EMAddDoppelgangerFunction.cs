using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMAddDoppelgangerFunction : EMAddEntityFunction
{
    public static readonly short AnchorRoomFlag = 0x200;

    public short AnchorRoom { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        TypeID = (short)TR1Type.Doppelganger;
        base.ApplyToLevel(level);

        EMLevelData data = GetData(level);
        level.Rooms[data.ConvertRoom(AnchorRoom)].Flags |= AnchorRoomFlag;
    }

    public override void ApplyToLevel(TR2Level level)
    {
        throw new NotSupportedException();
    }

    public override void ApplyToLevel(TR3Level level)
    {
        throw new NotSupportedException();
    }
}
