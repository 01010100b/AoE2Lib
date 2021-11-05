using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.UnitControllers
{
    class FarmerController : UnitController
    {
        public Tile Tile { get; private set; } = null;

        private int LastFarmTick { get; set; } = 0;

        public FarmerController(Unit unit, Unary unary) : base(unit, unary)
        {

        }

        protected override void Tick()
        {
            if (Tile == null)
            {
                ChooseTile();
            }
            else
            {
                FarmTile();
            }
        }

        private void ChooseTile()
        {
            var tiles = new HashSet<Tile>();
            foreach (var farm in Unary.GameState.MyPlayer.Units.Where(u => u.Targetable && u[ObjectData.BASE_TYPE] == Unary.Mod.Farm))
            {
                tiles.Add(farm.Tile);
            }

            foreach (var farmer in Unary.UnitsManager.GetControllers<FarmerController>())
            {
                if (farmer.Tile != null)
                {
                    tiles.Remove(farmer.Tile);
                }
            }

            if (tiles.Count > 0)
            {
                Tile = tiles.First();
                Unary.Log.Debug($"Farmer {Unit.Id} choose tile {Tile.Position}");
            }
            else
            {
                Unary.ProductionManager.RequestFarm(this);
                Unary.Log.Debug($"Farmer {Unit.Id} can not find tile");
            }
        }

        private void FarmTile()
        {
            var farm = Tile.Units.FirstOrDefault(u => u.Targetable && u[ObjectData.BASE_TYPE] == Unary.Mod.Farm);
            if (farm == null)
            {
                if (Unary.GameState.Tick - LastFarmTick > 10)
                {
                    Tile = null;
                }

                Unary.ProductionManager.RequestFarm(this);

                return;
            }

            if (Unit.GetTarget() != farm)
            {
                Unit.Target(farm);
            }

            farm.RequestUpdate();
            LastFarmTick = Unary.GameState.Tick;
        }
    }
}
