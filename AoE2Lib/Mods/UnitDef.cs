using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Mods
{
    public class UnitDef
    {
        public struct ArmorAmount
        {
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
