using System.Diagnostics;
using TRLevelControl.Build;
using TRLevelControl.Model;

namespace TRLevelControl;

public class TR5LevelControl : TRLevelControlBase<TR5Level>
{
    private TRObjectMeshBuilder<TR5Type> _meshBuilder;
    private TRTextureBuilder _textureBuilder;
    private TRSpriteBuilder<TR5Type> _spriteBuilder;

    public TR5LevelControl(ITRLevelObserver observer = null)
        : base(observer) { }

    protected override TR5Level CreateLevel(TRFileVersion version)
    {
        TR5Level level = new()
        {
            Version = new()
            {
                Game = TRGameVersion.TR5,
                File = version
            }
        };

        TestVersion(level, TRFileVersion.TR45, TRFileVersion.TRR5);
        return level;
    }

    protected override void Initialise()
    {
        _meshBuilder = new(TRGameVersion.TR5, _observer);
        _textureBuilder = new(TRGameVersion.TR5, _observer);
        _spriteBuilder = new(TRGameVersion.TR5);
    }

    protected override void Read(TRLevelReader reader)
    {
        ReadImages(reader);
        ReadLaraAndWeather(reader);
        ReadLevelDataChunk(reader);
    }

    protected override void Write(TRLevelWriter writer)
    {
        WriteImages(writer);
        WriteLaraAndWeather(writer);
        WriteLevelDataChunk(writer);
    }

    private void ReadImages(TRLevelReader reader)
    {
        _level.Images = new();
        ushort roomCount = reader.ReadUInt16();
        ushort objectCount = reader.ReadUInt16();
        reader.ReadUInt16(); // Previously bump in TR4, no longer used

        if (_level.Version.File == TRFileVersion.TR45)
        {
            using TRLevelReader reader32 = reader.Inflate(TRChunkType.Images32);
            _level.Images.Rooms.Images32 = reader32.ReadImage32s(roomCount);
            _level.Images.Objects.Images32 = reader32.ReadImage32s(objectCount);

            using TRLevelReader reader16 = reader.Inflate(TRChunkType.Images16);
            _level.Images.Rooms.Images16 = reader16.ReadImage16s(roomCount);
            _level.Images.Objects.Images16 = reader16.ReadImage16s(objectCount);

            using TRLevelReader skyReader = reader.Inflate(TRChunkType.SkyFont);
            _level.Images.Shine = skyReader.ReadImage32();
            _level.Images.Font = skyReader.ReadImage32();
            _level.Images.Sky = skyReader.ReadImage32();
        }
        else
        {
            reader.ReadUInt32();
            _level.Images.Rooms.Images32 = reader.ReadImage32s(roomCount);
            _level.Images.Objects.Images32 = reader.ReadImage32s(objectCount);
            _level.Images.Shine = reader.ReadImage32();
            _level.Images.Font = reader.ReadImage32();
            _level.Images.Sky = reader.ReadImage32();
        }
    }

    private void WriteImages(TRLevelWriter writer)
    {
        writer.Write((ushort)_level.Images.Rooms.Images32.Count);
        writer.Write((ushort)_level.Images.Objects.Images32.Count);
        writer.Write((ushort)0); // No bump

        using TRLevelWriter writer32 = new();
        writer32.Write(_level.Images.Rooms.Images32);
        writer32.Write(_level.Images.Objects.Images32);

        if (_level.Version.File == TRFileVersion.TR45)
        {
            Debug.Assert(_level.Images.Rooms.Images32.Count == _level.Images.Rooms.Images16.Count);
            Debug.Assert(_level.Images.Objects.Images32.Count == _level.Images.Objects.Images16.Count);

            writer.Deflate(writer32, TRChunkType.Images32);

            using TRLevelWriter writer16 = new();
            writer16.Write(_level.Images.Rooms.Images16);
            writer16.Write(_level.Images.Objects.Images16);
            writer.Deflate(writer16, TRChunkType.Images16);

            using TRLevelWriter skyWriter = new();
            skyWriter.Write(_level.Images.Shine);
            skyWriter.Write(_level.Images.Font);
            skyWriter.Write(_level.Images.Sky);
            writer.Deflate(skyWriter, TRChunkType.SkyFont);
        }
        else
        {
            writer.Write((uint)writer32.BaseStream.Length);
            writer32.BaseStream.Position = 0;
            writer32.BaseStream.CopyTo(writer.BaseStream);
            writer.Write(_level.Images.Shine);
            writer.Write(_level.Images.Font);
            writer.Write(_level.Images.Sky);
        }
    }

