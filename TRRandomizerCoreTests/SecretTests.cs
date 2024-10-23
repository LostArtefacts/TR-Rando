using TRLevelControl.Model;
using TRLevelControl;
using TRRandomizerCore.Secrets;

namespace TRRandomizerCoreTests;

[TestClass]
[TestCategory("Secrets")]
public class SecretTests
{
    [TestMethod]
    public void TestSecretTriggerMasks()
    {
        // TR3 has a max of 6 secrets in any level but we'll test a silly number to ensure the algorithm works.
        for (int totalSecrets = 1; totalSecrets <= 21; totalSecrets++)
        {
            // The number of doors determines the trigger masks
            int requiredDoors = (int)Math.Ceiling((double)totalSecrets / TRConsts.MaskBits);
            List<int> doors = new(requiredDoors);
            for (int i = 0; i < requiredDoors; i++)
            {
                doors.Add(i);
            }

            List<TRSecretPlacement<TR3Type>> secrets = new();

            // Create a secret up to the limit for this "level" and set its mask and door
            for (short secretIndex = 0; secretIndex < totalSecrets; secretIndex++)
            {
                TRSecretPlacement<TR3Type> secret = new()
                {
                    SecretIndex = secretIndex
                };
                secret.SetMaskAndDoor(totalSecrets, doors);
                secrets.Add(secret);
            }

            // Now test that for each door, the sum of the trigger masks for the secrets
            // allocated to it equals full activation i.e. 31.
            for (int i = 0; i < requiredDoors; i++)
            {
                List<TRSecretPlacement<TR3Type>> doorTriggers = secrets.FindAll(s => s.DoorIndex == i);
                int mask = 0;
                doorTriggers.ForEach(s => mask += s.TriggerMask);

                Assert.AreEqual(TRConsts.FullMask, mask);
            }
        }
    }
}
