using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMAddDoppelgangerFunction : EMAddEntityFunction
{
    public short AnchorRoom { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        TypeID = (short)TR1Type.Doppelganger;
        base.ApplyToLevel(level);

        EMLevelData data = GetData(level);
        level.Rooms[data.ConvertRoom(AnchorRoom)].SetFlag(TRRoomFlag.Unused1, true);
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
