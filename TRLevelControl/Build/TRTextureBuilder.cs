using System.Diagnostics;
using System.Drawing;
using TRLevelControl.Model;

namespace TRLevelControl.Build;

public class TRTextureBuilder
{
    private static readonly string _texMarker = "TEX";

    private readonly TRGameVersion _version;
    private readonly ITRLevelObserver _observer;

    public TRTextureBuilder(TRGameVersion version, ITRLevelObserver observer = null)
    {
        _version = version;
        _observer = observer;
    }

    public List<TRObjectTexture> ReadObjectTextures(TRLevelReader reader, bool remastered = false)
    {
        if (_version >= TRGameVersion.TR4)
        {
            string texMarker = new(reader.ReadChars(_texMarker.Length));
            Debug.Assert(texMarker == _texMarker);
            if (_version == TRGameVersion.TR5 && !remastered)
            {
                byte end = reader.ReadByte();
                Debug.Assert(end == 0);
            }
        }

        List<TRObjectTexture> textures = new();
        uint numTextures = reader.ReadUInt32();

        for (int i = 0; i < numTextures; i++)
        {
            textures.Add(Read(reader, i, remastered));
        }

        return textures;
    }

    public List<TRAnimatedTexture> ReadAnimatedTextures(TRLevelReader reader)
    {
        uint textureLength = reader.ReadUInt32();
        long endPosition = reader.BaseStream.Position + textureLength * sizeof(ushort);

        ushort numGroups = reader.ReadUInt16();
        List<TRAnimatedTexture> textures = reader.ReadAnimatedTextures(numGroups);

        // Old TRLEs can contain unreferenced texture data, so ensure to seek to the
        // end of the data before finishing.
        reader.BaseStream.Position = Math.Max(endPosition, reader.BaseStream.Position);

        if (_version >= TRGameVersion.TR4)
        {
            // The first sets in the group are UVRotate mode, else classic.
            byte uvRotates = reader.ReadByte();
            for (int i = 0; i < uvRotates && i < numGroups; i++)
            {
                textures[i].Mode = TRAnimatedTextureMode.UVRotate;
            }
        }

        return textures;
    }

    public void Write(TRLevelWriter writer, List<TRObjectTexture> textures, bool remastered = false)
    {
        if (_version >= TRGameVersion.TR4)
        {
            writer.Write(_texMarker.ToCharArray());
            if (_version == TRGameVersion.TR5 && !remastered)
            {
                writer.Write((byte)0);
            }
        }

        writer.Write((uint)textures.Count);

        for (int i = 0; i < textures.Count; i++)
        {
            Write(writer, textures[i], i, remastered);
        }
    }

    public void Write(TRLevelWriter writer, List<TRAnimatedTexture> textures)
    {
        List<ushort> data = new()
        {
            (ushort)textures.Count
        };

        if (_version >= TRGameVersion.TR4)
        {
            // Make sure to put UVRotate mode first.
            textures = new(textures.OrderByDescending(t => t.Mode));
        }

        byte uvRotate = 0;
        foreach (TRAnimatedTexture texture in textures)
        {
            data.Add((ushort)(texture.Textures.Count - 1));
            data.AddRange(texture.Textures);
            if (texture.Mode == TRAnimatedTextureMode.UVRotate)
            {
                uvRotate++;
            }
        }

        writer.Write((uint)data.Count);
        writer.Write(data);

        if (_version >= TRGameVersion.TR4)
        {
            writer.Write(uvRotate);
        }
    }

    private TRObjectTexture Read(TRLevelReader reader, int index, bool remastered)
    {
        TRObjectTexture texture = new()
        {
            BlendingMode = (TRBlendingMode)reader.ReadUInt16()
        };

        ushort flags = reader.ReadUInt16();
        texture.Atlas = (ushort)(flags & 0x7FFF);

        if (_version >= TRGameVersion.TR4)
        {
            texture.IsTriangle = (flags & 0x8000) > 0;

            ushort newFlags = reader.ReadUInt16();
            texture.Target = (newFlags & 0x8000) > 0 ? TRObjectTextureMode.Room : TRObjectTextureMode.Model;
            texture.BumpLevel = (byte)((newFlags & 0x600) >> 9);
            texture.MappingCorrection = (byte)(newFlags & 0x7);
            texture.UnknownFlag = (newFlags & 0x0800) > 0;
        }
        else
        {
            texture.Target = TRObjectTextureMode.Classic;
        }

        texture.Vertices = reader.ReadObectTextureVertices(4);

        if (_version >= TRGameVersion.TR4)
        {
            uint originalU = reader.ReadUInt32();
            uint originalV = reader.ReadUInt32();
            _observer?.OnOrignalUVRead(index, new(originalU, originalV));

            // Width-1 and Height-1
            reader.ReadUInt32();
            reader.ReadUInt32();

            if (_version == TRGameVersion.TR5 && !remastered)
            {
                // Padding
                reader.ReadUInt16();
            }
        }

        return texture;
    }

    private void Write(TRLevelWriter writer, TRObjectTexture texture, int index, bool remastered)
    {
        TRBlendingMode blendingMode = texture.BlendingMode;
        if ((blendingMode == TRBlendingMode.AlphaBlending && _version < TRGameVersion.TR3)
            || (blendingMode == TRBlendingMode.ForcedAlpha && _version < TRGameVersion.TR4))
        {
            blendingMode = TRBlendingMode.AlphaTesting;
        }

        writer.Write((ushort)blendingMode);

        ushort flags = texture.Atlas;
        if (_version >= TRGameVersion.TR4)
        {
            if (texture.IsTriangle)
            {
                flags |= 0x8000;
            }
            writer.Write(flags);

            ushort newFlags = (ushort)(texture.MappingCorrection & 0x7);
            newFlags |= (ushort)((texture.BumpLevel << 9) & 0x600);
            if (texture.Target == TRObjectTextureMode.Room)
            {
                newFlags |= 0x8000;
            }
            if (texture.UnknownFlag)
            {
                newFlags |= 0x0800;
            }
            writer.Write(newFlags);
        }
        else
        {
            writer.Write(flags);
        }

        writer.Write(texture.Vertices);

        if (_version >= TRGameVersion.TR4)
        {
            // Discarded in game, so only written back as OG for tests.
            Tuple<uint, uint> originalUV = _observer?.GetOrignalUV(index) ?? new(0, 0);
            writer.Write(originalUV.Item1);
            writer.Write(originalUV.Item2);

            Size size = texture.Size;
            if (_version == TRGameVersion.TR4 || remastered)
            {
                writer.Write((uint)(size.Width - 1));
                writer.Write((uint)(size.Height - 1));
            }
            else
            {
                writer.Write((uint)((size.Width - 1) << 16));
                writer.Write((uint)((size.Height - 1) << 16));

                if (!remastered)
                {
                    // Padding
                    writer.Write((ushort)0);
                }
            }
        }
    }
}
