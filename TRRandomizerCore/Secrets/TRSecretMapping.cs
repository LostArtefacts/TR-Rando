using System.Collections.Generic;

namespace TRRandomizerCore.Secrets
{
    public class TRSecretMapping<E> where E : class
    {
        public List<int> RewardEntities { get; set; }
        public List<int> JPRewardEntities { get; set; }
        public List<TRSecretRoom<E>> Rooms { get; set; }
    }
}