    private void ReadLaraAndWeather(TRLevelReader reader)
    {
        _level.LaraType = reader.ReadUInt16();
        if (_level.Version.File == TRFileVersion.TR45)
        {
            _level.WeatherType = reader.ReadUInt16();
            reader.ReadBytes(28); // Padding
        }
    }

    private void WriteLaraAndWeather(TRLevelWriter writer)
    {
        writer.Write(_level.LaraType);
        if (_level.Version.File == TRFileVersion.TR45)
        {
            writer.Write(_level.WeatherType);
            writer.Write(Enumerable.Repeat((byte)0, 28));
        }
    }

    private void ReadLevelDataChunk(TRLevelReader reader)
    {
        if (_level.Version.File == TRFileVersion.TR45)
        {
            // TR5 level chunk is not compressed.
            uint expectedLength = reader.ReadUInt32();
            uint compressedLength = reader.ReadUInt32();
            Debug.Assert(expectedLength == compressedLength);

            _level.Version.LevelNumber = reader.ReadUInt32();
        }

        ReadRooms(reader);

        ReadMeshData(reader);
        ReadModelData(reader);

        ReadStaticMeshes(reader);

        ReadSprites(reader);

        ReadCameras(reader);
        ReadSoundSources(reader);

        ReadBoxes(reader);

        ReadAnimatedTextures(reader);
        ReadObjectTextures(reader);

        ReadEntities(reader);

        if (_level.Version.File == TRFileVersion.TR45)
        {
            ReadDemoData(reader);
            ReadSoundEffects(reader);
        }
    }

    private void WriteLevelDataChunk(TRLevelWriter mainWriter)
    {
        using TRLevelWriter writer = new();

        if (_level.Version.File == TRFileVersion.TR45)
        {
            writer.Write(_level.Version.LevelNumber);
        }

        WriteRooms(writer);

        WriteMeshData(writer);
        WriteModelData(writer);

        WriteStaticMeshes(writer);

        WriteSprites(writer);

        WriteCameras(writer);
        WriteSoundSources(writer);

        WriteBoxes(writer);

        WriteAnimatedTextures(writer);
        WriteObjectTextures(writer);

        WriteEntities(writer);

        if (_level.Version.File == TRFileVersion.TR45)
        {
            WriteDemoData(writer);

            WriteSoundEffects(writer);

            // Simulate zipping
            byte[] data = (writer.BaseStream as MemoryStream).ToArray();
            mainWriter.Write((uint)data.Length);
            mainWriter.Write((uint)data.Length);
            mainWriter.Write(data);

            WriteWAVData(mainWriter);
        }
        else
        {
            writer.BaseStream.Position = 0;
            writer.BaseStream.CopyTo(mainWriter.BaseStream);
        }
    }

    private void ReadRooms(TRLevelReader reader)
    {
        if (_observer?.UseTR5RawRooms ?? false)
        {
            ReadRawRooms(reader);
            return;
        }

        _level.Rooms = TR5RoomBuilder.ReadRooms(reader, _level.Version.File == TRFileVersion.TRR5);

        TRFDBuilder builder = new(_level.Version.Game, _observer);
        _level.FloorData = builder.ReadFloorData(reader, _level.Rooms.SelectMany(r => r.Sectors));
    }

    private void ReadRawRooms(TRLevelReader reader)
    {
        long position = reader.BaseStream.Position;
        TR5RoomBuilder.ReadRooms(reader, _level.Version.File == TRFileVersion.TRR5);
        long length = reader.BaseStream.Position - position;
        reader.BaseStream.Position = position;
        List<byte> data = new(reader.ReadUInt8s(length));

        uint numFloorData = reader.ReadUInt32();
        reader.BaseStream.Position -= sizeof(uint);
        data.AddRange(reader.ReadBytes(sizeof(uint)));
        data.AddRange(reader.ReadBytes((int)(numFloorData * sizeof(ushort))));

        _observer?.OnRawTR5RoomsRead(data);
    }

    private void WriteRooms(TRLevelWriter writer)
    {
        _spriteBuilder.CacheSpriteOffsets(_level.Sprites);

        if (_observer?.UseTR5RawRooms ?? false)
        {
            writer.Write(_observer.GetTR5Rooms());
            return;
        }

        List<ushort> floorData = _level.FloorData
            .Flatten(_level.Rooms.SelectMany(r => r.Sectors));
        TR5RoomBuilder.WriteRooms(writer, _level.Rooms, _level.Version.File == TRFileVersion.TRR5);

        writer.Write((uint)floorData.Count);
        writer.Write(floorData);
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
        TRModelBuilder<TR5Type> builder = new(TRGameVersion.TR5, TRModelDataType.Level, _observer, _level.Version.File == TRFileVersion.TRR5);
        _level.Models = builder.ReadModelData(reader, _meshBuilder);
    }

