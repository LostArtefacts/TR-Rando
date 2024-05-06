using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Newtonsoft.Json;
using System.Text;

namespace TRDataControl;

public class TRBlobControl
{
    private static readonly uint _blobMagic = 'T' | ('R' << 8) | ('B' << 16) | ('M' << 24);

    private static readonly TRBlobConverter _converter = new();
    private static readonly JsonSerializerSettings _serializer = new()
    {
        ContractResolver = new TRBlobResolver()
    };

    public static T Read<T>(string file)
    {
        using BinaryReader reader = new(File.OpenRead(file));

        uint magic = reader.ReadUInt32();
        if (magic != _blobMagic)
        {
            throw new Exception("Unrecognised blob data file.");
        }

        using MemoryStream inflatedStream = new();
        using InflaterInputStream inflater = new(reader.BaseStream);
        inflater.CopyTo(inflatedStream);

        string data = Encoding.Default.GetString(inflatedStream.ToArray());
        return JsonConvert.DeserializeObject<T>(data, _converter);
    }

    public static void Write<T>(T blob, string file)
    {
        using BinaryWriter writer = new(File.Create(file));

        writer.Write(_blobMagic);

        string data = JsonConvert.SerializeObject(blob, _serializer);
        using MemoryStream inStream = new(Encoding.Default.GetBytes(data));
        using DeflaterOutputStream deflater = new(writer.BaseStream);

        inStream.CopyTo(deflater);
        deflater.Finish();
    }
}
