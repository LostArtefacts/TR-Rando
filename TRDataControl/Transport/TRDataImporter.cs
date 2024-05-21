using RectanglePacker.Events;
using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TRDataControl;

public abstract class TRDataImporter<L, T, S, B> : TRDataTransport<L, T, S, B>
    where L : TRLevelBase
    where T : Enum
    where S : Enum
    where B : TRBlobBase<T>
{
    public List<T> TypesToImport { get; set; } = new();
    public List<T> TypesToRemove { get; set; } = new();
    public bool ClearUnusedSprites { get; set; }
    public string TextureRemapPath { get; set; }
    public ITexturePositionMonitor<T> TextureMonitor { get; set; }
    public bool IgnoreGraphics { get; set; }
    public bool ForceCinematicOverwrite { get; set; }

    private List<T> _nonGraphicsDependencies;

    public ImportResult<T> Import()
    {
        ImportResult<T> result = new();
        _nonGraphicsDependencies = new();
        List<T> existingTypes = GetExistingTypes();

        CleanRemovalList();
        CleanAliases();

        List<B> blobs = new();
        foreach (T type in TypesToImport)
        {
            BuildBlobList(blobs, existingTypes, type, false);
        }

        // Check for alias duplication
        ValidateBlobList(existingTypes, blobs);

        if (blobs.Count == 0)
        {
            return result;
        }

        result.ImportedTypes.AddRange(blobs.Where(b => b.Type == TRBlobType.Model).Select(b => b.Alias));

        // Store the current dummy mesh in case we are replacing the master type.
        TRMesh dummyMesh = GetDummyMesh();

        // Remove old types first and tidy up stale textures
        RemoveData(result);

        // Try to pack the textures collectively now that we have cleared some space.
        // This will throw if it fails.
        ImportTextures(blobs);

        // Success - import the remaining data.
        ImportData(blobs, dummyMesh);

        return result;
    }

    private void CleanRemovalList()
    {
        // If an entity is marked to be removed but is also in the list
        // to import, don't remove it in the first place.
        List<T> cleanedEntities = new();
        foreach (T type in TypesToRemove)
        {
            bool entityClean = false;
            if (Data.HasAliases(type))
            {
                // Check if we have another alias in the import list different from any
                // in the current level
                T alias = Data.GetLevelAlias(LevelName, type);
                T importAlias = default;
                foreach (T a in Data.GetAliases(type))
                {
                    if (TypesToImport.Contains(a))
                    {
                        importAlias = a;
                        break;
                    }
                }

                if (!Equals(alias, importAlias))
                {
                    entityClean = true;
                }
            }
            else if (!TypesToImport.Contains(type))
            {
                entityClean = true;
            }

            if (entityClean)
            {
                // There may be null meshes dependent on this removal, so we can only remove it if they're
                // being removed as well.
                IEnumerable<T> exclusions = Data.GetRemovalExclusions(type);
                if (exclusions.Any() && exclusions.All(TypesToRemove.Contains))
                {
                    entityClean = false;
                }
            }

            if (entityClean)
            {
                // And similarly for cyclic dependencies
                IEnumerable<T> cyclics = Data.GetCyclicDependencies(type);
                entityClean = cyclics.All(TypesToRemove.Contains) && !cyclics.Any(TypesToImport.Contains);
            }

            if (entityClean)
            {
                cleanedEntities.Add(type);
            }
        }
        TypesToRemove = cleanedEntities;
    }

    private void CleanAliases()
    {
        List<T> cleanedTypes = new();
        // Do we have any aliases?
        foreach (T importType in TypesToImport)
        {
            if (Data.HasAliases(importType))
            {
                throw new TransportException(string.Format
                (
                    "Cannot import ambiguous entity {0} - choose an alias from [{1}].",
                    importType.ToString(),
                    string.Join(", ", Data.GetAliases(importType))
                ));
            }

            bool typeIsValid = true;
            if (Data.IsAlias(importType))
            {
                T existingType = Data.GetLevelAlias(LevelName, Data.TranslateAlias(importType));
                // This entity is only valid if the alias it's for is not already there
                typeIsValid = !Equals(importType, existingType);
            }

            if (typeIsValid)
            {
                cleanedTypes.Add(importType);
            }
        }

        // #139 Ensure that aliases are added last so to avoid dependency issues
        cleanedTypes.Sort((t1, t2) => Data.TranslateAlias(t1).CompareTo(Data.TranslateAlias(t2)));

        TypesToImport = cleanedTypes;
    }

    private void ValidateBlobList(List<T> modelTypes, List<B> importBlobs)
    {
        Dictionary<T, List<T>> detectedAliases = new();
        foreach (T entity in modelTypes)
        {
            if (Data.IsAlias(entity))
            {
                T masterType = Data.TranslateAlias(entity);
                if (!detectedAliases.ContainsKey(masterType))
                {
                    detectedAliases[masterType] = new();
                }
                detectedAliases[masterType].Add(entity);
            }
        }

        foreach (T masterType in detectedAliases.Keys)
        {
            if (detectedAliases[masterType].Count > 1)
            {
                if (!Data.IsAliasDuplicatePermitted(masterType))
                {
                    throw new TransportException(string.Format
                    (
                        "Only one alias per entity can exist in the same level. [{0}] were found as aliases for {1}.",
                        string.Join(", ", detectedAliases[masterType]),
                        masterType.ToString()
                    ));
                }
                else if (Data.AliasPriority.ContainsKey(masterType))
                {
                    // If we are importing two aliases such as LaraMiscAnim_Unwater and LaraMiscAnim_Xian,
                    // allow the priority list to define exactly what imports. Otherwise while the prioritised
                    // model will be imported, other aspects such as texture import will try to import both.
                    T prioritisedType = Data.AliasPriority[masterType];
                    importBlobs.RemoveAll(d => detectedAliases[masterType].Contains(d.Alias) && !Equals(d.Alias, prioritisedType));
                }
            }
        }

        // If we are importing a master model, ensure it is handled first - this will usually be
        // Lara, so when replacing her we need to define her new model first to get the new dummy
        // mesh for other models.
        B masterBlob = importBlobs.Find(b => IsMasterType(b.ID));
        if (masterBlob != null)
        {
            importBlobs.Remove(masterBlob);
            importBlobs.Insert(0, masterBlob);
        }
    }

    private void BuildBlobList(List<B> standardBlobs, List<T> modelTypes, T nextType, bool isDependency)
    {
        if (modelTypes.Contains(nextType))
        {
            // Are we allowed to replace it?
            if (!Data.IsOverridePermitted(nextType))
            {
                // If the model already in the list is a dependency only, but the new one to add isn't, switch it
                B blob = standardBlobs.Find(m => Equals(m.Alias, nextType));
                if (blob != null && blob.IsDependencyOnly && !isDependency)
                {
                    blob.IsDependencyOnly = false;
                }
                else if (TypesToRemove.Contains(nextType))
                {
                    TypesToRemove = new(TypesToRemove);
                    TypesToRemove.Remove(nextType);
                }

                // Avoid issues with cyclic dependencies by adding separately. The caveat here is
                // cyclic dependencies can't have further sub-dependencies.
                IEnumerable<T> cyclicDependencies = Data.GetCyclicDependencies(nextType);
                foreach (T cyclicDependency in cyclicDependencies)
                {
                    if (!modelTypes.Contains(cyclicDependency) || Data.IsOverridePermitted(cyclicDependency))
                    {
                        modelTypes.Add(cyclicDependency);
                        standardBlobs.Add(LoadBlob(cyclicDependency));
                    }
                }

                return;
            }
        }

        B nextBlob = LoadBlob(nextType);
        nextBlob.IsDependencyOnly = isDependency;
        modelTypes.Add(nextType);

        // Add dependencies first
        foreach (T dependency in nextBlob.Dependencies)
        {
            // If it's a non-graphics dependency, but we are importing another alias
            // for it, or the level already contains the dependency, we don't need it.
            bool nonGraphics = Data.IsNonGraphicsDependency(dependency);
            T aliasFor = Data.TranslateAlias(dependency);
            if (!Equals(aliasFor, dependency) && nonGraphics)
            {
                bool required = true;
                // #139 check entire model list for instances where alias and dependencies cause clashes
                foreach (T type in modelTypes)
                {
                    // If this entity and the dependency are in the same family
                    if (Equals(aliasFor, Data.TranslateAlias(type)))
                    {
                        // Skip it and ensure we don't remove the model later
                        _nonGraphicsDependencies.Add(type);
                        required = false;
                        break;
                    }
                }

                if (!required)
                {
                    continue;
                }
            }

            BuildBlobList(standardBlobs, modelTypes, dependency, nonGraphics);
        }

        standardBlobs.Add(nextBlob);
    }

    protected void RemoveData(ImportResult<T> resultTracker)
    {
        List<int> staleTextures = new();
        foreach (T type in TypesToRemove)
        {
            T id = Data.TranslateAlias(type);
            TRBlobType blobType = Data.GetBlobType(id);
            switch (blobType)
            {
                case TRBlobType.Model:
                    if (Models.ContainsKey(id))
                    {
                        staleTextures.AddRange(Models[type].Meshes
                            .SelectMany(m => m.TexturedFaces.Select(t => (int)t.Texture)));

                        if (!_nonGraphicsDependencies.Contains(id))
                        {
                            Models.Remove(id);
                            resultTracker.RemovedTypes.Add(type);
                        }
                    }
                    break;

                case TRBlobType.StaticMesh:
                    if (StaticMeshes.ContainsKey(id))
                    {
                        staleTextures.AddRange(StaticMeshes[type].Mesh.TexturedFaces.Select(t => (int)t.Texture));
                        StaticMeshes.Remove(id);
                    }
                    break;

                case TRBlobType.Sprite:
                    SpriteSequences.Remove(id);
                    break;
            }
        }

        staleTextures = new(staleTextures.Distinct());
        if (staleTextures.Count > 0)
        {
            TRTextureRemapGroup<T, L> remapGroup = TextureRemapPath == null ? null : GetRemapGroup();
            CreateRemapper(Level)?.RemoveUnusedTextures(staleTextures,
                (tile, bounds) => remapGroup?.CanRemoveRectangle(tile, bounds, TypesToRemove) ?? true);
        }

        TextureMonitor?.OnTexturesRemoved(TypesToRemove);
    }

    protected void ImportTextures(List<B> blobs)
    {
        if (IgnoreGraphics)
        {
            return;
        }

        List<TRTextileRegion> importRegions = new();

        foreach (B blob in blobs)
        {
            if (blob.IsDependencyOnly)
                continue;

            foreach (TRTextileRegion region in blob.Textures)
            {
                // Identical textures may exist between models that were exported separately, so merge them here.
                if (importRegions.Find(r => r.ID == region.ID) is TRTextileRegion otherRegion)
                {
                    region.MoveTo(otherRegion.Bounds.Location);
                    otherRegion.Segments.AddRange(region.Segments);
                    region.Segments.Clear();
                }
                else
                {
                    importRegions.Add(region);
                }
            }
        }

        TRTexturePacker packer = CreatePacker();
        packer.AddRectangles(importRegions);
        PackingResult<TRTextile, TRTextileRegion> packingResult = packer.Pack(true);

        if (packingResult.OrphanCount > 0)
        {
            throw new PackingException($"Failed to pack {packingResult.OrphanCount} rectangles for types [{string.Join(", ", blobs.Select(b => b.Alias))}].");
        }

        // Packing passed, so remap mesh textures to their new object references
        Dictionary<string, Dictionary<int, int>> globalRemap = new();
        Queue<int> freeSlots = new(Level.GetFreeTextureSlots());
        foreach (TRTextileRegion region in importRegions)
        {
            globalRemap[region.ID] = new();
            foreach (TRTextileSegment segment in region.Segments.Where(s => s.Texture is TRObjectTexture))
            {
                if (globalRemap[region.ID].ContainsKey(segment.Index))
                    continue;

                int newIndex;
                if (freeSlots.Count > 0)
                {
                    newIndex = freeSlots.Dequeue();
                    Level.ObjectTextures[newIndex] = segment.Texture as TRObjectTexture;
                }
                else if (Level.ObjectTextures.Count < Data.TextureObjectLimit)
                {
                    newIndex = Level.ObjectTextures.Count;
                    Level.ObjectTextures.Add(segment.Texture as TRObjectTexture);
                }
                else
                {
                    throw new PackingException($"Limit of {Data.TextureObjectLimit} textures reached.");
                }

                globalRemap[region.ID][segment.Index] = newIndex;
            }
        }

        Dictionary<T, List<PositionedTexture>> texturePositions = new();

        foreach (B blob in blobs)
        {
            if (blob.IsDependencyOnly)
                continue;

            Dictionary<int, int> remap = new();
            foreach (TRTextileRegion region in blob.Textures)
            {
                foreach (int oldIndex in globalRemap[region.ID].Keys)
                {
                    remap[oldIndex] = globalRemap[region.ID][oldIndex];
                }
            }

            List<TRMesh> meshes = new();
            if (blob.Model != null)
            {
                meshes.AddRange(blob.Model.Meshes);
            }
            if (blob.StaticMesh != null)
            {
                meshes.Add(blob.StaticMesh.Mesh);
            }

            IEnumerable<TRMeshFace> faces = meshes
                .Where(m => m != null)
                .SelectMany(m => m.TexturedFaces);
            foreach (TRMeshFace face in faces)
            {
                face.Texture = (ushort)remap[face.Texture];
            }

            faces = meshes
                .Where(m => m != null)
                .SelectMany(m => m.ColouredFaces);
            foreach (TRMeshFace face in faces)
            {
                face.Texture = ImportColour(blob, face.Texture);
            }

            texturePositions[blob.Alias] = new();
            foreach (var (oldIndex, newIndex) in remap)
            {
                TRObjectTexture texture = Level.ObjectTextures[newIndex];
                texturePositions[blob.Alias].Add(new()
                {
                    OriginalIndex = oldIndex,
                    TileIndex = texture.Atlas,
                    Position = texture.Position
                });
            }
        }

        TextureMonitor?.OnTexturesPositioned(texturePositions);
    }

    protected void ImportData(List<B> blobs, TRMesh oldDummyMesh)
    {
        foreach (B blob in blobs)
        {
            switch (blob.Type)
            {
                case TRBlobType.Model:
                    ImportModel(blob, oldDummyMesh);
                    break;
                case TRBlobType.StaticMesh:
                    ImportStaticMesh(blob);
                    break;
                case TRBlobType.Sprite:
                    ImportSprite(blob);
                    break;
            }

            ImportSound(blob);
            ImportCinematics(blob);

            BlobImported(blob);
        }
    }

    protected void ImportModel(B blob, TRMesh oldDummyMesh)
    {
        if (blob.Model == null)
            return;

        if (IsMasterType(blob.ID))
        {
            Models[blob.ID] = blob.Model;
            TRMesh newDummyMesh = GetDummyMesh();
            foreach (TRModel model in Models.Values)
            {
                if (model == blob.Model)
                    continue;

                for (int i = 0; i < model.Meshes.Count; i++)
                {
                    if (model.Meshes[i] == oldDummyMesh)
                    {
                        model.Meshes[i] = newDummyMesh;
                    }
                }
            }
        }
        // Is this condition really needed?
        else if (!Models.ContainsKey(blob.ID)
            || (!Data.AliasPriority?.ContainsKey(blob.ID) ?? true)
            || Equals(Data.AliasPriority[blob.ID], blob.Alias))
        {
            Models[blob.ID] = blob.Model;

            // Restore dummy meshes
            TRMesh dummyMesh = GetDummyMesh();
            for (int i = 0; i < blob.Model.Meshes.Count; i++)
            {
                if (blob.Model.Meshes[i] == null || blob.IsDependencyOnly)
                {
                    blob.Model.Meshes[i] = dummyMesh;
                }
            }
        }
    }

    protected void ImportCinematics(B blob)
    {
        if (blob.CinematicFrames == null)
        {
            return;
        }

        if (CinematicFrames.Count == 0 || ForceCinematicOverwrite)
        {
            CinematicFrames.Clear();
            CinematicFrames.AddRange(blob.CinematicFrames);
        }
    }

    protected void ImportStaticMesh(B blob)
    {
        if (blob.StaticMesh != null)
        {
            StaticMeshes[blob.ID] = blob.StaticMesh;
        }
    }

    protected void ImportSprite(B blob)
    {
        List<TRSpriteTexture> textures = new();
        foreach (int spriteOffset in blob.SpriteOffsets)
        {
            foreach (TRTextileSegment segment in blob.Textures.SelectMany(r => r.Segments))
            {
                if (segment.Index == spriteOffset)
                {
                    textures.Add(segment.Texture as TRSpriteTexture);
                    break;
                }
            }
        }

        SpriteSequences[blob.ID] = new()
        {
            Textures = textures,
        };
    }

    protected virtual void BlobImported(B blob) { }
    protected abstract ushort ImportColour(B blob, ushort currentIndex);
    protected abstract void ImportSound(B blob);

    protected abstract List<T> GetExistingTypes();
    protected abstract TRTextureRemapGroup<T, L> GetRemapGroup();
}
