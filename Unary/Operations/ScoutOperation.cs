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

        private Region Region { get; set; } = null;
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

            var scout = Units.Single();

            if (Region != null)
            {
                if (Region.ExploredFraction >= MinExploredFraction || Region.LastScouted >= TimeSpan.Zero || Region.LastAccessFailure >= TimeSpan.Zero)
                {
                    Region = null;
                }
            }

            if (Region == null)
            {
                TakeRegion();
            }

            if (scout[ObjectData.CLASS] == (int)UnitClass.Livestock && Focus.DistanceTo(Region.Position) > 25)
            {
                Region = null;
            }

            if (Region == null)
            {
                ClearUnits();

                return;
            }

            var distance = Region.Position.DistanceTo(scout.Position);

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

            if (Distance < 1 || TicksSinceDistanceChange > 2 || TicksSinceLargeDistanceChange > 10)
            {
                var gametime = Manager.Unary.InfoModule.GameTime;

                if (distance < Math.Max(1, MapManager.REGION_SIZE / 4))
                {
                    Region.LastScouted = gametime;
                }
                else
                {
                    Region.LastAccessFailure = gametime;
                }

                foreach (var tile in Region.Tiles)
                {
                    tile.RequestUpdate();
                }

                TakeRegion();

                if (Region == null)
                {
                    return;
                }
            }

            if (scout[ObjectData.MOVE_X] != Region.Position.PointX || scout[ObjectData.MOVE_Y] != Region.Position.PointY)
            {
                scout.TargetPosition(Region.Position, UnitAction.MOVE, UnitFormation.LINE, UnitStance.STAND_GROUND);
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

                    Region = null;
                }
            }
        }

        private void TakeRegion()
        {
            var regions = Manager.Unary.MapManager.Regions.Where(r => r.LastScouted < TimeSpan.Zero && r.LastAccessFailure < TimeSpan.Zero).ToList();
            var ops = new HashSet<Region>();
            foreach (var op in Manager.Operations.OfType<ScoutOperation>().Cast<ScoutOperation>())
            {
                if (op.Region != null)
                {
                    ops.Add(op.Region);
                }
            }

            regions.RemoveAll(r => ops.Contains(r) || r.ExploredFraction >= MinExploredFraction);

            var gametime = Manager.Unary.InfoModule.GameTime;
            var pos = Units.Single().Position;
            var cost = double.MaxValue;
            var best = regions.FirstOrDefault();

            foreach (var region in regions)
            {
                var d1 = region.Position.DistanceTo(Focus);
                var d2 = region.Position.DistanceTo(pos);

                var c = d1 + d2;

                if (best == null || c <= cost)
                {
                    cost = c;
                    best = region;
                }
            }

            if (best != null)
            {
                Region = best;
                Distance = 0;
                TicksSinceDistanceChange = 0;

                foreach (var tile in Region.Tiles)
                {
                    tile.RequestUpdate();
                }

                Manager.Unary.Log.Info($"ScoutingOperation: Go scout region {Region.Position}");
            }
            else
            {
                Region = null;
            }
        }
    }
}
