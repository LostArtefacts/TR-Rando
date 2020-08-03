using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRScriptReader.Models
{
    public class Op<T> where T: Enum
    {
        public T Type { get; set; }
        public byte? Arg { get; set; }
    }
}
