using System;
using System.Collections.Generic;
using System.Text;

namespace BzTests
{
    internal static class BzEmitter
    {
        public static void WriteLine(string msg)
        {
            Console.WriteLine(msg);
        }

        public static void WriteLine()
        {
            Console.WriteLine();
        }

        public static void Write(string msg)
        {
            Console.Write(msg);
        }

        public static void WriteResult(string label, string testName, ConsoleColor color)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            BzEmitter.Write(label);
            Console.ForegroundColor = oldColor;
            BzEmitter.Write("  ");
            BzEmitter.WriteLine(testName);
        }
    }
}
