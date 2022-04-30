using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace AoE2Lib
{
    public class Log : IDisposable
    {
        public const int LEVEL_DEBUG = 3;
        public const int LEVEL_INFO = 2;
        public const int LEVEL_WARNING = 1;
        public const int LEVEL_ERROR = 0;

        public int Level { get; set; } = LEVEL_DEBUG;

        private readonly StreamWriter Stream;

        public Log(string file)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }

            Stream = new StreamWriter(file);
        }

        public void Write(object message)
        {
            var str = $"{DateTime.Now:u}: {message}";

            Stream.WriteLine(str);
            Stream.Flush();
        }

        public void Debug(object message)
        {
            if (Level >= LEVEL_DEBUG)
            {
                Write($"DEBUG: {message}");
            }
        }

        public void Info(object message)
        {
            if (Level >= LEVEL_INFO)
            {
                Write($"INFO: {message}");
            }
        }

        public void Warning(object message)
        {
            if (Level >= LEVEL_WARNING)
            {
                Write($"WARNING: {message}");
            }
        }

        public void Error(object message)
        {
            if (Level >= LEVEL_ERROR)
            {
                Write($"ERROR: {message}");
            } 
        }

        public void Exception(Exception e)
        {
            Write($"EXCEPTION: {e.Message}\n{e.StackTrace}");
        }

        public void Dispose()
        {
            ((IDisposable)Stream).Dispose();
        }
    }
}
