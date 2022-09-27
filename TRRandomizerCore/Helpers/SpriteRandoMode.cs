namespace TRRandomizerCore.Helpers
{
    public enum SpriteRandoMode
    {
        /// <summary>
        /// For items chosen to change, all sprite would be shuffled but X will only look like Y 
        /// </summary>
        Default,
        /// <summary>
        /// For items chosen to change, all items would look the same and change in each level
        /// </summary>
        OneSpritePerLevel,
        /// <summary>
        /// For items chosen to change, all items would look the same for all the game
        /// </summary>
        OneSpritePerGame
    }
}
