using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots
{
    public class MathOp
    {
        public int C_EQUALS { get; private set; }
        public int C_ADD { get; private set; }
        public int C_SUB { get; private set; }
        public int C_MUL { get; private set; }
        public int C_ZDIV { get; private set; }
        public int C_DIV { get; private set; }
        public int C_MOD { get; private set; }
        public int C_MIN { get; private set; }
        public int C_MAX { get; private set; }
        public int C_NEG { get; private set; }
        public int C_PERC_DIV { get; private set; }
        public int C_PERC_MUL { get; private set; }
        public int G_EQUALS { get; private set; }
        public int G_ADD { get; private set; }
        public int G_SUB { get; private set; }
        public int G_MUL { get; private set; }
        public int G_ZDIV { get; private set; }
        public int G_DIV { get; private set; }
        public int G_MOD { get; private set; }
        public int G_MIN { get; private set; }
        public int G_MAX { get; private set; }
        public int G_NEG { get; private set; }
        public int G_PERC_DIV { get; private set; }
        public int G_PERC_MUL { get; private set; }
        public int S_EQUALS { get; private set; }
        public int S_ADD { get; private set; }
        public int S_SUB { get; private set; }
        public int S_MUL { get; private set; }
        public int S_ZDIV { get; private set; }
        public int S_DIV { get; private set; }
        public int S_MOD { get; private set; }
        public int S_MIN { get; private set; }
        public int S_MAX { get; private set; }
        public int S_NEG { get; private set; }
        public int S_PERC_DIV { get; private set; }
        public int S_PERC_MUL { get; private set; }

        public MathOp()
        {
            SetAOC();
        }

        public void SetAOC()
        {
            // TODO add math ops
            C_MAX = 8;
            G_MOD = 18;
            
        }

        public void SetDE()
        {
            // TODO add math ops
            C_MAX = 30;
            G_MOD = 19;
            
        }
    }
}
