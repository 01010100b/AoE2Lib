using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots
{
    public class TypeOp
    {
        public int C { get; private set; }
        public int G { get; private set; }

        public TypeOp()
        {
            SetAOC();
        }

        public void SetAOC()
        {
            C = 6;
            G = 13;
        }

        public void SetDE()
        {
            C = 6;
            G = 2;
        }
    }
}
