using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Behaviours
{
    internal class ScoutingBehaviour : Behaviour
    {
        public Tile Focus { get; set; } = null;
        public double Radius { get; set; } = 0;
        public Tile Target { get; private set; } = null;

        public override int GetPriority() => 100;

        protected override bool Tick(bool perform)
        {
            if (Target != null && Target.Explored)
            {
                Target = null;
            }

            if (ShouldRareTick(31))
            {
                Target = null;
            }

            if (Unary.Settings.ScoutingTilesPerRegion < 0)
            {
                Target = null;

                return false;
            }

            if (perform && Focus != null)
            {
                if (Target == null)
                {
                    FindTarget();
                }

                if (Target != null)
                {
                    Unit.Target(Target.Center);
                    Unit.RequestUpdate();

                    return true;
                }
            }

            Target = null;

            return false;
        }

        private void FindTarget()
        {
            var map = Unary.GameState.Map;
            var range = 4;
            var area = Math.PI * range * range;
            var regions = map.Width * map.Height / area;
            var count = (int)Math.Round(Unary.Settings.ScoutingTilesPerRegion * regions);
            var mgr = Unary.MapManager;
            var rng = Unary.Rng;
            var best = double.MaxValue;

            for (int i = 0; i < count; i++)
            {
                var x = rng.Next(map.Width);
                var y = rng.Next(map.Height);

                if (map.TryGetTile(x, y, out var tile))
                {
                    if (!tile.Explored)
                    {
                        if (mgr.CanReach(tile))
                        {
                            var d1 = tile.Position.DistanceTo(Unit.Position);
                            var d2 = Math.Abs(Radius - tile.Position.DistanceTo(Focus.Position));
                            var cost = d1 + d2;

                            if (Target == null || cost < best)
                            {
                                Target = tile;
                                best = cost;
                            }
                        }
                    }
                }
            }
        }
    }
}
