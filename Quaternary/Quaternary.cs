using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using AoE2Lib.Bots.Modules;
using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaternary
{
    class Quaternary : Bot
    {
        public override string Name => "Quaternary";

        public override int Id => 27432;

        private readonly Random RNG = new Random(Guid.NewGuid().GetHashCode() ^ DateTime.UtcNow.GetHashCode());

        protected override IEnumerable<Command> Update()
        {
            GetModule<ResearchModule>().Research(22); // loom

            GetModule<UnitsModule>().Train(Mod.Villager); // vill

            var pos = GetModule<InfoModule>().MyPosition;
            var dpos = Vector2.FromPoint(RNG.Next(-10, 10), RNG.Next(-10, 10));
            pos += dpos;

            var positions = GetModule<PlacementModule>().GetPlacementPositions(Mod.House, pos, 1, true, 10).ToList();
            if (positions.Count > 0)
            {
                pos = positions[RNG.Next(positions.Count)];
                GetModule<UnitsModule>().Build(Mod.House, pos.PointX, pos.PointY, int.MaxValue, 2);
            }

            var players = GetModule<PlayersModule>().Players.Count;
            Log.Info($"Number of players: {players}");

            var tiles = GetModule<MapModule>().GetTiles().ToList();
            Log.Info($"Number of tiles: {tiles.Count:N0} of which {tiles.Count(t => t.Explored):N0} explored");

            var seconds = GetModule<InfoModule>().GameSecondsPerTick;
            Log.Info($"Game seconds per tick: {seconds:N2}");

            var units = GetModule<UnitsModule>().Units;
            var speed = 0d;
            foreach (var unit in units.Values)
            {
                if (unit.Velocity.Norm() > speed)
                {
                    speed = unit.Velocity.Norm();
                }
            }
            Log.Info($"Number of units: {units.Count} with highest speed {speed:N2}");

            return Enumerable.Empty<Command>();
        }
    }
}
