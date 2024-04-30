using System.Diagnostics;
using TRLevelControl.Build;
using TRLevelControl.Model;

namespace TRLevelControl;

public class TR4LevelControl : TRLevelControlBase<TR4Level>
{
    private readonly TRObjectMeshBuilder<TR4Type> _meshBuilder;
    private readonly TRSpriteBuilder<TR4Type> _spriteBuilder;
    private readonly TR4RoomBuilder _roomBuilder;

    public TR4LevelControl(ITRLevelObserver observer = null)
        : base(observer)
    {
        _meshBuilder = new(TRGameVersion.TR4, _observer);
        _spriteBuilder = new(TRGameVersion.TR4);
        _roomBuilder = new();
    }

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
        ushort numRooms = reader.ReadUInt16();
        _level.Rooms = new();
        for (int i = 0; i < numRooms; i++)
        {
            TR4Room room = new()
            {
                Info = reader.ReadRoomInfo(_level.Version.Game)
            };
            _level.Rooms.Add(room);

            _roomBuilder.ReadRawMesh(reader);

            ushort numPortals = reader.ReadUInt16();
            room.Portals = reader.ReadRoomPortals(numPortals);

            room.NumZSectors = reader.ReadUInt16();
            room.NumXSectors = reader.ReadUInt16();
            room.Sectors = reader.ReadRoomSectors(room.NumXSectors * room.NumZSectors);

            room.AmbientIntensity = reader.ReadInt16();
            room.LightMode = reader.ReadInt16();
            ushort numLights = reader.ReadUInt16();
            room.Lights = new();
            for (int j = 0; j < numLights; j++)
            {
                room.Lights.Add(new()
                {
                    X = reader.ReadInt32(),
                    Y = reader.ReadInt32(),
                    Z = reader.ReadInt32(),
                    Colour = new()
                    {
                        Red = reader.ReadByte(),
                        Green = reader.ReadByte(),
                        Blue = reader.ReadByte()
                    },
                    LightType = reader.ReadByte(),
                    Unknown = reader.ReadByte(),
                    Intensity = reader.ReadByte(),
                    In = reader.ReadSingle(),
                    Out = reader.ReadSingle(),
                    Length = reader.ReadSingle(),
                    CutOff = reader.ReadSingle(),
                    Dx = reader.ReadSingle(),
                    Dy = reader.ReadSingle(),
                    Dz = reader.ReadSingle()
                });
            }

            ushort numStaticMeshes = reader.ReadUInt16();
            room.StaticMeshes = new();
            for (int j = 0; j < numStaticMeshes; j++)
            {
                room.StaticMeshes.Add(new()
                {
                    X = reader.ReadInt32(),
                    Y = reader.ReadInt32(),
                    Z = reader.ReadInt32(),
                    Angle = reader.ReadInt16(),
                    Colour = reader.ReadUInt16(),
                    Unused = reader.ReadUInt16(),
                    ID = TR4Type.SceneryBase + reader.ReadUInt16()
                });
            }

            room.AlternateRoom = reader.ReadInt16();
            room.Flags = reader.ReadInt16();
            room.WaterScheme = reader.ReadByte();
            room.ReverbInfo = reader.ReadByte();
            room.Filler = reader.ReadByte();
        }

        uint numFloorData = reader.ReadUInt32();
        _level.FloorData = reader.ReadUInt16s(numFloorData).ToList();
    }

    private void WriteRooms(TRLevelWriter writer)
    {
        _spriteBuilder.CacheSpriteOffsets(_level.Sprites);

        writer.Write((ushort)_level.Rooms.Count);
        foreach (TR4Room room in _level.Rooms)
        {
            writer.Write(room.Info, TRGameVersion.TR4);

            _roomBuilder.WriteMesh(writer, room.Mesh);

            writer.Write((ushort)room.Portals.Count);
            writer.Write(room.Portals);

            writer.Write(room.NumZSectors);
            writer.Write(room.NumXSectors);
            writer.Write(room.Sectors);

            writer.Write(room.AmbientIntensity);
            writer.Write(room.LightMode);

            writer.Write((ushort)room.Lights.Count);
            foreach (TR4RoomLight light in room.Lights)
            {
                writer.Write(light.X);
                writer.Write(light.Y);
                writer.Write(light.Z);
                writer.Write(light.Colour);
                writer.Write(light.LightType);
                writer.Write(light.Unknown);
                writer.Write(light.Intensity);
                writer.Write(light.In);
                writer.Write(light.Out);
                writer.Write(light.Length);
                writer.Write(light.CutOff);
                writer.Write(light.Dx);
                writer.Write(light.Dy);
                writer.Write(light.Dz);
            }

            writer.Write((ushort)room.StaticMeshes.Count);
            foreach (TR4RoomStaticMesh mesh in room.StaticMeshes)
            {
                writer.Write(mesh.X);
                writer.Write(mesh.Y);
                writer.Write(mesh.Z);
                writer.Write(mesh.Angle);
                writer.Write(mesh.Colour);
                writer.Write(mesh.Unused);
                writer.Write((ushort)(mesh.ID - TR4Type.SceneryBase));
            }

            writer.Write(room.AlternateRoom);
            writer.Write(room.Flags);
            writer.Write(room.WaterScheme);
            writer.Write(room.ReverbInfo);
            writer.Write(room.Filler);
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
        _meshBuilder.WriteObjectMeshes(writer, _level.Models.Values.SelectMany(m => m.Meshes), _level.StaticMeshes);
    }

    private void ReadModelData(TRLevelReader reader)
    {
        TRModelBuilder<TR4Type> builder = new(TRGameVersion.TR4, _observer);
        _level.Models = builder.ReadModelData(reader, _meshBuilder);
    }

    private void WriteModelData(TRLevelWriter writer)
    {
        TRModelBuilder<TR4Type> builder = new(TRGameVersion.TR4, _observer);
        builder.WriteModelData(writer, _level.Models);
    }

    private void ReadStaticMeshes(TRLevelReader reader)
    {
        _level.StaticMeshes = _meshBuilder.ReadStaticMeshes(reader, TR4Type.SceneryBase);
    }

    private void WriteStaticMeshes(TRLevelWriter writer)
    {
        _meshBuilder.WriteStaticMeshes(writer, _level.StaticMeshes, TR4Type.SceneryBase);
    }

    private void ReadSprites(TRLevelReader reader)
    {
        _level.Sprites = _spriteBuilder.ReadSprites(reader);

        for (int i = 0; i < _level.Rooms.Count; i++)
        {
            _level.Rooms[i].Mesh = _roomBuilder.BuildMesh(i);
        }
    }

    private void WriteSprites(TRLevelWriter writer)
    {
        _spriteBuilder.WriteSprites(writer, _level.Sprites);
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
