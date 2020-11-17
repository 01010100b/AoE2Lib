using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Utils
{
    public class Log
    {
        public int Level = 0;

        public void Info(object message)
        {
            if (Level >= 0)
            {
                Write(message);
            }
        }

        public void Debug(object message)
        {
            if (Level >= 1)
            {
                Write(message);
            }
        }

        public void Warning(object message)
        {
            if (Level >= 2)
            {
                Write(message);
            }
        }

        public void Error(object message)
        {
            if (Level >= 3)
            {
                Write(message);
            }
        }

        public void Write(object message)
        {
            var str = $"{DateTime.UtcNow}: {message}";
            System.Diagnostics.Trace.WriteLine(str);
        }
    }
}
