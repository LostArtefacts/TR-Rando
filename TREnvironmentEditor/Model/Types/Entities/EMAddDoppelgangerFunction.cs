using System;
using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;
using TRLevelControl.Model.Enums;

namespace TREnvironmentEditor.Model.Types
{
    public class EMAddDoppelgangerFunction : EMAddEntityFunction
    {
        public static readonly short AnchorRoomFlag = 0x200;

        public short AnchorRoom { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            TypeID = (short)TREntities.Doppelganger;
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
}
