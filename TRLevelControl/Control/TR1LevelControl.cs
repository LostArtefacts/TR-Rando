using System.Diagnostics;
using TRLevelControl.Build;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRLevelControl;

public class TR1LevelControl : TRLevelControlBase<TR1Level>
{
    private readonly TRObjectMeshBuilder<TR1Type> _meshBuilder;
    private readonly TRSpriteBuilder<TR1Type> _spriteBuilder;

    public TR1LevelControl(ITRLevelObserver observer = null)
        : base(observer)
    {
        _meshBuilder = new(TRGameVersion.TR1, _observer);
        _spriteBuilder = new(TRGameVersion.TR1);
    }

    protected override TR1Level CreateLevel(TRFileVersion version)
    {
        TR1Level level = new()
        {
            Version = new()
            {
                Game = TRGameVersion.TR1,
                File = version
            }
        };

        TestVersion(level, TRFileVersion.TR1);
        return level;
    }

    protected override void Read(TRLevelReader reader)
    {
        uint numImages = reader.ReadUInt32();
        _level.Images8 = reader.ReadImage8s(numImages);

        // Unused, always 0 in OG
        _level.Version.LevelNumber = reader.ReadUInt32();

        ushort numRooms = reader.ReadUInt16();
        _level.Rooms = new();
        for (int i = 0; i < numRooms; i++)
        {
            TR1Room room = new()
            {
                //Grab info
                Info = new TRRoomInfo
                {
                    X = reader.ReadInt32(),
                    Z = reader.ReadInt32(),
                    YBottom = reader.ReadInt32(),
                    YTop = reader.ReadInt32()
                },
            };
            _level.Rooms.Add(room);

            uint numWords = reader.ReadUInt32();
            room.Mesh = ConvertToRoomData(reader.ReadUInt16s(numWords));

            //Portals
            ushort numPortals = reader.ReadUInt16();
            room.Portals = new();
            for (int j = 0; j < numPortals; j++)
            {
                room.Portals.Add(TR2FileReadUtilities.ReadRoomPortal(reader));
            }

            //Sectors
            room.NumZSectors = reader.ReadUInt16();
            room.NumXSectors = reader.ReadUInt16();
            room.Sectors = new();
            for (int j = 0; j < (room.NumXSectors * room.NumZSectors); j++)
            {
                room.Sectors.Add(TR2FileReadUtilities.ReadRoomSector(reader));
            }

            //Lighting
            room.AmbientIntensity = reader.ReadInt16();
            ushort numLights = reader.ReadUInt16();
            room.Lights = new();
            for (int j = 0; j < numLights; j++)
            {
                room.Lights.Add(TR1FileReadUtilities.ReadRoomLight(reader));
            }

            //Static meshes
            ushort numStaticMeshes = reader.ReadUInt16();
            room.StaticMeshes = new();
            for (int j = 0; j < numStaticMeshes; j++)
            {
                room.StaticMeshes.Add(TR1FileReadUtilities.ReadRoomStaticMesh(reader));
            }

            room.AlternateRoom = reader.ReadInt16();
            room.Flags = reader.ReadInt16();
        }

        uint numFloorData = reader.ReadUInt32();
        _level.FloorData = reader.ReadUInt16s(numFloorData).ToList();

        ReadMeshData(reader);
        ReadModelData(reader);

        ReadStaticMeshes(reader);

        uint numObjectTextures = reader.ReadUInt32();
        _level.ObjectTextures = new();
        for (int i = 0; i < numObjectTextures; i++)
        {
            _level.ObjectTextures.Add(TR2FileReadUtilities.ReadObjectTexture(reader));
        }

        ReadSprites(reader);

        //Cameras
        uint numCameras = reader.ReadUInt32();
        _level.Cameras = new();
        for (int i = 0; i < numCameras; i++)
        {
            _level.Cameras.Add(TR2FileReadUtilities.ReadCamera(reader));
        }

        uint numSoundSources = reader.ReadUInt32();
        _level.SoundSources = new();
        for (int i = 0; i < numSoundSources; i++)
        {
            _level.SoundSources.Add(TR2FileReadUtilities.ReadSoundSource(reader));
        }

        //Boxes
        uint numBoxes = reader.ReadUInt32();
        _level.Boxes = new();
        for (int i = 0; i < numBoxes; i++)
        {
            _level.Boxes.Add(TR1FileReadUtilities.ReadBox(reader));
        }

        //Overlaps & Zones
        uint numOverlaps = reader.ReadUInt32();
        _level.Overlaps = reader.ReadUInt16s(numOverlaps).ToList();

        ushort[] zoneData = reader.ReadUInt16s(numBoxes * 6);
        _level.Zones = TR1BoxUtilities.ReadZones(numBoxes, zoneData);

        reader.ReadUInt32(); // Total count of ushorts
        ushort numGroups = reader.ReadUInt16();
        _level.AnimatedTextures = new();
        for (int i = 0; i < numGroups; i++)
        {
            _level.AnimatedTextures.Add(TR2FileReadUtilities.ReadAnimatedTexture(reader));
        }

        //Entities
        uint numEntities = reader.ReadUInt32();
        _level.Entities = reader.ReadTR1Entities(numEntities);

        _level.LightMap = new(reader.ReadBytes(TRConsts.LightMapSize));
        _level.Palette = reader.ReadColours(TRConsts.PaletteSize);

        //Cinematic Frames
        ushort numCinematicFrames = reader.ReadUInt16();
        _level.CinematicFrames = new();
        for (int i = 0; i < numCinematicFrames; i++)
        {
            _level.CinematicFrames.Add(TR2FileReadUtilities.ReadCinematicFrame(reader));
        }

        ushort numDemoData = reader.ReadUInt16();
        _level.DemoData = reader.ReadBytes(numDemoData);

        ReadSoundEffects(reader);
    }

