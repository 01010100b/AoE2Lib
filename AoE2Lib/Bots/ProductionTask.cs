using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots
{
    class ProductionTask
    {
        public int Priority { get; set; }
        public bool Blocking { get; set; }
        public int WoodCost { get; set; }
        public int FoodCost { get; set; }
        public int GoldCost { get; set; }
        public int StoneCost { get; set; }
        public int Id { get; set; }
        public int MaxCount { get; set; }
        public int MaxPending { get; set; }
        public bool IsTech { get; set; }
        public bool IsBuilding { get; set; }
        public List<Position> BuildPositions = new List<Position>();

    }
}
