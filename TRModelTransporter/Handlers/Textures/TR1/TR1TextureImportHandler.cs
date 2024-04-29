using TRLevelControl.Model;
using TRModelTransporter.Helpers;
using TRModelTransporter.Model.Definitions;
using TRModelTransporter.Packing;
using TRTexture16Importer.Helpers;

namespace TRModelTransporter.Handlers.Textures;

public class TR1TextureImportHandler : AbstractTextureImportHandler<TR1Type, TR1Level, TR1ModelDefinition>
{
    public TRPalette8Control PaletteManager { get; set; }

    protected override TRDictionary<TR1Type, TRSpriteSequence> GetExistingSpriteSequences()
    {
        if (_level.Sprites[TR1Type.Explosion1_S_H]?.Textures.Count == 1)
        {
            // Allow replacing the Explosion sequence in Vilcabamba (it's there but empty, originally dynamite?)
            _level.Sprites.Remove(TR1Type.Explosion1_S_H);
        }
        return _level.Sprites;
    }

    protected override AbstractTexturePacker<TR1Type, TR1Level> CreatePacker()
    {
        return new TR1TexturePacker(_level)
        {
            PaletteManager = PaletteManager
        };
    }

    protected override void ProcessRemovals(AbstractTexturePacker<TR1Type, TR1Level> packer)
    {
        List<TR1Type> removals = new();
        if (_clearUnusedSprites)
        {
            removals.Add(TR1Type.Map_M_U);
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

    private void RemoveUnusedSprites(AbstractTexturePacker<TR1Type, TR1Level> packer)
    {
        List<TR1Type> unusedItems = new()
        {
            TR1Type.PistolAmmo_S_P,
            TR1Type.Map_M_U
        };

        ISet<TR1Type> allEntities = new HashSet<TR1Type>();
        for (int i = 0; i < _level.Entities.Count; i++)
        {
            allEntities.Add(_level.Entities[i].TypeID);
        }

        for (int i = unusedItems.Count - 1; i >= 0; i--)
        {
            if (allEntities.Contains(unusedItems[i]))
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

    protected override void RemapMeshTextures(Dictionary<TR1ModelDefinition, Dictionary<int, int>> indexMap)
    {
        foreach (TR1ModelDefinition definition in indexMap.Keys)
        {
            foreach (TRMesh mesh in definition.Meshes)
            {
                foreach (TRMeshFace face in mesh.TexturedFaces)
                {
                    face.Texture = ConvertTextureReference(face.Texture, indexMap[definition]);
                }
            }
        }
    }

    public override void ResetUnusedTextures()
    {
        // Patch - this doesn't break the game, but it prevents the level being
        // opened in trview. Some textures will now be unused, but rather than
        // removing them and having to reindex everything that points to the
        // the object textures, we'll just reset them to atlas 0, and set all
        // coordinates to 0.

        _level.ResetUnusedTextures();
    }

    protected override IEnumerable<TR1Type> CollateWatchedTextures(IEnumerable<TR1Type> watchedEntities, TR1ModelDefinition definition)
    {
        return new List<TR1Type>();
    }
}
