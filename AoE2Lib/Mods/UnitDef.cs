using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoE2Lib.Mods
{
    public class UnitDef
    {
        public struct ArmorAmount
        {
            public const int PIERCE_ID = 3;
            public const int MELEE_ID = 4;

            public readonly int ArmorId;
            public readonly int Amount;

            public ArmorAmount(int id, int amount)
            {
                ArmorId = id;
                Amount = amount;
            }
        }

        public int TypeId { get; set; } = -1;
        public UnitClass UnitClass { get; set; }
        public readonly List<ArmorAmount> Attacks = new List<ArmorAmount>();
        public readonly List<ArmorAmount> Armors = new List<ArmorAmount>();
        public bool Melee => Attacks.Count(a => a.ArmorId == ArmorAmount.MELEE_ID) > 0;
        public bool Pierce => Attacks.Count(a => a.ArmorId == ArmorAmount.PIERCE_ID) > 0;
        public UnitDef UpgradedFrom { get; set; } = null;
        public UnitDef UpgradesTo { get; set; } = null;
        public UnitDef BaseUnit
        {
            get
            {
                var current = this;
                while (current.UpgradedFrom != null)
                {
                    current = current.UpgradedFrom;
                }

                return current;
            }
        }
    }
}
