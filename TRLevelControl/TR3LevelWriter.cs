using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Model;

namespace TRLevelReader
{
    public class TR3LevelWriter
    {
        public TR3LevelWriter()
        {

        }

        public void WriteLevelToFile(TR3Level lvl, string filepath)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Create(filepath)))
            {
                writer.Write(lvl.Serialize());
            }
        }
    }
}
