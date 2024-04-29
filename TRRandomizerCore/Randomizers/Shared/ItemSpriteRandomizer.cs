using TRLevelControl.Model;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Randomizers;

public class ItemSpriteRandomizer<T>
    where T : Enum
{
    private T _randomSpriteID = default;

    public List<T> StandardItemTypes { get; set; }
    public List<T> KeyItemTypes { get; set; }
    public List<T> SecretItemTypes { get; set; }
    public TRDictionary<T, TRSpriteSequence> Sequences { get; set; }
    public bool RandomizeKeyItemSprites { get; set; }
    public bool RandomizeSecretSprites { get; set; }
    public SpriteRandoMode Mode { get; set; }

    public event EventHandler<SpriteEventArgs<T>> TextureChanged;

    public void Randomize(Random generator)
    {
        List<T> replacementCandidates = new();
        List<T> commonTypesInAllLevels = new();
        Dictionary<T, TRSpriteTexture> spriteTextures = new();

        commonTypesInAllLevels.AddRange(StandardItemTypes);
        replacementCandidates.AddRange(StandardItemTypes);

        if (RandomizeKeyItemSprites)
        {
            replacementCandidates.AddRange(KeyItemTypes);
        }

        if (RandomizeSecretSprites)
        {
            replacementCandidates.AddRange(SecretItemTypes);
            commonTypesInAllLevels.AddRange(SecretItemTypes);
        }

        // Cache the sprite textures before changing anything, otherwise it's possible for items
        // to end up with the same texture even though they are chosen differently from the pool.
        // E.g. on one iteration the grenade launcher may be changed to the small med; on the next,
        // the autos may be changed to the grenade launcher, but that has already changed to the small
        // med - by caching we can repurpose those that have already changed.
        foreach (T entityType in replacementCandidates)
        {
            TRSpriteSequence spriteSequence = Sequences[entityType];
            if (spriteSequence != null)
            {
                spriteTextures[entityType] = spriteSequence.Textures.First();
            }
        }

        // The cache indicates exacty what's in the level, so the keys become the candidate list
        replacementCandidates = spriteTextures.Keys.ToList();

        T replacementSpriteID = default;
        if (Mode == SpriteRandoMode.OneSpritePerGame)
        {
            if (EqualityComparer<T>.Default.Equals(_randomSpriteID, default))
            {
                _randomSpriteID = commonTypesInAllLevels[generator.Next(0, commonTypesInAllLevels.Count)];
            }
            replacementSpriteID = _randomSpriteID;
        }
        else if (Mode == SpriteRandoMode.OneSpritePerLevel)
        {
            replacementSpriteID = replacementCandidates[generator.Next(0, replacementCandidates.Count)];
        }

        foreach (T entityType in replacementCandidates)
        {
            if (Mode == SpriteRandoMode.Default)
            {
                replacementSpriteID = replacementCandidates[generator.Next(0, replacementCandidates.Count)];
            }

            TRSpriteSequence currentSpriteSequence = Sequences[entityType];
            if (spriteTextures.ContainsKey(replacementSpriteID))
            {
                currentSpriteSequence.Textures.Clear();
                currentSpriteSequence.Textures.Add(spriteTextures[replacementSpriteID]);
                TextureChanged?.Invoke(this, new()
                {
                    OldSprite = entityType,
                    NewSprite = replacementSpriteID
                });
            }
        }
    }
}

public class SpriteEventArgs<E>
{
    public E OldSprite { get; set; }
    public E NewSprite { get; set; }
}
