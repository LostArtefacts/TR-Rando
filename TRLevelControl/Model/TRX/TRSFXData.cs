namespace TRLevelControl.Model.TRX;

public class TRSFXData
{
    public short ID { get; set; }
    public ushort Volume { get; set; }
    public ushort Chance { get; set; }
    public ushort Flags { get; set; }
    public List<byte[]> Data { get; set; }

    public static TRSFXData Read(TRLevelReader reader)
    {
        var sfx = new TRSFXData
        {
            ID = reader.ReadInt16(),
            Volume = reader.ReadUInt16(),
            Chance = reader.ReadUInt16(),
            Flags = reader.ReadUInt16(),
            Data = [],
        };
        
        int sampleCount = (sfx.Flags & 0xFC) >> 2;
        for (int i = 0; i < sampleCount; i++)
        {
            var length = reader.ReadInt32();
            sfx.Data.Add(reader.ReadBytes(length));
        }

        return sfx;
    }

    public void Write(TRLevelWriter writer)
    {
        writer.Write(ID);
        writer.Write(Volume);
        writer.Write(Chance);
        writer.Write(Flags);
        Data.ForEach(wav =>
        {
            writer.Write(wav.Length);
            writer.Write(wav);
        });
    }
}
