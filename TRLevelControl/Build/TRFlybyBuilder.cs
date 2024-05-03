using TRLevelControl.Model;

namespace TRLevelControl.Build;

public class TRFlybyBuilder
{
    private readonly ITRLevelObserver _observer;

    public TRFlybyBuilder(ITRLevelObserver observer = null)
    {
        _observer = observer;
    }

    public TRDictionary<byte, List<TRFlybyCamera>> ReadFlybys(TRLevelReader reader)
    {
        TRDictionary<byte, List<TRFlybyCamera>> flybys = new();

        uint numFlybys = reader.ReadUInt32();
        for (int i = 0; i < numFlybys; i++)
        {
            TRFlybyCamera camera = new()
            {
                Position = reader.ReadVertex32(),
                Target = reader.ReadVertex32(),
            };

            // Sequence is the flyby ID, index is the camera position in the sequence.
            // This is reset on write as OG data contains gaps. Observers can observe.
            byte sequence = reader.ReadByte();
            byte index = reader.ReadByte();
            _observer?.OnFlybyIndexRead(sequence, index);

            camera.FOV = reader.ReadUInt16();
            camera.Roll = reader.ReadInt16();
            camera.Timer = reader.ReadUInt16();
            camera.Speed = reader.ReadUInt16();
            camera.Flags = (TRFlybyFlags)reader.ReadUInt16();
            camera.Room = reader.ReadUInt32();

            if (!flybys.ContainsKey(sequence))
            {
                flybys[sequence] = new();
            }
            flybys[sequence].Add(camera);
        }

        return flybys;
    }

    public void WriteFlybys(TRLevelWriter writer, TRDictionary<byte, List<TRFlybyCamera>> flybys)
    {
        writer.Write((uint)flybys.Values.Sum(f => f.Count));

        foreach (byte sequence in flybys.Keys)
        {
            List<byte> indices = _observer?.GetFlybyIndices(sequence);
            for (byte i = 0; i < flybys[sequence].Count; i++)
            {
                TRFlybyCamera camera = flybys[sequence][i];
                writer.Write(camera.Position);
                writer.Write(camera.Target);
                writer.Write(sequence);
                writer.Write(indices?[i] ?? i);
                writer.Write(camera.FOV);
                writer.Write(camera.Roll);
                writer.Write(camera.Timer);
                writer.Write(camera.Speed);
                writer.Write((ushort)camera.Flags);
                writer.Write(camera.Room);
            }
        }
    }
}
