using TRLevelControl.Model;

namespace TRLevelControl.Build;

public class TR5RoomBuilder : TRRoomBuilder<TR5Type, TR5Room>
{
    public TR5RoomBuilder()
        : base(TRGameVersion.TR5) { }

    protected override void BuildMesh(TR5Room room, TRLevelReader reader, ISpriteProvider<TR5Type> spriteProvider)
    {
        throw new NotImplementedException();
    }

    protected override void ReadLights(TR5Room room, TRLevelReader reader)
    {
        throw new NotImplementedException();
    }

    protected override void ReadStatics(TR5Room room, TRLevelReader reader)
    {
        throw new NotImplementedException();
    }

    protected override void WriteLights(TR5Room room, TRLevelWriter writer)
    {
        throw new NotImplementedException();
    }

    protected override void WriteMesh(TR5Room room, TRLevelWriter writer, ISpriteProvider<TR5Type> spriteProvider)
    {
        throw new NotImplementedException();
    }

    protected override void WriteStatics(TR5Room room, TRLevelWriter writer)
    {
        throw new NotImplementedException();
    }
}
