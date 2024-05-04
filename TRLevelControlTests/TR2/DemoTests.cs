using Microsoft.VisualStudio.TestTools.UnitTesting;
using TRLevelControl.Model;

namespace TRLevelControlTests.TR2;

[TestClass]
[TestCategory("DemoData")]
public class DemoTests : TestBase
{
    [TestMethod]
    public void AddDemoData()
    {
        TR2Level level = GetTR2TestLevel();
        Assert.IsNull(level.DemoData);

        TRDemoData<TR2DemoGun, TR2InputState> data = new()
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
            LaraLastGun = TR2DemoGun.GrenadeLauncher,
            Inputs = new()
            {
                TR2InputState.None,
                TR2InputState.Forward,
                TR2InputState.Forward | TR2InputState.Left,
                TR2InputState.Forward | TR2InputState.Draw,
                TR2InputState.Forward | TR2InputState.Action,
                TR2InputState.Back,
                TR2InputState.Right,
                TR2InputState.Jump,
                TR2InputState.Walk,
                TR2InputState.Option,
                TR2InputState.Look,
                TR2InputState.StepLeft,
                TR2InputState.StepRight,
                TR2InputState.Roll,
                TR2InputState.MenuConfirm,
                TR2InputState.MenuBack,
                TR2InputState.Save,
                TR2InputState.Load,
                TR2InputState.Flare,
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
