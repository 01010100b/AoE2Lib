using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Utils
{
    public static class Log
    {
        public static int Level = 0;

        public static void Info(object message)
        {
            if (Level >= 0)
            {
                Write(message);
            }
        }

        public static void Debug(object message)
        {
            if (Level >= 1)
            {
                Write(message);
            }
        }

        public static void Warning(object message)
        {
            if (Level >= 2)
            {
                Write(message);
            }
        }

        public static void Error(object message)
        {
            if (Level >= 3)
            {
                Write(message);
            }
        }

        public static void Write(object message)
        {
            var str = $"{DateTime.UtcNow}: {message}";
            System.Diagnostics.Trace.WriteLine(str);
        }
    }
}
