using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class FixedFloat<T, U>
    {
        public T Whole { get; set; }

        public U Fraction { get; set; }
    }
}
