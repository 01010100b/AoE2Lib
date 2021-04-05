using System;
using System.Collections.Generic;
using System.Text;

namespace Unary.Utils
{
    class Mod
    {
        public TimeSpan GetAttackDelay(int id)
        {
            return TimeSpan.FromMilliseconds(500);
        }
        
        public double GetWidth(int id)
        {
            return 1;
        }

        public double GetHeight(int id)
        {
            return 1;
        }

        public int GetHillMode(int id)
        {
            return 3;
        }

        public int GetLOS(int id)
        {
            return 4;
        }

        public void Load()
        {

        }
    }
}
