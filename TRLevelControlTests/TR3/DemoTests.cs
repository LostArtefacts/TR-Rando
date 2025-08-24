using TRLevelControl.Model;

namespace TRLevelControlTests.TR3;

[TestClass]
[TestCategory("DemoData")]
public class DemoTests : TestBase
{
    [TestMethod]
    public void AddDemoData()
    {
        TR3Level level = GetTR3TestLevel();
        Assert.IsNull(level.DemoData);

        TRDemoData<TR3DemoGun, TR3InputState> data = new()
        {
            LaraPos = new()
            {
                X = 2048,
                Y = 1024,
                Z = 4096,
            },
            LaraRot = new()
            {
                X = 10,
                Y = 20,
                Z = 30,
            },
            LaraRoom = 19,
            LaraLastGun = TR3DemoGun.Harpoon,
            Inputs = new()
            {
                TR3InputState.None,
                TR3InputState.Forward,
                TR3InputState.Forward | TR3InputState.Left,
                TR3InputState.Forward | TR3InputState.Draw,
                TR3InputState.Forward | TR3InputState.Action,
                TR3InputState.Back,
                TR3InputState.Right,
                TR3InputState.Jump,
                TR3InputState.Walk,
                TR3InputState.Option,
                TR3InputState.Look,
                TR3InputState.StepLeft,
                TR3InputState.StepRight,
                TR3InputState.Roll,
                TR3InputState.MenuConfirm,
                TR3InputState.MenuBack,
                TR3InputState.Save,
                TR3InputState.Load,
                TR3InputState.Flare,
                TR3InputState.Duck,
                TR3InputState.Sprint,
            }
        };

        level.DemoData = data;
        level = WriteReadTempLevel(level);

        Assert.IsNotNull(level.DemoData);
        Assert.AreEqual(data.LaraPos.X, level.DemoData.LaraPos.X);
        Assert.AreEqual(data.LaraPos.Y, level.DemoData.LaraPos.Y);
        Assert.AreEqual(data.LaraPos.Z, level.DemoData.LaraPos.Z);
        Assert.AreEqual(data.LaraRot.X, level.DemoData.LaraRot.X);
        Assert.AreEqual(data.LaraRot.Y, level.DemoData.LaraRot.Y);
        Assert.AreEqual(data.LaraRot.Z, level.DemoData.LaraRot.Z);
        Assert.AreEqual(data.LaraRoom, level.DemoData.LaraRoom);
        Assert.AreEqual(data.LaraLastGun, level.DemoData.LaraLastGun);

        CollectionAssert.AreEqual(data.Inputs, level.DemoData.Inputs);
    }
}
