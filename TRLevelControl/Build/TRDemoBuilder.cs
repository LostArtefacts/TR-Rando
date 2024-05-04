using TRLevelControl.Model;

namespace TRLevelControl.Build;

public class TRDemoBuilder<G, I>
    where G : Enum
    where I : Enum
{
    private readonly TRGameVersion _version;

    public TRDemoBuilder(TRGameVersion version)
    {
        _version = version;
    }

    public TRDemoData<G, I> Read(TRLevelReader reader)
    {
        ushort numDemoData = reader.ReadUInt16();
        if (numDemoData == 0)
        {
            return null;
        }

        TRDemoData<G, I> demoData = new()
        {
            LaraPos = reader.ReadVertex32(),
            LaraRot = reader.ReadVertex32(),
            LaraRoom = reader.ReadInt32(),
        };

        if (_version > TRGameVersion.TR1)
        {
            demoData.LaraLastGun = (G)(object)reader.ReadInt32();
        }

        int inputData;
        while ((inputData = reader.ReadInt32()) != -1)
        {
            demoData.Inputs.Add((I)(object)inputData);
        }

        return demoData;
    }

    public void Write(TRDemoData<G, I> demoData, TRLevelWriter writer)
    {
        if (demoData == null)
        {
            writer.Write((ushort)0);
            return;
        }

        byte[] data;
        {
            using MemoryStream ms = new();
            using TRLevelWriter demoWriter = new(ms);

            demoWriter.Write(demoData.LaraPos);
            demoWriter.Write(demoData.LaraRot);
            demoWriter.Write(demoData.LaraRoom);

            if (_version > TRGameVersion.TR1)
            {
                demoWriter.Write((int)(object)demoData.LaraLastGun);
            }

            demoWriter.Write(demoData.Inputs.Select(i => (int)(object)i));
            demoWriter.Write(-1);

            data = ms.ToArray();
        }

        writer.Write((ushort)data.Length);
        writer.Write(data);
    }
}
