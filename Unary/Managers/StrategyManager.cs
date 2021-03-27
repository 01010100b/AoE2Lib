using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using AoE2Lib.Bots.Modules;
using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unary.Operations;

namespace Unary.Managers
{
    class StrategyManager : Manager
    {
        private BuildOperation BuildingOperation { get; set; } = null;
        private BattleOperation BattleOperation { get; set; } = null;
        private GuardOperation GuardOperation { get; set; } = null;

        public StrategyManager(Unary unary) : base(unary)
        {

        }

        public override void Update()
        {
            const int VILLAGER = 83;
            const int HOUSE = 70;

            var units = Unary.UnitsModule;
            var info = Unary.InfoModule;

            units.AddUnitType(VILLAGER);
            units.AddUnitType(HOUSE);

            Unary.ProductionManager.Train(VILLAGER);

            if (info.PopulationHeadroom > 0 && info.HousingHeadroom < 5 && units.UnitTypes[HOUSE].Pending == 0)
            {
                var town = new HashSet<Tile>();
                foreach (var tile in Unary.MapModule.GetTilesInRange(info.MyPosition, 10))
                {
                    town.Add(tile);
                }

                var places = Unary.MapManager.GetPlacements(HOUSE, town).ToList();
                if (places.Count > 0)
                {
                    places.Sort((a, b) => a.Position.DistanceTo(info.MyPosition).CompareTo(b.Position.DistanceTo(info.MyPosition)));
                    Unary.ProductionManager.Build(HOUSE, places[Unary.Rng.Next(places.Count)].Position, 1000, 1);
                }
                
            }

            DoBuilding();
            DoAttacking();
            DoGuarding();
        }

        private void DoBuilding()
        {
            var ops = Unary.OperationsManager;

            if (BuildingOperation == null)
            {
                BuildingOperation = new BuildOperation(ops);
            }

            var foundations = ops.FreeUnits.Concat(BuildingOperation.Units)
                .Where(u => u.GetData(ObjectData.STATUS) == 0 && u.GetData(ObjectData.CATEGORY) == 80)
                .ToList();

            if (foundations.Count == 0)
            {
                BuildingOperation.ClearUnits();
                
                return;
            }

            Unary.Log.Info($"StrategyManager: Have {foundations.Count} foundations");

            foreach (var foundation in foundations)
            {
                BuildingOperation.AddUnit(foundation);

                Unary.Log.Info($"Foundation type {foundation[ObjectData.TYPE]} pos {foundation.Position}");
            }

            if (BuildingOperation.Units.Count(u => u[ObjectData.CMDID] == (int)CmdId.VILLAGER) == 0)
            {
                var vill = ops.FreeUnits.FirstOrDefault(u => u.GetData(ObjectData.CMDID) == (int)CmdId.VILLAGER);

                if (vill != null)
                {
                    BuildingOperation.AddUnit(vill);
                }
            }

            if (BuildingOperation.Units.Count(u => u[ObjectData.CMDID] == (int)CmdId.VILLAGER) == 0)
            {
                foreach (var unit in ops.Operations.Where(o => !(o is BuildOperation)).SelectMany(o => o.Units).Where(u => u[ObjectData.CMDID] == (int)CmdId.VILLAGER))
                {
                    if (unit[ObjectData.CARRY] == 0)
                    {
                        BuildingOperation.AddUnit(unit);
                        
                        break;
                    }
                }
            }
        }

        private void DoAttacking()
        {
            var ops = Unary.OperationsManager;

            if (BattleOperation == null)
            {
                BattleOperation = new BattleOperation(ops);
            }

            BattleOperation.EnemyPriorities.Clear();

            foreach (var player in Unary.PlayersModule.Players.Values.Where(p => p.Stance == PlayerStance.ENEMY))
            {
                foreach (var unit in player.Units.Where(u => u.Visible && u[ObjectData.HITPOINTS] > 0 && u.GetData(ObjectData.CATEGORY) == 70))
                {
                    BattleOperation.EnemyPriorities.Add(unit, 1);
                }
            }

            Unary.Log.Info($"StrategyManager: {BattleOperation.EnemyPriorities.Count} enemies");

            if (BattleOperation.EnemyPriorities.Count == 0)
            {
                BattleOperation.ClearUnits();

                return;
            }

            foreach (var unit in BattleOperation.Manager.FreeUnits.Where(u => u.GetData(ObjectData.CMDID) == (int)CmdId.MILITARY).ToList())
            {
                BattleOperation.AddUnit(unit);
            }

            foreach (var scouting in BattleOperation.Manager.Operations.OfType<ScoutOperation>())
            {
                foreach (var unit in scouting.Units.ToList())
                {
                    BattleOperation.AddUnit(unit);
                }
            }

            
        }

        private void DoGuarding()
        {
            var ops = Unary.OperationsManager;

            if (GuardOperation == null && Unary.Tick > 10)
            {
                foreach (var unit in ops.FreeUnits.Where(u => u[ObjectData.CMDID] == (int)CmdId.MILITARY))
                {
                    if (unit.Position.DistanceTo(Unary.InfoModule.MyPosition) > 30)
                    {
                        GuardOperation = new GuardOperation(ops);
                        GuardOperation.GuardedUnits.Add(unit);

                        break;
                    }
                }
            }

            if (GuardOperation != null && GuardOperation.Units.Count() < 10)
            {
                foreach (var vill in ops.FreeUnits.Where(u => u[ObjectData.CMDID] == (int)CmdId.VILLAGER))
                {
                    if (vill.Position.DistanceTo(Unary.InfoModule.MyPosition) < 20)
                    {
                        GuardOperation.AddUnit(vill);

                        break;
                    }
                }
            }
        }
    }
}
