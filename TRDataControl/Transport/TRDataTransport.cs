using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TRDataControl;

public abstract class TRDataTransport<L, T, S, B>
    where L : TRLevelBase
    where T : Enum
    where S : Enum
    where B : TRBlobBase<T>
{
    protected static readonly string _defaultDataFolder = @"Resources\Objects";
    protected static readonly string _blobExt = ".TRB";

    public IDataProvider<T, S> Data { get; set; }
    public L Level { get; set; }
    public string LevelName { get; set; }
    public string DataFolder { get; set; } = _defaultDataFolder;

    public void StoreBlob(B blob)
    {
        Directory.CreateDirectory(DataFolder);
        TRBlobControl.Write(blob, Path.Combine(DataFolder, blob.Alias.ToString().ToUpper() + _blobExt));
    }

    public B LoadBlob(T type)
    {
        return TRBlobControl.Read<B>(Path.Combine(DataFolder, type.ToString().ToUpper() + _blobExt));
    }

    protected abstract TRTexturePacker CreatePacker();
    protected abstract TRTextureRemapper<L> CreateRemapper(L level);
    protected abstract bool IsMasterType(T type);
    protected abstract TRMesh GetDummyMesh();
    protected abstract TRDictionary<T, TRModel> Models { get; }
    protected abstract TRDictionary<T, TRStaticMesh> StaticMeshes { get; }
    protected abstract TRDictionary<T, TRSpriteSequence> SpriteSequences { get; }
    protected abstract List<TRCinematicFrame> CinematicFrames { get; }
}
