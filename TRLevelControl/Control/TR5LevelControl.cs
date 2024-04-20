using System.Diagnostics;
using TRLevelControl.Build;
using TRLevelControl.Model;

namespace TRLevelControl;

public class TR5LevelControl : TRLevelControlBase<TR5Level>
{
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

        //reader.ReadUInt16(); // Unused, always 0 //IF we eliminate demodata

        ReadSoundEffects(reader);
        ReadWAVData(reader);
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

        //writer.Write((ushort)0); //IF we eliminate demodata

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
        TRObjectMeshBuilder builder = new(_observer);
        builder.BuildObjectMeshes(reader);

        _level.Meshes = builder.Meshes;
        _level.MeshPointers = builder.MeshPointers;
    }

    private void WriteMeshData(TRLevelWriter writer)
    {
        TRObjectMeshBuilder builder = new(_observer);
        List<byte> meshData = _level.Meshes.SelectMany(m => builder.Serialize(m)).ToList();

        writer.Write((uint)meshData.Count / 2);
        writer.Write(meshData.ToArray());

        writer.Write((uint)_level.MeshPointers.Count);
        foreach (uint data in _level.MeshPointers)
        {
            writer.Write(data);
        }
    }

    private void ReadModelData(TRLevelReader reader)
    {
        TR5FileReadUtilities.PopulateAnimations(reader, _level);
        TR5FileReadUtilities.PopulateMeshTreesFramesModels(reader, _level);
    }

    private void WriteModelData(TRLevelWriter writer)
    {
        writer.Write((uint)_level.Animations.Count);
        foreach (TR4Animation anim in _level.Animations)
        {
            writer.Write(anim.Serialize());
        }

        writer.Write((uint)_level.StateChanges.Count);
        foreach (TRStateChange sc in _level.StateChanges)
        {
            writer.Write(sc.Serialize());
        }

        writer.Write((uint)_level.AnimDispatches.Count);
        foreach (TRAnimDispatch ad in _level.AnimDispatches)
        {
            writer.Write(ad.Serialize());
        }

        writer.Write((uint)_level.AnimCommands.Count);
        foreach (TRAnimCommand ac in _level.AnimCommands)
        {
            writer.Write(ac.Serialize());
        }

        writer.Write((uint)_level.MeshTrees.Count * 4); //To get the correct number /= 4 is done during read, make sure to reverse it here.
        foreach (TRMeshTreeNode node in _level.MeshTrees)
        {
            writer.Write(node.Serialize());
        }

        writer.Write((uint)_level.Frames.Count);
        foreach (ushort frame in _level.Frames)
        {
            writer.Write(frame);
        }

        writer.Write((uint)_level.Models.Count);
        foreach (TR5Model model in _level.Models)
        {
            writer.Write(model.Serialize());
        }
    }

    private void ReadStaticMeshes(TRLevelReader reader)
    {
        TR5FileReadUtilities.PopulateStaticMeshes(reader, _level);
    }

    private void WriteStaticMeshes(TRLevelWriter writer)
    {
        writer.Write((uint)_level.StaticMeshes.Count);
        foreach (TRStaticMesh sm in _level.StaticMeshes)
        {
            writer.Write(sm.Serialize());
        }
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

    private void ReadSoundEffects(TRLevelReader reader)
    {
        TR5FileReadUtilities.PopulateDemoSoundSampleIndices(reader, _level);
        reader.ReadBytes(6); // Always 0xCD
    }

    private void WriteSoundEffects(TRLevelWriter writer)
    {
        writer.Write((ushort)_level.DemoData.Length);
        writer.Write(_level.DemoData);

        foreach (short sound in _level.SoundMap)
        {
            writer.Write(sound);
        }

        writer.Write((uint)_level.SoundDetails.Count);
        foreach (TR3SoundDetails snd in _level.SoundDetails)
        {
            writer.Write(snd.Serialize());
        }

        writer.Write((uint)_level.SampleIndices.Count);
        foreach (uint sampleindex in _level.SampleIndices)
        {
            writer.Write(sampleindex);
        }

        writer.Write(Enumerable.Repeat((byte)0xCD, 6).ToArray());
    }

    private void ReadWAVData(TRLevelReader reader)
    {
        uint numSamples = reader.ReadUInt32();
        _level.Samples = new();

        for (int i = 0; i < numSamples; i++)
        {
            _level.Samples.Add(new()
            {
                UncompSize = reader.ReadUInt32(),
                CompSize = reader.ReadUInt32(),
            });

            _level.Samples[i].CompressedChunk = reader.ReadBytes((int)_level.Samples[i].CompSize);
        }
    }

    private void WriteWAVData(TRLevelWriter writer)
    {
        writer.Write((uint)_level.Samples.Count);
        foreach (TR4Sample sample in _level.Samples)
        {
            writer.Write(sample.UncompSize);
            writer.Write(sample.CompSize);
            writer.Write(sample.CompressedChunk);
        }
    }
}
