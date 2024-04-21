using System.Diagnostics;
using TRLevelControl.Build;
using TRLevelControl.Model;

namespace TRLevelControl;

public class TR4LevelControl : TRLevelControlBase<TR4Level>
{
    public TR4LevelControl(ITRLevelObserver observer = null)
        : base(observer) { }

    protected override TR4Level CreateLevel(TRFileVersion version)
    {
        TR4Level level = new()
        {
            Version = new()
            {
                Game = TRGameVersion.TR4,
                File = version
            }
        };

        TestVersion(level, TRFileVersion.TR45);
        return level;
    }

    protected override void Read(TRLevelReader reader)
    {
        ReadImages(reader);
        ReadLevelDataChunk(reader);
        ReadWAVData(reader);
    }

    protected override void Write(TRLevelWriter writer)
    {
        WriteImages(writer);
        WriteLevelDataChunk(writer);
        WriteWAVData(writer);
    }

    private void ReadImages(TRLevelReader reader)
    {
        _level.Images = new();
        ushort roomCount = reader.ReadUInt16();
        ushort objectCount = reader.ReadUInt16();
        ushort bumpCount = reader.ReadUInt16();

        using TRLevelReader reader32 = reader.Inflate(TRChunkType.Images32);
        _level.Images.Rooms.Images32 = reader32.ReadImage32s(roomCount);
        _level.Images.Objects.Images32 = reader32.ReadImage32s(objectCount);
        _level.Images.Bump.Images32 = reader32.ReadImage32s(bumpCount);

        using TRLevelReader reader16 = reader.Inflate(TRChunkType.Images16);
        _level.Images.Rooms.Images16 = reader16.ReadImage16s(roomCount);
        _level.Images.Objects.Images16 = reader16.ReadImage16s(objectCount);
        _level.Images.Bump.Images16 = reader16.ReadImage16s(bumpCount);

        using TRLevelReader skyReader = reader.Inflate(TRChunkType.SkyFont);
        _level.Images.Font = skyReader.ReadImage32();
        _level.Images.Sky = skyReader.ReadImage32();
    }

    private void WriteImages(TRLevelWriter writer)
    {
        Debug.Assert(_level.Images.Rooms.Images32.Count == _level.Images.Rooms.Images16.Count);
        Debug.Assert(_level.Images.Objects.Images32.Count == _level.Images.Objects.Images16.Count);
        Debug.Assert(_level.Images.Bump.Images32.Count == _level.Images.Bump.Images16.Count);

        writer.Write((ushort)_level.Images.Rooms.Images32.Count);
        writer.Write((ushort)_level.Images.Objects.Images32.Count);
        writer.Write((ushort)_level.Images.Bump.Images32.Count);

        using TRLevelWriter writer32 = new();
        writer32.Write(_level.Images.Rooms.Images32);
        writer32.Write(_level.Images.Objects.Images32);
        writer32.Write(_level.Images.Bump.Images32);
        writer.Deflate(writer32, TRChunkType.Images32);

        using TRLevelWriter writer16 = new();
        writer16.Write(_level.Images.Rooms.Images16);
        writer16.Write(_level.Images.Objects.Images16);
        writer16.Write(_level.Images.Bump.Images16);
        writer.Deflate(writer16, TRChunkType.Images16);

        using TRLevelWriter skyWriter = new();
        skyWriter.Write(_level.Images.Font);
        skyWriter.Write(_level.Images.Sky);
        writer.Deflate(skyWriter, TRChunkType.SkyFont);
    }

    private void ReadLevelDataChunk(TRLevelReader mainReader)
    {
        using TRLevelReader reader = mainReader.Inflate(TRChunkType.LevelData);

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

        reader.ReadUInt16s(3); // Always 0s
        Debug.Assert(reader.BaseStream.Position == reader.BaseStream.Length);
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

        writer.Write(Enumerable.Repeat((ushort)0, 3).ToArray());

        mainWriter.Deflate(writer, TRChunkType.LevelData);
    }

    private void ReadRooms(TRLevelReader reader)
    {
        TR4FileReadUtilities.PopulateRooms(reader, _level);
        TR4FileReadUtilities.PopulateFloordata(reader, _level);
    }

    private void WriteRooms(TRLevelWriter writer)
    {
        writer.Write((ushort)_level.Rooms.Count);
        foreach (TR4Room room in _level.Rooms)
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
        TR4FileReadUtilities.PopulateAnimations(reader, _level);
        TR4FileReadUtilities.PopulateMeshTreesFramesModels(reader, _level);
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
        foreach (TRModel model in _level.Models)
        {
            writer.Write(model.Serialize());
        }
    }

    private void ReadStaticMeshes(TRLevelReader reader)
    {
        TR4FileReadUtilities.PopulateStaticMeshes(reader, _level);
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
        TR4FileReadUtilities.PopulateSprites(reader, _level);
    }

    private void WriteSprites(TRLevelWriter writer)
    {
        writer.Write(TR4FileReadUtilities.SPRMarker.ToCharArray());

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
        TR4FileReadUtilities.PopulateCameras(reader, _level);
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
        TR4FileReadUtilities.PopulateSoundSources(reader, _level);
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
        TR4FileReadUtilities.PopulateBoxesOverlapsZones(reader, _level);
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
        TR4FileReadUtilities.PopulateAnimatedTextures(reader, _level);
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
        TR4FileReadUtilities.PopulateObjectTextures(reader, _level);
    }

    private void WriteObjectTextures(TRLevelWriter writer)
    {
        writer.Write(TR4FileReadUtilities.TEXMarker.ToCharArray());

        writer.Write((uint)_level.ObjectTextures.Count);
        foreach (TR4ObjectTexture otex in _level.ObjectTextures)
        {
            writer.Write(otex.Serialize());
        }
    }

    private void ReadEntities(TRLevelReader reader)
    {
        TR4FileReadUtilities.PopulateEntitiesAndAI(reader, _level);
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
        short[] soundMap = reader.ReadInt16s(Enum.GetValues<TR4SFX>().Length);

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
            if (soundMap[i] < 0 || soundMap[i] >= sfx.Count)
            {
                continue;
            }

            _level.SoundEffects[(TR4SFX)i] = sfx[soundMap[i]];
        }
    }

    private void WriteSoundEffects(TRLevelWriter writer)
    {
        short detailsIndex = 0;
        List<uint> sampleIndices = new();
        List<TR4Sample> samples = new();

        foreach (TR4SFX id in Enum.GetValues<TR4SFX>())
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

        // Sample indices are not required, but write them anyway to match OG
        IEnumerable<uint> outputIndices = _observer?.GetSampleIndices() ?? sampleIndices;
        writer.Write((uint)outputIndices.Count());
        writer.Write(outputIndices);
    }

    private void ReadWAVData(TRLevelReader reader)
    {
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

        int pos = 0;
        foreach (TR4SoundEffect sfx in _level.SoundEffects.Values)
        {
            for (int i = 0; i < sfx.Samples.Capacity; i++)
            {
                sfx.Samples.Add(samples[pos++]);
            }
        }
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
