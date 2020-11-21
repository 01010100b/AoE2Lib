using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace AoE2Lib.Utils
{
    public class Log
    {
        public static readonly Log Static = new Log();

        public string LogFile { get; set; } = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "aoe2lib.log");
        public int Level = 2;

        public Log()
        {
            if (File.Exists(LogFile))
            {
                File.Delete(LogFile);
            }
        }

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
            var str = $"{DateTime.UtcNow}: {message}";

            Trace.WriteLine(str);
            lock (LogFile)
            {
                File.AppendAllText(LogFile, str + "\n");
            }
        }
    }
}
