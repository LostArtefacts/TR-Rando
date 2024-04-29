using System.Diagnostics;
using TRLevelControl.Model;

namespace TRLevelControl.Build;

public class TRSpriteBuilder<T> : ISpriteProvider<T>
    where T : Enum
{
    private static readonly string _sprMarker = "SPR";

    private readonly TRGameVersion _version;
    private Dictionary<T, short> _spriteOffsets;

    public TRSpriteBuilder(TRGameVersion version)
    {
        _version = version;
    }

    public TRDictionary<T, TRSpriteSequence> ReadSprites(TRLevelReader reader)
    {
        if (_version >= TRGameVersion.TR4)
        {
            string sprMarker = new(reader.ReadChars(_sprMarker.Length));
            Debug.Assert(sprMarker == _sprMarker);
            Debug.Assert(_version != TRGameVersion.TR5 || reader.ReadByte() == 0);
        }

        uint numTextures = reader.ReadUInt32();
        List<TRSpriteTexture> textures = reader.ReadSpriteTextures(numTextures, _version);

        uint numSpriteSequences = reader.ReadUInt32();
        TRDictionary<T, TRSpriteSequence> sprites = new();

        for (int i = 0; i < numSpriteSequences; i++)
        {
            TRSpriteSequence sequence = new()
            {
                Textures = new()
            };
            sprites[(T)(object)(uint)reader.ReadInt32()] = sequence;

            short negativeLength = reader.ReadInt16();
            short offset = reader.ReadInt16();
            for (int j = 0; j < -negativeLength; j++)
            {
                sequence.Textures.Add(textures[offset + j]);
            }
        }

        CacheSpriteOffsets(sprites);
        return sprites;
    }

    public void WriteSprites(TRLevelWriter writer, TRDictionary<T, TRSpriteSequence> sprites)
    {
        if (_version >= TRGameVersion.TR4)
        {
            writer.Write(_sprMarker.ToCharArray());
            if (_version == TRGameVersion.TR5)
            {
                writer.Write((byte)0);
            }
        }

        List<TRSpriteTexture> textures = sprites.Values
            .SelectMany(s => s.Textures)
            .ToList();

        writer.Write((uint)textures.Count);
        writer.Write(textures, _version);

        writer.Write((uint)sprites.Count);
        foreach (T spriteID in sprites.Keys)
        {
            TRSpriteSequence sequence = sprites[spriteID];
            uint id = (uint)(object)spriteID;
            writer.Write((int)id);
            writer.Write((short)-sequence.Textures.Count);
            writer.Write(GetSpriteOffset(spriteID));
        }
    }

    public void CacheSpriteOffsets(TRDictionary<T, TRSpriteSequence> spriteSequences)
    {
        _spriteOffsets = new();
        short offset = 0;
        foreach (var (type, sequence) in spriteSequences)
        {
            _spriteOffsets[type] = offset;
            offset += (short)sequence.Textures.Count;
        }
    }

    public T FindSpriteType(short textureOffset)
    {
        foreach (var (type, offset) in _spriteOffsets)
        {
            if (offset == textureOffset)
            {
                return type;
            }
        }

        throw new IndexOutOfRangeException();
    }

    public short GetSpriteOffset(T type)
    {
        return _spriteOffsets[type];
    }
}
