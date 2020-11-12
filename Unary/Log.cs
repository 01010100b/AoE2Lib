using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary
{
    static class Log
    {
        public static int MinimumLevel = 1;

        public static void Info(object message)
        {
            Write(0, message);
        }

        public static void Debug(object message)
        {
            Write(1, message);
        }

        public static void Warning(object message)
        {
            Write(2, message);
        }

        public static void Error(object message)
        {
            Write(3, message);
        }

        private static void Write(int level, object message)
        {
            if (level >= MinimumLevel)
            {
                System.Diagnostics.Debug.WriteLine(message.ToString());
            }
        }
    }
}
