using System;
using System.Collections.Generic;
using System.Text;

namespace Unary.Utils
{
    class Mod
    {
        public class ObjectDef
        {
            public int Id { get; set; }
            public int AttackDelay { get; set; }
        }

        public readonly Dictionary<int, ObjectDef> Objects = new Dictionary<int, ObjectDef>();

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

        public double GetHillMode(int id)
        {
            return 3;
        }

        public void Load()
        {
            var arb = new ObjectDef() { Id = 492, AttackDelay = 350 };
            Objects.Add(arb.Id, arb);
        }
    }
}
