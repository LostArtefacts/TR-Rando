﻿using Newtonsoft.Json;
using TRLevelControl.Model;
using TRModelTransporter.Helpers;
using TRModelTransporter.Model.Textures;
using TRModelTransporter.Packing;

namespace TRModelTransporter.Utilities;

public class TR2LevelTextureDeduplicator : AbstractTRLevelTextureDeduplicator<TR2Type, TR2Level>
{
    protected override AbstractTexturePacker<TR2Type, TR2Level> CreatePacker(TR2Level level)
    {
        return new TR2TexturePacker(level);
    }

    protected override AbstractTextureRemapGroup<TR2Type, TR2Level> GetRemapGroup(string path)
    {
        return JsonConvert.DeserializeObject<TR2TextureRemapGroup>(File.ReadAllText(path));
    }

    protected override void ReindexTextures(Dictionary<int, int> indexMap)
    {
        Level.ReindexTextures(indexMap);
        Level.ResetUnusedTextures();
    }
}