    protected override void Write(TRLevelWriter writer)
    {
        writer.Write((uint)_level.Images8.Count);
        writer.Write(_level.Images8);

        writer.Write(_level.Version.LevelNumber);

        _spriteBuilder.CacheSpriteOffsets(_level.Sprites);
        writer.Write((ushort)_level.Rooms.Count);
        foreach (TR1Room room in _level.Rooms) { writer.Write(room.Serialize()); }

        writer.Write((uint)_level.FloorData.Count);
        writer.Write(_level.FloorData);

        WriteMeshData(writer);
        WriteModelData(writer);

        WriteStaticMeshes(writer);

        writer.Write((uint)_level.ObjectTextures.Count);
        foreach (TRObjectTexture tex in _level.ObjectTextures) { writer.Write(tex.Serialize()); }
        WriteSprites(writer);

        writer.Write((uint)_level.Cameras.Count);
        foreach (TRCamera cam in _level.Cameras) { writer.Write(cam.Serialize()); }

        writer.Write((uint)_level.SoundSources.Count);
        foreach (TRSoundSource src in _level.SoundSources) { writer.Write(src.Serialize()); }

        writer.Write((uint)_level.Boxes.Count);
        foreach (TRBox box in _level.Boxes) { writer.Write(box.Serialize()); }
        writer.Write((uint)_level.Overlaps.Count);
        writer.Write(_level.Overlaps);
        writer.Write(TR1BoxUtilities.FlattenZones(_level.Zones));

        byte[] animTextureData = _level.AnimatedTextures.SelectMany(a => a.Serialize()).ToArray();
        writer.Write((uint)(animTextureData.Length / sizeof(ushort)) + 1);
        writer.Write((ushort)_level.AnimatedTextures.Count);
        writer.Write(animTextureData);

        writer.Write((uint)_level.Entities.Count);
        writer.Write(_level.Entities);

        Debug.Assert(_level.LightMap.Count == TRConsts.LightMapSize);
        Debug.Assert(_level.Palette.Count == TRConsts.PaletteSize);
        writer.Write(_level.LightMap.ToArray());
        writer.Write(_level.Palette);

        writer.Write((ushort)_level.CinematicFrames.Count);
        foreach (TRCinematicFrame cineframe in _level.CinematicFrames) { writer.Write(cineframe.Serialize()); }

        writer.Write((ushort)_level.DemoData.Length);
        writer.Write(_level.DemoData);

        WriteSoundEffects(writer);
    }

    private void ReadMeshData(TRLevelReader reader)
    {
        _meshBuilder.BuildObjectMeshes(reader);
    }

    private void WriteMeshData(TRLevelWriter writer)
    {
        _meshBuilder.WriteObjectMeshes(writer, _level.Models.Values.SelectMany(m => m.Meshes), _level.StaticMeshes);
    }

    private void ReadModelData(TRLevelReader reader)
    {
        TRModelBuilder<TR1Type> builder = new(TRGameVersion.TR1, _observer);
        _level.Models = builder.ReadModelData(reader, _meshBuilder);
    }

    private void WriteModelData(TRLevelWriter writer)
    {
        TRModelBuilder<TR1Type> builder = new(TRGameVersion.TR1, _observer);
        builder.WriteModelData(writer, _level.Models);
    }

    private void ReadStaticMeshes(TRLevelReader reader)
    {
        _level.StaticMeshes = _meshBuilder.ReadStaticMeshes(reader, TR1Type.SceneryBase);
    }

    private void WriteStaticMeshes(TRLevelWriter writer)
    {
        _meshBuilder.WriteStaticMeshes(writer, _level.StaticMeshes, TR1Type.SceneryBase);
    }

    private void ReadSprites(TRLevelReader reader)
    {
        _level.Sprites = _spriteBuilder.ReadSprites(reader);
    }

