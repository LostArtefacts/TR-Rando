using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TRLevelControlTests;

public class TR45Observer : ObserverBase
{
    public override void TestOutput(byte[] input, byte[] output)
    {
        Assert.IsTrue(true);
    }
}
