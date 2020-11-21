using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots
{
    public class MathOp
    {
        public int S_EQUALS { get; private set; }
        public int S_ADD { get; private set; }
        public int G_MUL { get; private set; }
        public int G_DIV { get; private set; }
        public int G_MIN { get; private set; }
        public int G_MAX { get; private set; }
        public int G_MOD { get; private set; }
        public int C_EQUALS { get; private set; }
        public int C_MIN { get; private set; }
        public int C_MAX { get; private set; }

        public MathOp()
        {
            SetAOC();
        }

        public void SetAOC()
        {
            S_EQUALS = 24;
            S_ADD = 25;
            G_MUL = 15;
            G_DIV = 17;
            G_MIN = 19;
            G_MAX = 21;
            G_MOD = 18;
            C_EQUALS = 0;
            C_MIN = 7;
            C_MAX = 8;
        }

        public void SetDE()
        {
            S_EQUALS = 0;
            S_ADD = 1;
            G_MUL = 15;
            G_DIV = 16;
            G_MIN = 17;
            G_MAX = 18;
            G_MOD = 19;
            C_EQUALS = 24;
            C_MIN = 29;
            C_MAX = 30;
        }
    }
}
