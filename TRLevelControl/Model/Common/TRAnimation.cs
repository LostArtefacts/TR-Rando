﻿namespace TRLevelControl.Model;

public class TRAnimation : ICloneable
{
    public byte FrameRate { get; set; }
    public ushort StateID { get; set; }
    public TRFixedFloat32 Speed { get; set; }
    public TRFixedFloat32 Accel { get; set; }
    public TRFixedFloat32 SpeedLateral { get; set; }
    public TRFixedFloat32 AccelLateral { get; set; }
    public short FrameStart { get; set; }
    public short FrameEnd { get; set; }
    public ushort NextAnimation { get; set; }
    public ushort NextFrame { get; set; }
    public List<TRStateChange> Changes { get; set; } = new();
    public List<TRAnimCommand> Commands { get; set; } = new();
    public List<TRAnimFrame> Frames { get; set; } = new();

    public int TotalDispatchCount => Changes.Sum(c => c.Dispatches.Count);

    public TRAnimation Clone()
    {
        return new()
        {
            StateID = StateID,
            FrameRate = FrameRate,
            FrameStart = FrameStart,
            FrameEnd = FrameEnd,
            Speed = Speed,
            Accel = Accel,
            SpeedLateral = SpeedLateral,
            AccelLateral = AccelLateral,
            NextAnimation = NextAnimation,
            NextFrame = NextFrame,
            Frames = new(Frames.Select(f => f.Clone())),
            Changes = new(Changes.Select(c => c.Clone())),
            Commands = new(Commands.Select(c => c.Clone()))
        };
    }

    object ICloneable.Clone()
        => Clone();
}
