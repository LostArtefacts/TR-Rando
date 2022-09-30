using System;
using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Model;
using TRRandomizerCore.Helpers;

/// <summary>
/// Class used to Randomize the sprites of pickups and maybe keys items and/or secret items according to global settings chosen by the user
/// Today this class is usable by TR1 and TR2 randomizer with E=proper Entity type
/// </summary>
namespace TRRandomizerCore.Randomizers
{
    public class ItemSpriteRandomizer<E> where E : Enum
    {
        /// <summary>
        /// Used only if the sprite randomisation is active and is in OneItemPerGame Mode
        /// </summary>
        private E _randomSpriteID = default;

        /// <summary>
        /// List of pichups and weapons of the game
        /// </summary>
        public List<E> StandardItemTypes { get; set; }

        /// <summary>
        /// List of key items from the game
        /// </summary>
        public List<E> KeyItemTypes { get; set; }

        /// <summary>
        /// List of secret items 
        /// </summary>
        public List<E> SecretItemTypes { get; set; }

        /// <summary>
        /// Sprite Sequence of the current level (should be left untouched it's just to ensure what types are in the level)
        /// </summary>
        public List<TRSpriteSequence> Sequences { get; set; }

        /// <summary>
        /// Sprite Textures of the current level (will be changed) 
        /// </summary>
        public List<TRSpriteTexture> Textures { get; set; }

        /// <summary>
        /// Debug event
        /// </summary>
        public event EventHandler<SpriteEventArgs<E>> TextureChanged;


        /// <summary>
        /// Setting entered by the user in Item Rando Window
        /// It allows the sprite randomization to include key items in the mix
        /// </summary>
        public bool RandomizeKeyItemSprites { get; set; }
        /// <summary>
        /// Setting entered by the user in Item Rando Window 
        /// It allows the sprite randomization to include secrets items in the mix
        /// </summary>
        public bool RandomizeSecretSprites { get; set; }

        /// <summary>
        /// Setting entered by the user in Item Rando Window
        /// Is set the randomization logic
        /// </summary>
        public SpriteRandoMode Mode { get; set; }

        /// <summary>
        /// Apply the randomization of sprites according to settings
        /// </summary>
        /// <param name="generator">Random generator</param>
        public void Randomize(Random generator)
        {
            List<E> replacementCandidates = new List<E>(); // Unique list of item types we wish to target in this level
            List<E> commonTypesInAllLevels = new List<E>();// for 1 item per game
            Dictionary<E, TRSpriteTexture> spriteTextures = new Dictionary<E, TRSpriteTexture>();

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
            foreach (E entityType in replacementCandidates)
            {
                TRSpriteSequence spriteSequence = Sequences.Find(s => s.SpriteID == Convert.ToInt32(entityType));
                if (spriteSequence != null)
                {
                    spriteTextures[entityType] = Textures[spriteSequence.Offset];
                }
            }

            // The cache indicates exacty what's in the level, so the keys become the candidate list
            replacementCandidates = spriteTextures.Keys.ToList();

            E replacementSpriteID = default;
            if (Mode == SpriteRandoMode.OneSpritePerGame)
            {
                if (EqualityComparer<E>.Default.Equals(_randomSpriteID, default))
                {
                    // I choose just once among pickups that should be in everylevel
                    _randomSpriteID = commonTypesInAllLevels[generator.Next(0, commonTypesInAllLevels.Count)];
                }
                replacementSpriteID = _randomSpriteID;
            }
            else if (Mode == SpriteRandoMode.OneSpritePerLevel)
            {
                //I chose 1 random type among the items to switch
                replacementSpriteID = replacementCandidates[generator.Next(0, replacementCandidates.Count)];
            }

            // Carry out the actual replacements
            foreach (E entityType in replacementCandidates)
            {
                if (Mode == SpriteRandoMode.Default)
                {
                    //I Chose the random item to replace with (there is a small chance it gets replaced with itslef... but i think thats ok :D )       
                    replacementSpriteID = replacementCandidates[generator.Next(0, replacementCandidates.Count)];
                }

                // Get this type's current sequence
                TRSpriteSequence currentSpriteSequence = Sequences.Find(s => s.SpriteID == Convert.ToInt32(entityType));
                // Get the replacement texture and replace this type's texture with it if it exists
                if (spriteTextures.ContainsKey(replacementSpriteID))
                {
                    Textures[currentSpriteSequence.Offset] = spriteTextures[replacementSpriteID];
                    TextureChanged?.Invoke(this, new SpriteEventArgs<E>
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
}