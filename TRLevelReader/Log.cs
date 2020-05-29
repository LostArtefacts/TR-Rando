using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader
{
    internal static class Log
    {
        public static void LogC(string text)
        {
            Console.WriteLine(text);
        }

        public static void LogF(string text)
        {
            File.AppendAllText("log.txt", text + "\n");
        }
    }
}
