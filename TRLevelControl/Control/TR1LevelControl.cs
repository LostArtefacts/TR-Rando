using System.Diagnostics;
using TRLevelControl.Build;
using TRLevelControl.Model;

namespace TRLevelControl;

public class TR1LevelControl : TRLevelControlBase<TR1Level>
{
    private TRObjectMeshBuilder<TR1Type> _meshBuilder;
    private TRTextureBuilder _textureBuilder;
    private TRSpriteBuilder<TR1Type> _spriteBuilder;
    private TR1RoomBuilder _roomBuilder;

    public TR1LevelControl(ITRLevelObserver observer = null)
        : base(observer) { }

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

    protected override void Initialise()
    {
        _meshBuilder = new(TRGameVersion.TR1, _observer);
        _textureBuilder = new(TRGameVersion.TR1, _observer);
        _spriteBuilder = new(TRGameVersion.TR1);
        _roomBuilder = new();
    }

    protected override void Read(TRLevelReader reader)
    {
        uint numImages = reader.ReadUInt32();
        _level.Images8 = reader.ReadImage8s(numImages);

        // Unused, always 0 in OG
        _level.Version.LevelNumber = reader.ReadUInt32();

        ReadRooms(reader);

        ReadMeshData(reader);
        ReadModelData(reader);

        ReadStaticMeshes(reader);

        ReadObjectTextures(reader);
        ReadSprites(reader);

        ReadCameras(reader);
        ReadSoundSources(reader);

        ReadBoxes(reader);

        ReadAnimatedTextures(reader);

        ReadEntities(reader);

        _level.LightMap = new(reader.ReadBytes(TRConsts.LightMapSize));
        _level.Palette = reader.ReadColours(TRConsts.PaletteSize);

        ReadCinematicFrames(reader);

        ushort numDemoData = reader.ReadUInt16();
        _level.DemoData = reader.ReadBytes(numDemoData);

        ReadSoundEffects(reader);
    }

    protected override void Write(TRLevelWriter writer)
    {
        writer.Write((uint)_level.Images8.Count);
        writer.Write(_level.Images8);

        writer.Write(_level.Version.LevelNumber);

        WriteRooms(writer);

        WriteMeshData(writer);
        WriteModelData(writer);

        WriteStaticMeshes(writer);

        WriteObjectTextures(writer);
        WriteSprites(writer);

        WriteCameras(writer);
        WriteSoundSources(writer);

        WriteBoxes(writer);

        WriteAnimatedTextures(writer);

        WriteEntities(writer);

        Debug.Assert(_level.LightMap.Count == TRConsts.LightMapSize);
        Debug.Assert(_level.Palette.Count == TRConsts.PaletteSize);
        writer.Write(_level.LightMap.ToArray());
        writer.Write(_level.Palette);

        WriteCinematicFrames(writer);

        writer.Write((ushort)_level.DemoData.Length);
        writer.Write(_level.DemoData);

        WriteSoundEffects(writer);
    }

    private void ReadRooms(TRLevelReader reader)
    {
        _level.Rooms = _roomBuilder.ReadRooms(reader);

        TRFDBuilder builder = new(_level.Version.Game, _observer);
        _level.FloorData = builder.ReadFloorData(reader, _level.Rooms.SelectMany(r => r.Sectors));
    }

    private void WriteRooms(TRLevelWriter writer)
    {
        _spriteBuilder.CacheSpriteOffsets(_level.Sprites);

        List<ushort> floorData = _level.FloorData
            .Flatten(_level.Rooms.SelectMany(r => r.Sectors));
        _roomBuilder.WriteRooms(writer, _level.Rooms, _spriteBuilder);

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

    private void ReadObjectTextures(TRLevelReader reader)
    {
        _level.ObjectTextures = _textureBuilder.ReadObjectTextures(reader);
    }

    private void WriteObjectTextures(TRLevelWriter writer)
    {
        _textureBuilder.Write(writer, _level.ObjectTextures);
    }

    private void ReadSprites(TRLevelReader reader)
    {
        _level.Sprites = _spriteBuilder.ReadSprites(reader);

        for (int i = 0; i < _level.Rooms.Count; i++)
        {
            _roomBuilder.BuildMesh(_level.Rooms[i], i, _spriteBuilder);
        }
    }

    private void WriteSprites(TRLevelWriter writer)
    {
        _spriteBuilder.WriteSprites(writer, _level.Sprites);
    }

    private void ReadCameras(TRLevelReader reader)
    {
        uint numCameras = reader.ReadUInt32();
        _level.Cameras = reader.ReadCameras(numCameras);
    }

    private void WriteCameras(TRLevelWriter writer)
    {
        writer.Write((uint)_level.Cameras.Count);
        writer.Write(_level.Cameras);
    }

    private void ReadSoundSources(TRLevelReader reader)
    {
        uint numSources = reader.ReadUInt32();
        _level.SoundSources = reader.ReadSoundSources<TR1SFX>(numSources);
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

    private void ReadEntities(TRLevelReader reader)
    {
        uint numEntities = reader.ReadUInt32();
        _level.Entities = reader.ReadTR1Entities(numEntities);
    }

    private void WriteEntities(TRLevelWriter writer)
    {
        writer.Write((uint)_level.Entities.Count);
        writer.Write(_level.Entities);
    }

    private void ReadCinematicFrames(TRLevelReader reader)
    {
        ushort numFrames = reader.ReadUInt16();
        _level.CinematicFrames = reader.ReadCinematicFrames(numFrames);
    }

    private void WriteCinematicFrames(TRLevelWriter writer)
    {
        writer.Write((ushort)_level.CinematicFrames.Count);
        writer.Write(_level.CinematicFrames);
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
