using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Utils
{
    public class Log
    {
        public static readonly Log Static = new Log();

        public int Level = 3;

        public void Info(object message)
        {
            if (Level >= 3)
            {
                Write($"INFO: {message}");
            }
        }

        public void Debug(object message)
        {
            if (Level >= 2)
            {
                Write($"DEBUG: {message}");
            }
        }

        public void Warning(object message)
        {
            if (Level >= 1)
            {
                Write($"WARNING: {message}");
            }
        }

        public void Error(object message)
        {
            if (Level >= 0)
            {
                Write($"ERROR: {message}");
            }
        }

        public void Write(object message)
        {
            System.Diagnostics.Trace.WriteLine($"{DateTime.UtcNow}: {message}");
        }
    }
}
