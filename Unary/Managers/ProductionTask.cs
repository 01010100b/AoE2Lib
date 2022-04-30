using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Managers
{
    internal class ProductionTask
    {
        public readonly int Priority;
        public readonly bool Blocking;
        public bool IsTech => Technology != null;
        public bool IsBuilding => IsTech == false && UnitType.IsBuilding;
        public int FoodCost => IsTech ? Technology.FoodCost : UnitType.FoodCost;
        public int WoodCost => IsTech ? Technology.WoodCost : UnitType.WoodCost;
        public int GoldCost => IsTech ? Technology.GoldCost : UnitType.GoldCost;
        public int StoneCost => IsTech ? Technology.StoneCost : UnitType.StoneCost;

        private readonly Technology Technology;
        private readonly UnitType UnitType;
        private readonly List<Tile> Tiles;
        private readonly int MaxCount;
        private readonly int MaxPending;

        public ProductionTask(Technology technology, int priority, bool blocking)
        {
            Priority = priority;
            Blocking = blocking;
            Technology = technology;
            UnitType = null;
            MaxCount = int.MaxValue;
            MaxPending = int.MaxValue;
        }

        public ProductionTask(UnitType type, List<Tile> tiles, int max_count, int max_pending, int priority, bool blocking)
        {
            Priority = priority;
            Blocking = blocking;
            Technology = null;
            UnitType = type;
            Tiles = tiles;
            MaxCount = max_count;
            MaxPending = max_pending;
        }

        public void Perform(Unary unary, HashSet<Unit> excluded_trainsites)
        {
            if (IsTech)
            {
                Technology.Research();
            }
            else if (UnitType.IsBuilding)
            {
                var placements = unary.TownManager.GetSortedBuildingPlacements(UnitType, Tiles);
                UnitType.Build(placements.Take(100), MaxCount, MaxPending);
            }
            else
            {
                var site_id = UnitType.TrainSite[ObjectData.BASE_TYPE];
                var sites = unary.GameState.MyPlayer.Units
                    .Where(u => u[ObjectData.BASE_TYPE] == site_id && !excluded_trainsites.Contains(u))
                    .Where(u => u[ObjectData.PROGRESS_TYPE] == 0 || u[ObjectData.PROGRESS_TYPE] == 102)
                    .ToList();

                if (sites.Count > 0)
                {
                    var site = sites[unary.Rng.Next(sites.Count)];
                    site.Train(UnitType, MaxCount, MaxPending);
                } 
            }
        }
    }
}