    private void WriteSprites(TRLevelWriter writer)
    {
        _spriteBuilder.WriteSprites(writer, _level.Sprites);
    }

    private static TR1RoomMesh ConvertToRoomData(ushort[] rawData)
    {
        // This approach is temporarily retained

        TR1RoomMesh roomData = new()
        {
            Vertices = new()
        };

        int offset = 0;
        ushort count = rawData[offset++];
        for (int j = 0; j < count; j++)
        {
            roomData.Vertices.Add(new()
            {
                Vertex = new()
                {
                    X = UnsafeConversions.UShortToShort(rawData[offset++]),
                    Y = UnsafeConversions.UShortToShort(rawData[offset++]),
                    Z = UnsafeConversions.UShortToShort(rawData[offset++]),
                },
                Lighting = UnsafeConversions.UShortToShort(rawData[offset++]),
            });
        }

        count = rawData[offset++];
        roomData.Rectangles = new();
        for (int j = 0; j < count; j++)
        {
            roomData.Rectangles.Add(new()
            {
                Vertices = new ushort[]
                {
                    rawData[offset++],
                    rawData[offset++],
                    rawData[offset++],
                    rawData[offset++],
                },
                Texture = rawData[offset++],
            });
        }

        count = rawData[offset++];
        roomData.Triangles = new();
        for (int j = 0; j < count; j++)
        {
            roomData.Triangles.Add(new()
            {
                Vertices = new ushort[]
                {
                    rawData[offset++],
                    rawData[offset++],
                    rawData[offset++],
                },
                Texture = rawData[offset++],
            });
        }

        count = rawData[offset++];
        roomData.Sprites = new();
        for (int j = 0; j < count; j++)
        {
            roomData.Sprites.Add(new()
            {
                Vertex = UnsafeConversions.UShortToShort(rawData[offset++]),
                Texture = UnsafeConversions.UShortToShort(rawData[offset++]),
            });
        }

        Debug.Assert(offset == rawData.Length);

        return roomData;
    }

    private void ReadSoundEffects(TRLevelReader reader)
    {
        _level.SoundEffects = new();
        short[] soundMap = reader.ReadInt16s(Enum.GetValues<TR1SFX>().Length);

        uint numSoundDetails = reader.ReadUInt32();
        List<TR1SoundEffect> sfx = new();

        Dictionary<int, ushort> sampleMap = new();
        for (int i = 0; i < numSoundDetails; i++)
        {
            sampleMap[i] = reader.ReadUInt16();
            sfx.Add(new()
            {
                Volume = reader.ReadUInt16(),
                Chance = reader.ReadUInt16(),
                Samples = new()
            });

            sfx[i].SetFlags(reader.ReadUInt16());
        }

        uint numSamples = reader.ReadUInt32();
        byte[] allSamples = reader.ReadUInt8s(numSamples);

        uint numSampleIndices = reader.ReadUInt32();
        uint[] sampleIndices = reader.ReadUInt32s(numSampleIndices);

        foreach (int soundID in sampleMap.Keys)
        {
            TR1SoundEffect effect = sfx[soundID];
            ushort baseIndex = sampleMap[soundID];
            for (int i = 0; i < effect.Samples.Capacity; i++)
            {
                uint start = sampleIndices[baseIndex + i];
                uint end = baseIndex + i + 1 == sampleIndices.Length
                    ? (uint)allSamples.Length
                    : sampleIndices[baseIndex + i + 1];

                byte[] sample = new byte[end - start];
                for (int j = 0; j < sample.Length; j++)
                {
                    sample[j] = allSamples[start + j];
                }

                effect.Samples.Add(sample);
            }
        }

        for (int i = 0; i < soundMap.Length; i++)
        {
            if (soundMap[i] == -1)
            {
                continue;
            }

            _level.SoundEffects[(TR1SFX)i] = sfx[soundMap[i]];
        }
    }

    private void WriteSoundEffects(TRLevelWriter writer)
    {
        short detailsIndex = 0;
        List<uint> samplePointers = new();
        List<byte> wavData = new();

        foreach (TR1SFX id in Enum.GetValues<TR1SFX>())
        {
            writer.Write(_level.SoundEffects.ContainsKey(id) ? detailsIndex++ : (short)-1);
        }

        writer.Write((uint)_level.SoundEffects.Count);
        foreach (var (_, effect) in _level.SoundEffects)
        {
            writer.Write((ushort)samplePointers.Count);
            writer.Write(effect.Volume);
            writer.Write(effect.Chance);
            writer.Write(effect.GetFlags());

            foreach (byte[] wav in effect.Samples)
            {
                samplePointers.Add((uint)wavData.Count);
                wavData.AddRange(wav);
            }
        }

        writer.Write((uint)wavData.Count);
        writer.Write(wavData);

        writer.Write((uint)samplePointers.Count);
        writer.Write(samplePointers);
    }
}
