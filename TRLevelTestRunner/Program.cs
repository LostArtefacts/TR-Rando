using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TRLevelReaderUnitTests;

namespace TRLevelTestRunner
{
    class Program
    {
        private static int _maxMethodName = 0, _pass = 0, _fail = 0;

        static void Main()
        {
            RunTest(new TR2Level_UnitTests());

            Console.WriteLine();
            Console.WriteLine("Pass: {0}", _pass);
            Console.WriteLine("Fail: {0}", _fail);
            Console.Read();
        }

        private static void RunTest(object testInstance)
        {
            WriteHeader(testInstance.GetType().ToString());
            foreach (MethodInfo mi in GetTestMethods(testInstance))
            {
                try
                {
                    WriteMethodName(mi.Name);
                    ((Action)Delegate.CreateDelegate(typeof(Action), testInstance, mi))();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Pass");
                    _pass++;
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Fail");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(e);
                    _fail++;
                }

                Console.ResetColor();
            }
        }

        private static List<MethodInfo> GetTestMethods(object obj)
        {
            _maxMethodName = 0;
            MethodInfo[] methodArr = obj.GetType().GetMethods(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
            List<MethodInfo> methods = new List<MethodInfo>();
            foreach (MethodInfo mi in methodArr)
            {
                if (mi.GetCustomAttribute(typeof(TestMethodAttribute)) != null)
                {
                    methods.Add(mi);
                    _maxMethodName = Math.Max(_maxMethodName, mi.Name.Length);
                }
            }
            return methods;
        }

        private static void WriteHeader(string header)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < header.Length + 4; i++)
            {
                sb.Append("*");
            }

            Console.WriteLine(sb);
            Console.WriteLine("* {0} *", header);
            Console.WriteLine(sb);
        }

        private static void WriteMethodName(string name)
        {
            Console.Write(name);
            for (int i = name.Length; i < _maxMethodName + 3; i++)
            {
                Console.Write(".");
            }
        }
    }
}