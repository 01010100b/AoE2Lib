﻿using System;
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
        public const int DEBUG = 3;
        public const int INFO = 2;
        public const int WARNING = 1;
        public const int ERROR = 0;

        public int Level { get; set; } = 3;

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
            var str = $"{DateTime.Now}: {message}";

            Stream.WriteLine(str);
            Stream.Flush();
            //Trace.WriteLine(str);
        }

        public void Debug(object message)
        {
            if (Level >= 3)
            {
                Write($"DEBUG: {message}");
            }
        }

        public void Info(object message)
        {
            if (Level >= 2)
            {
                Write($"INFO: {message}");
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
