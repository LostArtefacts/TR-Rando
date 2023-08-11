using TRLevelControl.Model;

namespace TRLevelControl;

public class TRLevelReader : BinaryReader
{
    public TRLevelReader(Stream stream)
        : base(stream) { }

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
}