    private void WriteModelData(TRLevelWriter writer)
    {
        TRModelBuilder<TR5Type> builder = new(TRGameVersion.TR5, TRModelDataType.Level, _observer, _level.Version.File == TRFileVersion.TRR5);
        builder.WriteModelData(writer, _level.Models);
    }

    private void ReadStaticMeshes(TRLevelReader reader)
    {
        _level.StaticMeshes = _meshBuilder.ReadStaticMeshes(reader, TR5Type.SceneryBase);
    }

    private void WriteStaticMeshes(TRLevelWriter writer)
    {
        _meshBuilder.WriteStaticMeshes(writer, _level.StaticMeshes, TR5Type.SceneryBase);
    }

    private void ReadSprites(TRLevelReader reader)
    {
        _level.Sprites = _spriteBuilder.ReadSprites(reader, _level.Version.File == TRFileVersion.TRR5);
    }

    private void WriteSprites(TRLevelWriter writer)
    {
        _spriteBuilder.WriteSprites(writer, _level.Sprites, _level.Version.File == TRFileVersion.TRR5);
    }

    private void ReadCameras(TRLevelReader reader)
    {
        uint numCameras = reader.ReadUInt32();
        _level.Cameras = reader.ReadCameras(numCameras);

        TRFlybyBuilder builder = new(_observer);
        _level.Flybys = builder.ReadFlybys(reader);
    }

    private void WriteCameras(TRLevelWriter writer)
    {
        writer.Write((uint)_level.Cameras.Count);
        writer.Write(_level.Cameras);

        TRFlybyBuilder builder = new(_observer);
        builder.WriteFlybys(writer, _level.Flybys);
    }

    private void ReadSoundSources(TRLevelReader reader)
    {
        uint numSources = reader.ReadUInt32();
        _level.SoundSources = reader.ReadSoundSources<TR5SFX>(numSources);
    }

    private void WriteSoundSources(TRLevelWriter writer)
    {
        writer.Write((uint)_level.SoundSources.Count);
        writer.Write(_level.SoundSources);
    }

    private void ReadBoxes(TRLevelReader reader)
    {
        TRBoxBuilder boxBuilder = new(_level.Version.Game, _observer);
        _level.Boxes = boxBuilder.ReadBoxes(reader);
    }

    private void WriteBoxes(TRLevelWriter writer)
    {
        TRBoxBuilder boxBuilder = new(_level.Version.Game, _observer);
        boxBuilder.WriteBoxes(writer, _level.Boxes);
    }

    private void ReadAnimatedTextures(TRLevelReader reader)
    {
        _level.AnimatedTextures = _textureBuilder.ReadAnimatedTextures(reader);
    }

    private void WriteAnimatedTextures(TRLevelWriter writer)
    {
        _textureBuilder.Write(writer, _level.AnimatedTextures);
    }

    private void ReadObjectTextures(TRLevelReader reader)
    {
        _level.ObjectTextures = _textureBuilder.ReadObjectTextures(reader, _level.Version.File == TRFileVersion.TRR5);
    }

    private void WriteObjectTextures(TRLevelWriter writer)
    {
        _textureBuilder.Write(writer, _level.ObjectTextures, _level.Version.File == TRFileVersion.TRR5);
    }

    private void ReadEntities(TRLevelReader reader)
    {
        if (_level.Version.File == TRFileVersion.TR45)
        {
            uint numEntities = reader.ReadUInt32();
            _level.Entities = reader.ReadTR5Entities(numEntities);

            numEntities = reader.ReadUInt32();
            _level.AIEntities = reader.ReadTR5AIEntities(numEntities);
        }
        else
        {
            uint numEntities = reader.ReadUInt32();
            _level.AIEntities = reader.ReadTR5AIEntities(numEntities);

            numEntities = reader.ReadUInt32();
            _level.Entities = reader.ReadTR5Entities(numEntities);
        }
    }

    private void WriteEntities(TRLevelWriter writer)
    {
        if (_level.Version.File == TRFileVersion.TR45)
        {
            writer.Write((uint)_level.Entities.Count);
            writer.Write(_level.Entities);
            writer.Write((uint)_level.AIEntities.Count);
            writer.Write(_level.AIEntities);
        }
        else
        {
            writer.Write((uint)_level.AIEntities.Count);
            writer.Write(_level.AIEntities);
            writer.Write((uint)_level.Entities.Count);
            writer.Write(_level.Entities);
        }
    }

