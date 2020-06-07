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
        public const bool AllowVerboseLogging = false;

        public static void LogC(string text)
        {
            Console.WriteLine(text);
        }

        public static void LogF(string text)
        {
            File.AppendAllText("log.txt", text + "\n");
        }

        public static void LogV(object obj)
        {
            if (AllowVerboseLogging)
            {
                LogV(obj.ToString());
            }          
        }

        public static void LogV(object[] obj)
        {
            if (AllowVerboseLogging)
            {
                for (int i = 0; i < obj.Count(); i++)
                {
                    LogV(obj[i].ToString());
                }
            }
        }

        public static void LogV(byte[] obj)
        {
            if (AllowVerboseLogging)
            {
                for (int i = 0; i < obj.Count(); i++)
                {
                    LogV(obj[i].ToString());
                }
            }
        }

        public static void LogV(ushort[] obj)
        {
            if (AllowVerboseLogging)
            {
                for (int i = 0; i < obj.Count(); i++)
                {
                    LogV(obj[i].ToString());
                }
            }
        }

        public static void LogV(uint[] obj)
        {
            if (AllowVerboseLogging)
            {
                for (int i = 0; i < obj.Count(); i++)
                {
                    LogV(obj[i].ToString());
                }
            }
        }

        public static void LogV(sbyte[] obj)
        {
            if (AllowVerboseLogging)
            {
                for (int i = 0; i < obj.Count(); i++)
                {
                    LogV(obj[i].ToString());
                }
            }
        }

        public static void LogV(short[] obj)
        {
            if (AllowVerboseLogging)
            {
                for (int i = 0; i < obj.Count(); i++)
                {
                    LogV(obj[i].ToString());
                }
            }
        }

        public static void LogV(int[] obj)
        {
            if (AllowVerboseLogging)
            {
                for (int i = 0; i < obj.Count(); i++)
                {
                    LogV(obj[i].ToString());
                }
            }
        }

        public static void LogV(string text)
        {
            if (AllowVerboseLogging)
            {
                File.AppendAllText("log_v.txt", text + "\n");
            }
        }
    }
}
