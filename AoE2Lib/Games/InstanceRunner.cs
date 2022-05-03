using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Games
{
    public class InstanceRunner
    {
        private readonly string Exe;
        private readonly string Args;
        private readonly double Speed;

        public InstanceRunner(string exe, string args = null, double speed = 1.7)
        {
            Exe = exe;
            Args = args;
            Speed = speed;
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        private void Run()
        {
            throw new NotImplementedException();
        }

        private void Play()
        {
            throw new NotImplementedException();
        }
    }
}
