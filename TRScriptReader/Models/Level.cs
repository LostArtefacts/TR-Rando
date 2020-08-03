using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRScriptReader.Enums;

namespace TRScriptReader.Models
{
    public class Level
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public List<Op<Sequence>> Sequences { get; set; }
    }
}
