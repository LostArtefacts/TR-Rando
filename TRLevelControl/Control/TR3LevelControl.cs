using System.Diagnostics;
using TRLevelControl.Build;
using TRLevelControl.Model;

namespace TRLevelControl;

public class TR3LevelControl : TRLevelControlBase<TR3Level>
{
    private TRObjectMeshBuilder<TR3Type> _meshBuilder;
    private TRTextureBuilder _textureBuilder;
    private TRSpriteBuilder<TR3Type> _spriteBuilder;
    private TR3RoomBuilder _roomBuilder;

    public TR3LevelControl(ITRLevelObserver observer = null)
        : base(observer) { }

    protected override TR3Level CreateLevel(TRFileVersion version)
    {
        TR3Level level = new()
        {
            Version = new()
            {
                Game = TRGameVersion.TR3,
                File = version
            }
        };

        TestVersion(level, TRFileVersion.TR3a, TRFileVersion.TR3b);
        return level;
    }

    protected override void Initialise()
    {
        _meshBuilder = new(TRGameVersion.TR3, _observer);
        _textureBuilder = new(TRGameVersion.TR3, _observer);
        _spriteBuilder = new(TRGameVersion.TR3);
        _roomBuilder = new();
    }

    protected override void Read(TRLevelReader reader)
    {
        ReadPalette(reader);
        ReadImages(reader);

        _level.Version.LevelNumber = reader.ReadUInt32();

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

        ReadLightMap(reader);

        ReadCinematicFrames(reader);

        ReadDemoData(reader);

        ReadSoundEffects(reader);
    }

    protected override void Write(TRLevelWriter writer)
    {
        WritePalette(writer);
        WriteImages(writer);

        writer.Write(_level.Version.LevelNumber);

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

        WriteLightMap(writer);

        WriteCinematicFrames(writer);

        WriteDemoData(writer);

        WriteSoundEffects(writer);
    }

    private void ReadPalette(TRLevelReader reader)
    {
        _level.Palette = reader.ReadColours(TRConsts.PaletteSize, TRConsts.Palette8Multiplier);
        _level.Palette16 = reader.ReadColour4s(TRConsts.PaletteSize);
    }

    private void WritePalette(TRLevelWriter writer)
    {
        Debug.Assert(_level.Palette.Count == TRConsts.PaletteSize);
        Debug.Assert(_level.Palette16.Count == TRConsts.PaletteSize);
        writer.Write(_level.Palette, TRConsts.Palette8Multiplier);
        writer.Write(_level.Palette16);
    }

    private void ReadImages(TRLevelReader reader)
    {
        uint numImages = reader.ReadUInt32();
        _level.Images8 = reader.ReadImage8s(numImages);
        _level.Images16 = reader.ReadImage16s(numImages);
    }

    private void WriteImages(TRLevelWriter writer)
    {
        Debug.Assert(_level.Images8.Count == _level.Images16.Count);
        writer.Write((uint)_level.Images8.Count);
        writer.Write(_level.Images8);
        writer.Write(_level.Images16);
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
        TRModelBuilder<TR3Type> builder = new(TRGameVersion.TR3, _observer);
        _level.Models = builder.ReadModelData(reader, _meshBuilder);
    }

    private void WriteModelData(TRLevelWriter writer)
    {
        TRModelBuilder<TR3Type> builder = new(TRGameVersion.TR3, _observer);
        builder.WriteModelData(writer, _level.Models);
    }

    private void ReadStaticMeshes(TRLevelReader reader)
    {
        _level.StaticMeshes = _meshBuilder.ReadStaticMeshes(reader, TR3Type.SceneryBase);
    }

    private void WriteStaticMeshes(TRLevelWriter writer)
    {
        _meshBuilder.WriteStaticMeshes(writer, _level.StaticMeshes, TR3Type.SceneryBase);
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
        _level.SoundSources = reader.ReadSoundSources<TR3SFX>(numSources);
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
        _level.ObjectTextures = _textureBuilder.ReadObjectTextures(reader);
    }

    private void WriteObjectTextures(TRLevelWriter writer)
    {
        _textureBuilder.Write(writer, _level.ObjectTextures);
    }

    private void ReadEntities(TRLevelReader reader)
    {
        uint numEntities = reader.ReadUInt32();
        _level.Entities = reader.ReadTR3Entities(numEntities);
    }

    private void WriteEntities(TRLevelWriter writer)
    {
        writer.Write((uint)_level.Entities.Count);
        writer.Write(_level.Entities);
    }

    private void ReadLightMap(TRLevelReader reader)
    {
        _level.LightMap = new(reader.ReadBytes(TRConsts.LightMapSize));
    }

    private void WriteLightMap(TRLevelWriter writer)
    {
        Debug.Assert(_level.LightMap.Count == TRConsts.LightMapSize);
        writer.Write(_level.LightMap);
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

    private void ReadDemoData(TRLevelReader reader)
    {
        TRDemoBuilder<TR3DemoGun, TR3InputState> builder = new(TRGameVersion.TR3);
        _level.DemoData = builder.Read(reader);
    }

    private void WriteDemoData(TRLevelWriter writer)
    {
        TRDemoBuilder<TR3DemoGun, TR3InputState> builder = new(TRGameVersion.TR3);
        builder.Write(_level.DemoData, writer);
    }

    private void ReadSoundEffects(TRLevelReader reader)
    {
        _level.SoundEffects = new();
        short[] soundMap = reader.ReadInt16s(Enum.GetValues<TR3SFX>().Length);

        uint numSoundDetails = reader.ReadUInt32();
        List<TR3SoundEffect> sfx = new();

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

        uint numSampleIndices = reader.ReadUInt32();
        uint[] sampleIndices = reader.ReadUInt32s(numSampleIndices);

        foreach (int soundID in sampleMap.Keys)
        {
            TR3SoundEffect effect = sfx[soundID];
            ushort baseIndex = sampleMap[soundID];
            for (int i = 0; i < effect.Samples.Capacity; i++)
            {
                effect.Samples.Add(sampleIndices[baseIndex + i]);
            }
        }

        for (int i = 0; i < soundMap.Length; i++)
        {
            if (soundMap[i] < 0 || soundMap[i] >= sfx.Count)
            {
                continue;
            }

            _level.SoundEffects[(TR3SFX)i] = sfx[soundMap[i]];
        }
    }

    private void WriteSoundEffects(TRLevelWriter writer)
    {
        short detailsIndex = 0;
        foreach (TR3SFX id in Enum.GetValues<TR3SFX>())
        {
            writer.Write(_level.SoundEffects.ContainsKey(id) ? detailsIndex++ : (short)-1);
        }

        List<uint> samplePointers = new();
        foreach (var (_, effect) in _level.SoundEffects)
        {
            if (!samplePointers.Contains(effect.Samples.First()))
            {
                samplePointers.AddRange(effect.Samples);
            }
        }
        samplePointers.Sort();

        writer.Write((uint)_level.SoundEffects.Count);
        foreach (var (_, effect) in _level.SoundEffects)
        {
            uint firstSample = effect.Samples.First();
            writer.Write((ushort)samplePointers.IndexOf(firstSample));
            writer.Write(effect.Volume);
            writer.Write(effect.Range);
            writer.Write(effect.Chance);
            writer.Write(effect.Pitch);
            writer.Write(effect.GetFlags());
        }

        writer.Write((uint)samplePointers.Count);
        writer.Write(samplePointers);
    }
}
