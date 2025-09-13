using TRDataControl.Environment;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRLevelControlTests;

namespace TRDataControlTests.Environment.Entities;

[TestClass]
[TestCategory("Environment")]
public class MoveEnemyTests : TestBase
{
    [TestMethod]
    public void TestLandEnemyMove()
    {
        var level = Setup();
        var enemy = level.Entities[1];
        Assert.IsFalse(TR2TypeUtilities.IsWaterCreature(enemy.TypeID));

        var newEnemyLocation = new EMLocation
        {
            X = 4608,
            Y = -1024,
            Z = 3584,
            Room = 3,
        };
        var oldTriggers = level.FloorData.GetEntityTriggers(1);

        var func = new EMMoveEnemyFunction
        {
            EntityIndex = 1,
            IfLandCreature = true,
            Location = newEnemyLocation,
            TriggerLocations = [new()
            {
                X = 4608,
                Z = 8704,
                Room = 2,
            }],
        };
        func.ApplyToLevel(level);

        Assert.IsFalse(TR2TypeUtilities.IsWaterCreature(enemy.TypeID));
        Assert.AreEqual(newEnemyLocation.X, enemy.X);
        Assert.AreEqual(newEnemyLocation.Y, enemy.Y);
        Assert.AreEqual(newEnemyLocation.Z, enemy.Z);
        Assert.AreEqual(newEnemyLocation.Room, enemy.Room);

        var newTriggers = level.FloorData.GetEntityTriggers(1);
        Assert.IsFalse(newTriggers.Any(oldTriggers.Contains));
    }

    [TestMethod]
    public void TestClonedEnemyMove()
    {
        var level = Setup();
        var enemy = level.Entities[1];
        var clone = enemy.Clone() as TR2Entity;
        level.Entities.Add(clone);

        var newEnemyLocation = new EMLocation
        {
            X = 4608,
            Y = -1024,
            Z = 3584,
            Room = 3,
        };

        var func = new EMMoveEnemyFunction
        {
            EntityIndex = 1,
            IfLandCreature = true,
            Location = newEnemyLocation,
        };
        func.ApplyToLevel(level);

        Assert.AreEqual(newEnemyLocation.X, clone.X);
        Assert.AreEqual(newEnemyLocation.Y, clone.Y);
        Assert.AreEqual(newEnemyLocation.Z, clone.Z);
        Assert.AreEqual(newEnemyLocation.Room, clone.Room);
    }

    [TestMethod]
    public void TestLandEnemySwitch()
    {
        var level = Setup();
        var enemy = level.Entities[1];
        Assert.IsFalse(TR2TypeUtilities.IsWaterCreature(enemy.TypeID));

        var oldLocation = new EMLocation
        {
            X = enemy.X,
            Y = enemy.Y,
            Z = enemy.Z,
            Room = enemy.Room,
        };

        var func = new EMMoveEnemyFunction
        {
            EntityIndex = 1,
            IfLandCreature = true,
            AttemptWaterCreature = true,
        };
        func.ApplyToLevel(level);

        Assert.IsTrue(TR2TypeUtilities.IsWaterCreature(enemy.TypeID));
        Assert.AreEqual(oldLocation.X, enemy.X);
        Assert.AreEqual(oldLocation.Y, enemy.Y);
        Assert.AreEqual(oldLocation.Z, enemy.Z);
        Assert.AreEqual(oldLocation.Room, enemy.Room);
    }

    [TestMethod]
    public void TestWaterEnemyNoop()
    {
        var level = Setup();

        var enemy = level.Entities[1];
        enemy.TypeID = TR2Type.Barracuda;

        var oldLocation = new EMLocation
        {
            X = enemy.X,
            Y = enemy.Y,
            Z = enemy.Z,
            Room = enemy.Room,
        };

        var func = new EMMoveEnemyFunction
        {
            EntityIndex = 1,
            IfLandCreature = true,
            Location = new()
            {
                X = 4608,
                Y = -1024,
                Z = 3584,
                Room = 3,
            },
        };
        func.ApplyToLevel(level);

        Assert.AreEqual(TR2Type.Barracuda, enemy.TypeID);
        Assert.AreEqual(oldLocation.X, enemy.X);
        Assert.AreEqual(oldLocation.Y, enemy.Y);
        Assert.AreEqual(oldLocation.Z, enemy.Z);
        Assert.AreEqual(oldLocation.Room, enemy.Room);
    }

    private static TR2Level Setup()
    {
        var level = GetTR2TestLevel();

        // Put a land creature in a dry room that will in theory be flooded
        level.Entities[1].TypeID = TR2Type.TigerOrSnowLeopard;
        level.Entities[1].X = 3584;
        level.Entities[1].Y = 3072;
        level.Entities[1].Z = 11776;
        level.Entities[1].Room = 1;

        // Make a water creature model available
        level.Models[TR2Type.Barracuda] = level.Models[TR2Type.TigerOrSnowLeopard].Clone();
        level.Entities[4].TypeID = TR2Type.Barracuda;

        return level;
    }
}
