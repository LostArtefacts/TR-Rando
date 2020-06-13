using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR2Randomizer.Randomizers
{
    public interface IRandomizer
    {
        void Randomize(int seed);
    }
}
