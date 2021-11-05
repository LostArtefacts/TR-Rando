using System;
using System.Collections.Generic;

namespace TRRandomizerCore.Secrets
{
    public class TRSecretModelAllocation<E> where E : Enum
    {
        internal List<E> ImportModels { get; set; }
        internal List<E> AvailablePickupModels { get; set; }
        internal List<E> AssignedPickupModels { get; set; }

        internal TRSecretModelAllocation()
        {
            ImportModels = new List<E>();
            AvailablePickupModels = new List<E>();
            AssignedPickupModels = new List<E>();
        }
    }
}