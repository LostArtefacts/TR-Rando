using System.Diagnostics;
using TRLevelControl.Build;
using TRLevelControl.Model;

namespace TRLevelControl;

public class TR5LevelControl : TRLevelControlBase<TR5Level>
{
    private readonly TRObjectMeshBuilder _meshBuilder;

    public TR5LevelControl(ITRLevelObserver observer = null)
        : base(observer)
    {
        _meshBuilder = new(TRGameVersion.TR5, _observer);
    }

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

        TestVersion(level, TRFileVersion.TR45);
        return level;
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

    private void WriteImages(TRLevelWriter writer)
    {
        Debug.Assert(_level.Images.Rooms.Images32.Count == _level.Images.Rooms.Images16.Count);
        Debug.Assert(_level.Images.Objects.Images32.Count == _level.Images.Objects.Images16.Count);

        writer.Write((ushort)_level.Images.Rooms.Images32.Count);
        writer.Write((ushort)_level.Images.Objects.Images32.Count);
        writer.Write((ushort)0); // No bump

        using TRLevelWriter writer32 = new();
        writer32.Write(_level.Images.Rooms.Images32);
        writer32.Write(_level.Images.Objects.Images32);
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

    private void ReadLaraAndWeather(TRLevelReader reader)
    {
        _level.LaraType = reader.ReadUInt16();
        _level.WeatherType = reader.ReadUInt16();

        reader.ReadBytes(28); // Padding
    }

    private void WriteLaraAndWeather(TRLevelWriter writer)
    {
        writer.Write(_level.LaraType);
        writer.Write(_level.WeatherType);

        writer.Write(Enumerable.Repeat((byte)0, 28).ToArray());
    }

    private void ReadLevelDataChunk(TRLevelReader reader)
    {
        // TR5 level chunk is not compressed.
        uint expectedLength = reader.ReadUInt32();
        uint compressedLength = reader.ReadUInt32();
        Debug.Assert(expectedLength == compressedLength);

        reader.ReadUInt32(); // Unused, always 0

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

        ReadDemoData(reader);

        ReadSoundEffects(reader);
    }

    private void WriteLevelDataChunk(TRLevelWriter mainWriter)
    {
        using TRLevelWriter writer = new();

        writer.Write((uint)0); // Unused, always 0

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

        WriteDemoData(writer);

        WriteSoundEffects(writer);

        // Simulate zipping
        byte[] data = (writer.BaseStream as MemoryStream).ToArray();
        mainWriter.Write((uint)data.Length);
        mainWriter.Write((uint)data.Length);
        mainWriter.Write(data);

        WriteWAVData(mainWriter);
    }

    private void ReadRooms(TRLevelReader reader)
    {
        TR5FileReadUtilities.PopulateRooms(reader, _level);
        TR5FileReadUtilities.PopulateFloordata(reader, _level);
    }

    private void WriteRooms(TRLevelWriter writer)
    {
        writer.Write((uint)_level.Rooms.Count);
        foreach (TR5Room room in _level.Rooms)
        {
            writer.Write(room.Serialize());
        }

        writer.Write((uint)_level.FloorData.Count);
        writer.Write(_level.FloorData);
    }

    private void ReadMeshData(TRLevelReader reader)
    {
        _meshBuilder.BuildObjectMeshes(reader);
    }

    private void WriteMeshData(TRLevelWriter writer)
    {
        _meshBuilder.WriteObjectMeshes(writer, _level.Models.SelectMany(m => m.Meshes), _level.StaticMeshes);
    }

    private void ReadModelData(TRLevelReader reader)
    {
        TRModelBuilder builder = new(TRGameVersion.TR5, _observer);
        _level.Models = builder.ReadModelData(reader, _meshBuilder);
    }

    private void WriteModelData(TRLevelWriter writer)
    {
        TRModelBuilder builder = new(TRGameVersion.TR5, _observer);
        builder.WriteModelData(writer, _level.Models);
    }

    private void ReadStaticMeshes(TRLevelReader reader)
    {
        _level.StaticMeshes = _meshBuilder.ReadStaticMeshes(reader);
    }

    private void WriteStaticMeshes(TRLevelWriter writer)
    {
        _meshBuilder.WriteStaticMeshes(writer, _level.StaticMeshes);
    }

    private void ReadSprites(TRLevelReader reader)
    {
        TR5FileReadUtilities.VerifySPRMarker(reader);
        TR5FileReadUtilities.PopulateSprites(reader, _level);
    }

    private void WriteSprites(TRLevelWriter writer)
    {
        writer.Write(TR5FileReadUtilities.SPRMarker.ToCharArray());

        writer.Write((uint)_level.SpriteTextures.Count);
        foreach (TRSpriteTexture st in _level.SpriteTextures)
        {
            writer.Write(st.Serialize());
        }

        writer.Write((uint)_level.SpriteSequences.Count);
        foreach (TRSpriteSequence seq in _level.SpriteSequences)
        {
            writer.Write(seq.Serialize());
        }
    }

    private void ReadCameras(TRLevelReader reader)
    {
        TR5FileReadUtilities.PopulateCameras(reader, _level);
    }

    private void WriteCameras(TRLevelWriter writer)
    {
        writer.Write((uint)_level.Cameras.Count);
        foreach (TRCamera cam in _level.Cameras)
        {
            writer.Write(cam.Serialize());
        }

        writer.Write((uint)_level.FlybyCameras.Count);
        foreach (TR4FlyByCamera flycam in _level.FlybyCameras)
        {
            writer.Write(flycam.Serialize());
        }
    }

    private void ReadSoundSources(TRLevelReader reader)
    {
        TR5FileReadUtilities.PopulateSoundSources(reader, _level);
    }

    private void WriteSoundSources(TRLevelWriter writer)
    {
        writer.Write((uint)_level.SoundSources.Count);
        foreach (TRSoundSource ssrc in _level.SoundSources)
        {
            writer.Write(ssrc.Serialize());
        }
    }

    private void ReadBoxes(TRLevelReader reader)
    {
        TR5FileReadUtilities.PopulateBoxesOverlapsZones(reader, _level);
    }

    private void WriteBoxes(TRLevelWriter writer)
    {
        writer.Write((uint)_level.Boxes.Count);
        foreach (TR2Box box in _level.Boxes)
        {
            writer.Write(box.Serialize());
        }

        writer.Write((uint)_level.Overlaps.Count);
        foreach (ushort overlap in _level.Overlaps)
        {
            writer.Write(overlap);
        }

        foreach (short zone in _level.Zones)
        {
            writer.Write(zone);
        }
    }

    private void ReadAnimatedTextures(TRLevelReader reader)
    {
        TR5FileReadUtilities.PopulateAnimatedTextures(reader, _level);
    }

    private void WriteAnimatedTextures(TRLevelWriter writer)
    {
        byte[] animTextureData = _level.AnimatedTextures.SelectMany(a => a.Serialize()).ToArray();
        writer.Write((uint)(animTextureData.Length / sizeof(ushort)) + 1);
        writer.Write((ushort)_level.AnimatedTextures.Count);
        writer.Write(animTextureData);
        writer.Write(_level.AnimatedTexturesUVCount);
    }

    private void ReadObjectTextures(TRLevelReader reader)
    {
        TR5FileReadUtilities.VerifyTEXMarker(reader);
        TR5FileReadUtilities.PopulateObjectTextures(reader, _level);
    }

    private void WriteObjectTextures(TRLevelWriter writer)
    {
        writer.Write(TR5FileReadUtilities.TEXMarker.ToCharArray());

        writer.Write((uint)_level.ObjectTextures.Count);
        foreach (TR5ObjectTexture otex in _level.ObjectTextures)
        {
            writer.Write(otex.Serialize());
        }
    }

    private void ReadEntities(TRLevelReader reader)
    {
        TR5FileReadUtilities.PopulateEntitiesAndAI(reader, _level);
    }

    private void WriteEntities(TRLevelWriter writer)
    {
        writer.Write((uint)_level.Entities.Count);
        writer.Write(_level.Entities);

        writer.Write((uint)_level.AIEntities.Count);
        writer.Write(_level.AIEntities);
    }

    private void ReadDemoData(TRLevelReader reader)
    {
        ushort numDemoData = reader.ReadUInt16();
        _level.DemoData = reader.ReadBytes(numDemoData);
    }

    private void WriteDemoData(TRLevelWriter writer)
    {
        writer.Write((ushort)_level.DemoData.Length);
        writer.Write(_level.DemoData);
    }

    private void ReadSoundEffects(TRLevelReader reader)
    {
        _level.SoundEffects = new();
        short[] soundMap = reader.ReadInt16s(Enum.GetValues<TR5SFX>().Length);

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

        for (int i = 0; i < soundMap.Length; i++)
        {
            if (soundMap[i] == -1)
                continue;

            _level.SoundEffects[(TR5SFX)i] = sfx[soundMap[i]];
        }

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
