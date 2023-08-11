using TRLevelControl.Model;

namespace TRLevelControl;

public class TRLevelReader : BinaryReader
{
    public TRLevelReader(Stream stream)
        : base(stream) { }

    public ushort[] ReadUInt16s(long numData)
    {
        ushort[] data = new ushort[numData];
        for (int i = 0; i < numData; i++)
        {
            data[i] = ReadUInt16();
        }

        return data;
    }

    public List<TRTexImage8> ReadImage8s(long numImages)
    {
        List<TRTexImage8> images = new((int)numImages);
        for (long i = 0; i < numImages; i++)
        {
            images.Add(new()
            {
                Pixels = ReadBytes(TRConsts.TPageSize)
            });
        }
        return images;
    }

    public List<TRTexImage16> ReadImage16s(long numImages)
    {
        List<TRTexImage16> images = new((int)numImages);
        for (long i = 0; i < numImages; i++)
        {
            images.Add(new()
            {
                Pixels = ReadUInt16s(TRConsts.TPageSize)
            });
        }
        return images;
    }
}
