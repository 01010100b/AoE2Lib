using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using AoE2Lib.Bots.Modules;
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

        protected override IEnumerable<Command> RequestUpdate()
        {
            return Enumerable.Empty<Command>();
        }

        protected override void Update()
        {
            var players = GetModule<PlayersModule>().Players.Count;
            Log($"Number of players: {players}");

            var tiles = GetModule<MapModule>().GetTiles().ToList();
            Log($"Number of tiles: {tiles.Count:N0} of which {tiles.Count(t => t.Explored):N0} explored");

            var seconds = GetModule<InfoModule>().GameSecondsPerTick;
            Log($"Game seconds per tick: {seconds:N2}");

            var units = GetModule<UnitsModule>().Units.Count;
            Log($"Number of units: {units}");
        }
    }
}
