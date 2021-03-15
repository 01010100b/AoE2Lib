using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace AoE2Lib.Utils
{
    public class Log
    {
        public int Level { get; set; } = 3;

        private string LogFile { get; set; }
        private readonly List<string> Lines = new List<string>();
        private readonly Thread Thread;

        public Log(string file)
        {
            LogFile = file;

            if (File.Exists(LogFile))
            {
                File.Delete(LogFile);
            }

            Thread = new Thread(() => FlushUpdate())
            {
                IsBackground = true
            };
            Thread.Start();
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
                Flush();
            } 
        }

        public void Write(object message)
        {
            var str = $"{DateTime.UtcNow}: {message}";

            Trace.WriteLine(str);
            lock (Lines)
            {
                Lines.Add(str);
            }
            
            if (Lines.Count > 100)
            {
                Flush();
            }
        }

        public void Flush()
        {
            if (Lines.Count == 0)
            {
                return;
            }

            var lines = new List<string>();
            lock (Lines)
            {
                lines.AddRange(Lines);
                Lines.Clear();
            }

            if (lines.Count == 0)
            {
                return;
            }

            lock (LogFile)
            {
                File.AppendAllLines(LogFile, lines);
            }
        }

        private void FlushUpdate()
        {
            var last = DateTime.UtcNow;
            while (true)
            {
                while (DateTime.UtcNow - last < TimeSpan.FromSeconds(10))
                {
                    Thread.Sleep(1000);
                }

                Flush();
                last = DateTime.UtcNow;
            }
        }
    }
}
