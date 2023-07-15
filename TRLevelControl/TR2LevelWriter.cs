using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelControl.Model;

namespace TRLevelControl
{
    public class TR2LevelWriter
    {
        public TR2LevelWriter()
        {

        }

        public void WriteLevelToFile(TR2Level lvl, string filepath)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Create(filepath)))
            {
                writer.Write(lvl.Serialize());
            }
        }
    }
}
