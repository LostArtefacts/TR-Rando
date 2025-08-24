using TRLevelControl.Model;

namespace TRLevelControlTests.TR1;

[TestClass]
[TestCategory("DemoData")]
public class DemoTests : TestBase
{
    [TestMethod]
    public void AddDemoData()
    {
        TR1Level level = GetTR1TestLevel();
        Assert.IsNull(level.DemoData);

        TRDemoData<TR1DemoGun, TR1InputState> data = new()
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
            LaraLastGun = TR1DemoGun.None,
            Inputs = new()
            {
                TR1InputState.None,
                TR1InputState.Forward,
                TR1InputState.Forward | TR1InputState.Left,
                TR1InputState.Forward | TR1InputState.Draw,
                TR1InputState.Forward | TR1InputState.Action,
                TR1InputState.Back,
                TR1InputState.Right,
                TR1InputState.Jump,
                TR1InputState.Walk,
                TR1InputState.Option,
                TR1InputState.Look,
                TR1InputState.StepLeft,
                TR1InputState.StepRight,
                TR1InputState.Roll,
                TR1InputState.MenuConfirm,
                TR1InputState.MenuBack,
                TR1InputState.Save,
                TR1InputState.Load,
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
