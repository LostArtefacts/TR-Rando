using TRLevelControl.Model;
using TRModelTransporter.Helpers;
using TRModelTransporter.Model.Definitions;
using TRModelTransporter.Packing;

namespace TRModelTransporter.Handlers;

public class TR2TextureImportHandler : AbstractTextureImportHandler<TR2Type, TR2Level, TR2ModelDefinition>
{
    protected override List<TRSpriteSequence> GetExistingSpriteSequences()
    {
        return _level.SpriteSequences;
    }

    protected override List<TRSpriteTexture> GetExistingSpriteTextures()
    {
        return _level.SpriteTextures;
    }

    protected override AbstractTexturePacker<TR2Type, TR2Level> CreatePacker()
    {
        return new TR2TexturePacker(_level);
    }

    protected override void ProcessRemovals(AbstractTexturePacker<TR2Type, TR2Level> packer)
    {
        List<TR2Type> removals = new();
        if (_clearUnusedSprites)
        {
            removals.Add(TR2Type.Map_M_U);
        }

        // Marco is in Floaters by default but he isn't used. Removing the textures will break precompiled deduplication
        // so this remains unimplemented for the time being.
        //List<TRModel> models = _level.Models.ToList();
        //if (models.Find(m => m.ID == (uint)TR2Entities.MarcoBartoli) != null && models.Find(m => m.ID == (uint)TR2Entities.DragonBack_H) == null)
        //{
        //    removals.Add(TR2Entities.MarcoBartoli);
        //}

        if (_entitiesToRemove != null)
        {
            removals.AddRange(_entitiesToRemove);
        }
        packer.RemoveModelSegments(removals, _textureRemap);

        ApplyFlamePatch();

        if (_clearUnusedSprites)
        {
            RemoveUnusedSprites(packer);
        }
    }

    private void ApplyFlamePatch()
    {
        // TextureDeduplicator will have removed the extra flame blasts present in DA and Lair (Flamethrower and Dragon).
        // We need to ensure that if these models are present in any level, that the sprite sequences for the blasts point
        // to the same as the grenade blast instead.

        List<TR2Type> flameEnemies = new()
        {
            TR2Type.FlamethrowerGoon, TR2Type.DragonExplosionEmitter_N
        };

        if
        (
            _definitions.ToList().FindIndex(d => flameEnemies.Contains(d.Entity)) != -1 ||
            _level.Models.Keys.Any(flameEnemies.Contains)
        )
        {
            int blastSequence = _level.SpriteSequences.FindIndex(s => s.SpriteID == (int)TR2Type.FireBlast_S_H);
            int grenadeSequence = _level.SpriteSequences.FindIndex(s => s.SpriteID == (int)TR2Type.Explosion_S_H);

            if (grenadeSequence != -1)
            {
                if (blastSequence == -1)
                {
                    TRSpriteSequence grenadeBlast = _level.SpriteSequences[grenadeSequence];
                    _level.SpriteSequences.Add(new TRSpriteSequence
                    {
                        SpriteID = (int)TR2Type.FireBlast_S_H,
                        NegativeLength = grenadeBlast.NegativeLength,
                        Offset = grenadeBlast.Offset
                    });
                }
                else
                {
                    // #275 Rather than just pointing the blast sequence offset to the grenade sequence offset,
                    // retain the original sprite texture objects but just remap where they point in the tiles.
                    for (int i = 0; i < _level.SpriteSequences[grenadeSequence].NegativeLength * -1; i++)
                    {
                        _level.SpriteTextures[_level.SpriteSequences[blastSequence].Offset + i] = _level.SpriteTextures[_level.SpriteSequences[grenadeSequence].Offset + i];
                    }
                }
            }
        }
    }

    private void RemoveUnusedSprites(AbstractTexturePacker<TR2Type, TR2Level> packer)
    {
        List<TR2Type> unusedItems = new()
        {
            TR2Type.PistolAmmo_S_P,
            TR2Type.Map_M_U,
            TR2Type.GrayDisk_S_H
        };

        ISet<TR2Type> allEntities = new HashSet<TR2Type>();
        for (int i = 0; i < _level.Entities.Count; i++)
        {
            allEntities.Add(_level.Entities[i].TypeID);
        }

        for (int i = unusedItems.Count - 1; i >= 0; i--)
        {
            if (unusedItems[i] != TR2Type.GrayDisk_S_H && allEntities.Contains(unusedItems[i]))
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

    protected override void RemapMeshTextures(Dictionary<TR2ModelDefinition, Dictionary<int, int>> indexMap)
    {
        foreach (TR2ModelDefinition definition in indexMap.Keys)
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
        _level.ResetUnusedTextures();
    }

    protected override IEnumerable<TR2Type> CollateWatchedTextures(IEnumerable<TR2Type> watchedEntities, TR2ModelDefinition definition)
    {
        // Ensure the likes of the flamethrower having been imported triggers the fact that
        // the flame sprite sequence has been positioned.
        List<TR2Type> entities = new();
        foreach (TR2Type spriteEntity in definition.SpriteSequences.Keys)
        {
            if (watchedEntities.Contains(spriteEntity))
            {
                entities.Add(spriteEntity);
            }
        }

        return entities;
    }
}
