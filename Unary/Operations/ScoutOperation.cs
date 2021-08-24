using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using AoE2Lib.Bots.Modules;
using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unary.Managers;
using Unary.Utils;

namespace Unary.Operations
{
    class ScoutOperation : Operation
    {
        public Position Focus { get; set; }
        public double MinExploredFraction { get; set; } = 0.95;

        private Tile Tile { get; set; } = null;
        private double Distance { get; set; } = 0;
        private int TicksSinceDistanceChange { get; set; } = 0;
        private double LargeDistance { get; set; } = 0;
        private int TicksSinceLargeDistanceChange { get; set; } = 0;
        
        public ScoutOperation(OperationsManager manager) : base(manager)
        {

        }

        public override void Update()
        {
            if (Units.Count() == 0)
            {
                return;
            }

            if (Tile == null)
            {
                TakeTile();
            }

            if (Tile != null)
            {
                CheckProgress();
            }
        }

        private void CheckProgress()
        {
            var scout = Units.First();
            var los = Manager.Unary.Mod.GetLOS(scout[ObjectData.UPGRADE_TYPE]);
            var map = Manager.Unary.MapModule;
            foreach (var tile in map.GetTilesInRange(scout.Position, los))
            {
                tile.RequestUpdate();
            }

            var count = 0;
            var explored = 0;
            foreach (var tile in map.GetTilesInRange(Tile.Position, los))
            {
                count++;
                if (tile.Explored)
                {
                    explored++;
                }
            }

            var perc = explored / (double)count;
            if (perc >= MinExploredFraction)
            {
                Tile = null;

                return;
            }

            var distance = Tile.Position.DistanceTo(scout.Position);

            if (distance != Distance)
            {
                TicksSinceDistanceChange = 0;
            }
            else
            {
                TicksSinceDistanceChange++;
            }
            Distance = distance;

            if (Math.Abs(distance - LargeDistance) > 1)
            {
                LargeDistance = distance;
                TicksSinceLargeDistanceChange = 0;
            }
            else
            {
                TicksSinceLargeDistanceChange++;
            }

            if (Distance < 2 || TicksSinceDistanceChange > 2 || TicksSinceLargeDistanceChange > 10)
            {
                var gametime = Manager.Unary.InfoModule.GameTime;
                var state = Manager.Unary.MapManager.GetScoutingState(Tile);

                if (distance < 2)
                {
                    state.LastScoutedGameTime = gametime;
                }
                else
                {
                    state.LastAccessFailureGameTime = gametime;
                }

                Tile = null;
            }
            else
            {
                if (scout[ObjectData.MOVE_X] != Tile.Position.PointX || scout[ObjectData.MOVE_Y] != Tile.Position.PointY)
                {
                    scout.TargetPosition(Tile.Position, UnitAction.MOVE, UnitFormation.LINE, UnitStance.STAND_GROUND);
                }

                if (scout[ObjectData.CMDID] == (int)CmdId.MILITARY || scout[ObjectData.CMDID] == (int)CmdId.VILLAGER)
                {
                    var scouted_sheep = Manager.Unary.UnitsModule.Units.Values
                        .Where(u => u.PlayerNumber == 0 && u[ObjectData.CLASS] == (int)UnitClass.Livestock && u[ObjectData.HITPOINTS] > 0).ToList();

                    if (scouted_sheep.Count > 0)
                    {
                        scouted_sheep.Sort((a, b) => a.Position.DistanceTo(scout.Position).CompareTo(b.Position.DistanceTo(scout.Position)));

                        var sheep = scouted_sheep[0];
                        sheep.RequestUpdate();

                        scout.TargetUnit(sheep, UnitAction.MOVE, UnitFormation.LINE, UnitStance.NO_ATTACK);

                        Tile = null;
                    }
                }
            }
        }


        private void TakeTile()
        {
            var scout = Units.First();
            var size = 2 * Manager.Unary.Mod.GetLOS(scout[ObjectData.UPGRADE_TYPE]);
            var cost = double.MaxValue;
            Tile best = null;

            var map = Manager.Unary.MapManager;
            var mapmod = Manager.Unary.MapModule;

            foreach (var tile in map.GetGrid(size))
            {
                var state = map.GetScoutingState(tile);
                if (state.LastAccessFailureGameTime > TimeSpan.Zero || state.LastScoutedGameTime > TimeSpan.Zero)
                {
                    continue;
                }

                var d1 = tile.Position.DistanceTo(Focus);
                var d2 = tile.Position.DistanceTo(scout.Position);

                var c = d1 + d2;

                if (best == null || c <= cost)
                {
                    var count = 0;
                    var explored = 0;
                    foreach (var t in mapmod.GetTilesInRange(tile.Position, size / 2))
                    {
                        count++;
                        if (t.Explored)
                        {
                            explored++;
                        }
                    }

                    var perc = explored / (double)count;

                    if (perc < MinExploredFraction)
                    {
                        cost = c;
                        best = tile;
                    }
                }
            }

            if (best != null)
            {
                Tile = best;
                Distance = 0;
                TicksSinceDistanceChange = 0;
                LargeDistance = 0;
                TicksSinceLargeDistanceChange = 0;
            }
        }
    }
}
