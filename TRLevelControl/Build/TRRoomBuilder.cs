using TRLevelControl.Model;

namespace TRLevelControl.Build;

public abstract class TRRoomBuilder<T, R>
    where T : Enum
    where R : TRRoom, new()
{
    protected readonly TRGameVersion _version;
    protected List<ushort[]> _rawMeshes = new();

    public TRRoomBuilder(TRGameVersion version)
    {
        _version = version;
    }

    public List<R> ReadRooms(TRLevelReader reader)
    {
        ushort numRooms = reader.ReadUInt16();
        List<R> rooms = new();
        for (int i = 0; i < numRooms; i++)
        {
            R room = new()
            {
                Info = reader.ReadRoomInfo(_version)
            };
            rooms.Add(room);

            uint numMeshData = reader.ReadUInt32();
            _rawMeshes.Add(reader.ReadUInt16s(numMeshData));

            ushort numPortals = reader.ReadUInt16();
            room.Portals = reader.ReadRoomPortals(numPortals);

            room.NumZSectors = reader.ReadUInt16();
            room.NumXSectors = reader.ReadUInt16();
            room.Sectors = reader.ReadRoomSectors(room.NumXSectors * room.NumZSectors, _version);

            ReadLights(room, reader);
            ReadStatics(room, reader);

            room.AlternateRoom = reader.ReadInt16();
            room.Flags = (TRRoomFlag)reader.ReadInt16();

            ReadAdditionalProperties(room, reader);
        }

        return rooms;
    }

    public void BuildMesh(R room, int roomIndex, ISpriteProvider<T> spriteProvider)
    {
        ushort[] rawData = _rawMeshes[roomIndex];
        byte[] target = new byte[rawData.Length * sizeof(short)];
        Buffer.BlockCopy(rawData, 0, target, 0, target.Length);

        using MemoryStream ms = new(target);
        using TRLevelReader reader = new(ms);

        BuildMesh(room, reader, spriteProvider);
    }

    protected List<TRFace> ReadFaces(TRFaceType type, TRLevelReader reader)
    {
        short numFaces = reader.ReadInt16();
        return reader.ReadRoomFaces(numFaces, type, _version);
    }

    protected List<TRRoomSprite<T>> ReadSprites(TRLevelReader reader, ISpriteProvider<T> spriteProvider)
    {
        short numSprites = reader.ReadInt16();
        List<TRRoomSprite<T>> sprites = new();

        for (int i = 0; i < numSprites; i++)
        {
            sprites.Add(new()
            {
                Vertex = reader.ReadInt16(),
                ID = spriteProvider.FindSpriteType(reader.ReadInt16())
            });
        }

        return sprites;
    }

    public void WriteRooms(TRLevelWriter writer, List<R> rooms, ISpriteProvider<T> spriteProvider)
    {
        writer.Write((ushort)rooms.Count);
        foreach (R room in rooms)
        {
            writer.Write(room.Info, _version);

            using MemoryStream stream = new();
            using TRLevelWriter meshWriter = new(stream);

            WriteMesh(room, meshWriter, spriteProvider);
            byte[] data = stream.ToArray();
            writer.Write((uint)data.Length / sizeof(short));
            writer.Write(data);

            writer.Write((ushort)room.Portals.Count);
            writer.Write(room.Portals);

            writer.Write(room.NumZSectors);
            writer.Write(room.NumXSectors);
            writer.Write(room.Sectors, _version);

            WriteLights(room, writer);
            WriteStatics(room, writer);

            writer.Write(room.AlternateRoom);
            writer.Write((short)room.Flags);

            WriteAdditionalProperties(room, writer);
        }
    }

    protected void WriteFaces(List<TRFace> faces, TRLevelWriter writer)
    {
        writer.Write((short)faces.Count);
        writer.Write(faces, _version);
    }

    protected void WriteSprites(List<TRRoomSprite<T>> sprites, ISpriteProvider<T> spriteProvider, TRLevelWriter writer)
    {
        writer.Write((short)sprites.Count);
        foreach (TRRoomSprite<T> sprite in sprites)
        {
            writer.Write(sprite.Vertex);
            writer.Write(spriteProvider.GetSpriteOffset(sprite.ID));
        }
    }

    protected abstract void ReadLights(R room, TRLevelReader reader);
    protected abstract void ReadStatics(R room, TRLevelReader reader);
    protected virtual void ReadAdditionalProperties(R room, TRLevelReader reader) { }

    protected abstract void WriteLights(R room, TRLevelWriter writer);
    protected abstract void WriteStatics(R room, TRLevelWriter writer);
    protected virtual void WriteAdditionalProperties(R room, TRLevelWriter writer) { }

    protected abstract void BuildMesh(R room, TRLevelReader reader, ISpriteProvider<T> spriteProvider);
    protected abstract void WriteMesh(R room, TRLevelWriter writer, ISpriteProvider<T> spriteProvider);
}
