using TRLevelControl.Model;
using TRModelTransporter.Helpers;
using TRModelTransporter.Model.Definitions;
using TRModelTransporter.Packing;

namespace TRModelTransporter.Handlers;

public class TR3TextureImportHandler : AbstractTextureImportHandler<TR3Type, TR3Level, TR3ModelDefinition>
{
    protected override IEnumerable<TRSpriteSequence> GetExistingSpriteSequences()
    {
        return _level.SpriteSequences;
    }

    protected override void WriteSpriteSequences(IEnumerable<TRSpriteSequence> spriteSequences)
    {
        _level.SpriteSequences = spriteSequences.ToArray();
        _level.NumSpriteSequences = (uint)_level.SpriteSequences.Length;
    }

    protected override IEnumerable<TRSpriteTexture> GetExistingSpriteTextures()
    {
        return _level.SpriteTextures.ToList();
    }

    protected override void WriteSpriteTextures(IEnumerable<TRSpriteTexture> spriteTextures)
    {
        _level.SpriteTextures = spriteTextures.ToArray();
        _level.NumSpriteTextures = (uint)_level.SpriteTextures.Length;
    }

    protected override AbstractTexturePacker<TR3Type, TR3Level> CreatePacker()
    {
        return new TR3TexturePacker(_level);
    }

    protected override void ProcessRemovals(AbstractTexturePacker<TR3Type, TR3Level> packer)
    {
        List<TR3Type> removals = new();
        if (_clearUnusedSprites)
        {
            removals.Add(TR3Type.Map_H);
        }

        if (_entitiesToRemove != null)
        {
            removals.AddRange(_entitiesToRemove);
        }
        packer.RemoveModelSegments(removals, _textureRemap);

        if (_clearUnusedSprites)
        {
            RemoveUnusedSprites(packer);
        }
    }

    private void RemoveUnusedSprites(AbstractTexturePacker<TR3Type, TR3Level> packer)
    {
        List<TR3Type> unusedItems = new()
        {
            TR3Type.PistolAmmo_M_H,
            TR3Type.Map_H,
            TR3Type.Disc_H
        };

        ISet<TR3Type> allEntities = new HashSet<TR3Type>();
        for (int i = 0; i < _level.Entities.Count; i++)
        {
            allEntities.Add(_level.Entities[i].TypeID);
        }

        for (int i = unusedItems.Count - 1; i >= 0; i--)
        {
            if (unusedItems[i] != TR3Type.Disc_H && allEntities.Contains(unusedItems[i]))
            {
                unusedItems.RemoveAt(i);
            }
        }

        packer.RemoveSpriteSegments(unusedItems);
    }

    protected override List<TRObjectTexture> GetExistingObjectTextures()
    {
        return _level.ObjectTextures;
    }

    protected override IEnumerable<int> GetInvalidObjectTextureIndices()
    {
        return _level.GetInvalidObjectTextureIndices();
    }

    protected override void RemapMeshTextures(Dictionary<TR3ModelDefinition, Dictionary<int, int>> indexMap)
    {
        foreach (TR3ModelDefinition definition in indexMap.Keys)
        {
            foreach (TRMesh mesh in definition.Meshes)
            {
                foreach (TRFace4 rect in mesh.TexturedRectangles)
                {
                    rect.Texture = ConvertTextureReference(rect.Texture, indexMap[definition]);
                }
                foreach (TRFace3 tri in mesh.TexturedTriangles)
                {
                    tri.Texture = ConvertTextureReference(tri.Texture, indexMap[definition]);
                }
            }
        }
    }

    public override void ResetUnusedTextures()
    {
        _level.ResetUnusedTextures();
    }

    protected override IEnumerable<TR3Type> CollateWatchedTextures(IEnumerable<TR3Type> watchedEntities, TR3ModelDefinition definition)
    {
        return new List<TR3Type>();
    }
}
