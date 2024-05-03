using Microsoft.VisualStudio.TestTools.UnitTesting;
using TRLevelControl;
using TRLevelControl.Model;

namespace TRLevelControlTests;

public class FDTestBase : TestBase
{
    protected static void TestSectorEdges(TRRoom room, int x, int z)
    {
        // Ensure we are anchored to the sector midpoint
        int rootX = (x / TRConsts.Step4) * TRConsts.Step4 + TRConsts.Step2;
        int rootZ = (z / TRConsts.Step4) * TRConsts.Step4 + TRConsts.Step2;

        void Reset()
        {
            x = rootX;
            z = rootZ;
        }

        Assert.AreEqual(TRConsts.Step2, rootX % TRConsts.Step4);
        Assert.AreEqual(TRConsts.Step2, rootZ % TRConsts.Step4);
        Reset();

        TRRoomSector sector1 = room.GetSector(x, z, TRUnit.World);

        // On the -x edge
        x -= TRConsts.Step2;
        TRRoomSector sector2 = room.GetSector(x, z, TRUnit.World);
        Assert.AreEqual(sector1, sector2);
        Reset();

        // On the -z edge
        z -= TRConsts.Step2;
        sector2 = room.GetSector(x, z, TRUnit.World);
        Assert.AreEqual(sector1, sector2);
        Reset();

        // Just over the -x edge
        x -= TRConsts.Step2 + 1;
        sector2 = room.GetSector(x, z, TRUnit.World);
        Assert.AreNotEqual(sector1, sector2);
        Reset();

        // Just over the -z edge
        z -= TRConsts.Step2 + 1;
        sector2 = room.GetSector(x, z, TRUnit.World);
        Assert.AreNotEqual(sector1, sector2);
        Reset();

        // Just over both -edges
        x -= TRConsts.Step2 + 1;
        z -= TRConsts.Step2 + 1;
        sector2 = room.GetSector(x, z, TRUnit.World);
        Assert.AreNotEqual(sector1, sector2);
        Reset();


        // On the +x edge
        x += TRConsts.Step2 - 1;
        sector2 = room.GetSector(x, z, TRUnit.World);
        Assert.AreEqual(sector1, sector2);
        Reset();

        // On the +z edge
        z += TRConsts.Step2 - 1;
        sector2 = room.GetSector(x, z, TRUnit.World);
        Assert.AreEqual(sector1, sector2);
        Reset();

        // Just over the +x edge
        x += TRConsts.Step2;
        sector2 = room.GetSector(x, z, TRUnit.World);
        Assert.AreNotEqual(sector1, sector2);
        Reset();

        // Just over the +z edge
        z += TRConsts.Step2;
        sector2 = room.GetSector(x, z, TRUnit.World);
        Assert.AreNotEqual(sector1, sector2);
        Reset();

        // Just over both +edges
        x += TRConsts.Step2;
        z += TRConsts.Step2;
        sector2 = room.GetSector(x, z, TRUnit.World);
        Assert.AreNotEqual(sector1, sector2);
    }

    protected static void TestGetEntityTriggers(FDControl floorData, int entityIndex)
    {
        List<FDTriggerEntry> triggers = floorData.GetEntityTriggers(entityIndex);
        Assert.AreNotEqual(0, triggers.Count);
        Assert.IsTrue(triggers.All(t => t.Actions.Find(a => a.Action == FDTrigAction.Object && a.Parameter == entityIndex) != null));
    }

    protected static void TestGetSecretTriggers(FDControl floorData, int secretIndex)
    {
        List<FDTriggerEntry> triggers = floorData.GetSecretTriggers(secretIndex);
        Assert.AreNotEqual(0, triggers.Count);
        Assert.IsTrue(triggers.All(t => t.Actions.Find(a => a.Action == FDTrigAction.SecretFound && a.Parameter == secretIndex) != null));
    }

    protected static void ModifyOverlaps(TRLevelBase level, Func<TRLevelBase> rewriteAction)
    {
        level.Boxes[0].Overlaps.Add(Enumerable.Range(0, level.Boxes.Count).Select(i => (ushort)i)
            .First(i => !level.Boxes[0].Overlaps.Contains(i)));

        level.Boxes[1].Overlaps.RemoveAt(0);

        List<List<ushort>> originalOverlaps = new();
        for (int i = 0; i < level.Boxes.Count; i++)
        {
            originalOverlaps.Add(new(level.Boxes[i].Overlaps));
        }

        level = rewriteAction();

        for (int i = 0; i < level.Boxes.Count; i++)
        {
            CollectionAssert.AreEqual(originalOverlaps[i], level.Boxes[i].Overlaps);
        }
    }
}
