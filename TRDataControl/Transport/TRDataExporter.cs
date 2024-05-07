using System.Diagnostics;
using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TRDataControl;

public abstract class TRDataExporter<L, T, S, B> : TRDataTransport<L, T, S, B>
    where L : TRLevelBase
    where T : Enum
    where S : Enum
    where B : TRBlobBase<T>
{
    public string BaseLevelDirectory { get; set; }

    public B Export(L level, T type, TRBlobType blobType)
    {
        Level = level;

        CreateRemapper(Level)?.Remap();

        PreCreation(level, type, blobType);

        B blob = CreateBlob(level, type, blobType);
        blob.Dependencies = new(Data.GetDependencies(blob.Alias));

        CompileTextures(blob, CreatePacker());

        switch (blobType)
        {
            case TRBlobType.Model:
                BuildModel(blob);
                break;
            case TRBlobType.StaticMesh:
                BuildStaticMesh(blob);
                break;
            case TRBlobType.Sprite:
                BuildSprite(blob);
                break;
        }

        foreach (S sfx in Data.GetHardcodedSounds(blob.Alias))
        {
            StoreSFX(sfx, blob);
        }

        PostCreation(blob);
        StoreBlob(blob);

        return blob;
    }

    protected void CompileTextures(B blob, TRTexturePacker packer)
    {
        // Get the object or sprite textures from the packer
        Dictionary<TRTextile, List<TRTextileRegion>> textures;
        if (blob.Type == TRBlobType.Sprite)
        {
            textures = packer.GetSpriteRegions(SpriteSequences[blob.ID]);
        }
        else
        {
            List<TRMesh> meshes = new();
            if (blob.Type == TRBlobType.Model)
            {
                meshes.AddRange(Models[blob.ID].Meshes);
            }
            else if (blob.Type == TRBlobType.StaticMesh)
            {
                meshes.Add(StaticMeshes[blob.ID].Mesh);
            }

            // We don't ignore the dummy mesh if this is the master type
            TRMesh dummyMesh = IsMasterType(blob.ID) ? null : GetDummyMesh();
            textures = packer.GetMeshRegions(meshes, dummyMesh);

            ExtractMeshColours(meshes, dummyMesh, blob);
        }

        // Deduplicate for efficient import later
        new TRImageDeduplicator().Deduplicate(textures);

        // Classify each texture to allow identical texture matching during import
        blob.Textures = new(textures.SelectMany(r => r.Value));
        foreach (TRTextileRegion region in blob.Textures)
        {
            region.GenerateID();
        }

        Debug.Assert(blob.Textures.Select(t => t.ID).Distinct().Count() == blob.Textures.Count);
    }

    protected void BuildModel(B blob)
    {
        blob.Model = Models[blob.ID];

        if (!IsMasterType(blob.ID))
        {
            TRMesh dummyMesh = GetDummyMesh();
            for (int i = 0; i < blob.Model.Meshes.Count; i++)
            {
                if (blob.Model.Meshes[i] == dummyMesh)
                {
                    blob.Model.Meshes[i] = null;
                }
            }
        }

        HashSet<S> modelSFX = blob.Model.Animations
            .SelectMany(a => a.Commands.Where(c => c is TRSFXCommand))
            .Select(s => (S)(object)(uint)((TRSFXCommand)s).SoundID)
            .ToHashSet();

        foreach (S sfx in modelSFX)
        {
            StoreSFX(sfx, blob);
        }

        if (Data.GetCinematicTypes().Contains(blob.Alias))
        {
            blob.CinematicFrames = CinematicFrames;
        }
    }

    protected void BuildStaticMesh(B blob)
    {
        blob.StaticMesh = StaticMeshes[blob.ID];
    }

    protected void ExtractMeshColours(IEnumerable<TRMesh> meshes, TRMesh dummyMesh, B blob)
    {
        IEnumerable<ushort> faces = meshes
            .Where(m => m != dummyMesh)
            .SelectMany(m => m.ColouredFaces)
            .Select(f => f.Texture)
            .Distinct();

        foreach (ushort texture in faces)
        {
            StoreColour(texture, blob);
        }
    }

    protected void BuildSprite(B blob)
    {
        // Track where sprite textures are to retain sequencing
        blob.SpriteOffsets = new();
        TRSpriteSequence sequence = SpriteSequences[blob.ID];
        foreach (TRSpriteTexture texture in sequence.Textures)
        {
            foreach (TRTextileSegment segment in blob.Textures.SelectMany(r => r.Segments))
            {
                if (segment.Texture == texture)
                {
                    blob.SpriteOffsets.Add(segment.Index);
                    break;
                }
            }
        }
    }

    protected virtual void PreCreation(L level, T type, TRBlobType blobType) { }
    protected abstract B CreateBlob(L level, T type, TRBlobType blobType);
    protected abstract void StoreColour(ushort index, B blob);
    protected abstract void StoreSFX(S sfx, B blob);
    protected virtual void PostCreation(B blob) { }
}
