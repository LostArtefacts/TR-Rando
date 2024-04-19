using Microsoft.VisualStudio.TestTools.UnitTesting;
using TRLevelControl;

namespace TRLevelControlTests;

public class ObserverBase : ITRLevelObserver
{
    public virtual void TestOutput(byte[] input, byte[] output)
    {
        CollectionAssert.AreEqual(input, output);
    }
}
