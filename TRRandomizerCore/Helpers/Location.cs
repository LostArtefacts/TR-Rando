using System.ComponentModel;
using System.Numerics;
using TRLevelControl.Model;

namespace TRRandomizerCore.Helpers;

public class Location : ITRLocatable, ICloneable
{
    public const string DefaultPackID = "TRRando";

    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public short Room { get; set; }
    public bool RequiresGlitch { get; set; }
    public Difficulty Difficulty { get; set; }
    public bool IsUWCorner { get; set; }
    public ItemRange Range { get; set; }
    public RoomType RoomType { get; set; }
    public bool VehicleRequired { get; set; }
    public bool RequiresDamage { get; set; }
    public bool InvalidatesRoom { get; set; }
    public bool RequiresReturnPath { get; set; }
    public LevelState LevelState { get; set; }

    [DefaultValue(16384)]
    public short Angle { get; set; }

    [DefaultValue(true)]
    public bool Validated { get; set; }

    [DefaultValue(-1)]
    public int EntityIndex { get; set; }

    [DefaultValue(-1)]
    public short TargetType { get; set; }

    [DefaultValue(DefaultPackID)]
    public string PackID { get; set; }

    [DefaultValue("")]
    public string KeyItemsLow { get; set; }

    [DefaultValue("")]
    public string KeyItemsHigh { get; set; }

    public Location()
    {
        Angle = 16384;
        Validated = true;
        EntityIndex = -1;
        TargetType = -1;
        PackID = DefaultPackID;
        KeyItemsLow = string.Empty;
        KeyItemsHigh = string.Empty;
    }

    public bool IsEquivalent(Location other)
    {
        return other.X == X
            && other.Y == Y
            && other.Z == Z
            && other.Room == Room;
    }

    public Vector3 ToVector()
    {
        return new(X, Y, Z);
    }

    public Location Clone()
        => (Location)MemberwiseClone();

    object ICloneable.Clone()
        => Clone();

    public override string ToString()
    {
        return $"{base.ToString()} X: {X} Y: {Y} Z: {Z} Room: {Room} Angle: {Angle}";
    }
}