    private static void ReadDemoData(TRLevelReader reader)
    {
        ushort numDemoData = reader.ReadUInt16();
        Debug.Assert(numDemoData == 0);
    }

    private static void WriteDemoData(TRLevelWriter writer)
    {
        writer.Write((ushort)0);
    }

    private void ReadSoundEffects(TRLevelReader reader)
    {
        long startPos = reader.BaseStream.Position;
        try
        {
            ReadSoundEffects(reader, -1);
        }
        catch
        {
            reader.BaseStream.Position = startPos;
            ReadSoundEffects(reader, Enum.GetValues<TR5SFX>().Length);
        }
    }

    private void ReadSoundEffects(TRLevelReader reader, int mapLength)
    {
        List<short> soundMap = TRSFXBuilder.ReadSoundMap(reader, mapLength);

        uint numSoundDetails = reader.ReadUInt32();
        List<TR4SoundEffect> sfx = new();

        Dictionary<int, ushort> sampleMap = new();
        for (int i = 0; i < numSoundDetails; i++)
        {
            sampleMap[i] = reader.ReadUInt16();
            sfx.Add(new()
            {
                Volume = reader.ReadByte(),
                Range = reader.ReadByte(),
                Chance = reader.ReadByte(),
                Pitch = reader.ReadByte(),
                Samples = new()
            });

            sfx[i].SetFlags(reader.ReadUInt16());
        }

        // Sample indices are discarded in game. The details point to the samples
        // directly per ReadWAVData. Observe the reads here only.
        uint numSampleIndices = reader.ReadUInt32();
        uint[] sampleIndices = reader.ReadUInt32s(numSampleIndices);
        _observer?.OnSampleIndicesRead(sampleIndices);

        _level.SoundEffects = TRSFXBuilder.Build<TR5SFX, TR4SoundEffect>(soundMap, sfx);

        reader.ReadBytes(6); // OxCD padding

        uint numSamples = reader.ReadUInt32();
        List<TR4Sample> samples = new();

        for (int i = 0; i < numSamples; i++)
        {
            TR4Sample sample = new()
            {
                InflatedLength = reader.ReadUInt32()
            };
            samples.Add(sample);

            uint compressedSize = reader.ReadUInt32();
            sample.Data = reader.ReadUInt8s(compressedSize);
        }

        for (int i = 0; i < sfx.Count; i++)
        {
            TR4SoundEffect effect = sfx[i];
            for (int j = 0; j < effect.Samples.Capacity; j++)
            {
                effect.Samples.Add(samples[sampleMap[i] + j]);
            }
        }
    }

    private void WriteSoundEffects(TRLevelWriter writer)
    {
        short detailsIndex = 0;
        List<uint> sampleIndices = new();
        List<TR4Sample> samples = new();

        foreach (TR5SFX id in Enum.GetValues<TR5SFX>())
        {
            writer.Write(_level.SoundEffects.ContainsKey(id) ? detailsIndex++ : (short)-1);
        }

        writer.Write((uint)_level.SoundEffects.Count);
        foreach (TR4SoundEffect details in _level.SoundEffects.Values)
        {
            TR4Sample firstSample = details.Samples.First();
            int sampleIndex = samples.IndexOf(firstSample);
            if (sampleIndex == -1)
            {
                sampleIndex = samples.Count;
                samples.AddRange(details.Samples);
            }

            writer.Write((ushort)sampleIndex);
            writer.Write(details.Volume);
            writer.Write(details.Range);
            writer.Write(details.Chance);
            writer.Write(details.Pitch);
            writer.Write(details.GetFlags());

            sampleIndices.Add((uint)sampleIndex);
        }

        IEnumerable<uint> outputIndices = _observer?.GetSampleIndices() ?? sampleIndices;
        writer.Write((uint)outputIndices.Count());
        writer.Write(outputIndices);

        writer.Write(Enumerable.Repeat((byte)0xCD, 6));
    }

    private void WriteWAVData(TRLevelWriter writer)
    {
        List<TR4Sample> samples = _level.SoundEffects.Values
            .SelectMany(s => s.Samples)
            .Distinct()
            .ToList();

        writer.Write((uint)samples.Count);
        foreach (TR4Sample sample in samples)
        {
            writer.Write(sample.InflatedLength);
            writer.Write((uint)sample.Data.Length);
            writer.Write(sample.Data);
        }
    }
}
