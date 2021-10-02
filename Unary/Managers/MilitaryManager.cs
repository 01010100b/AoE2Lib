using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Managers
{
    internal class MilitaryManager : Manager
    {
        public class ScoutingState
        {
            public readonly Tile Tile;
            public TimeSpan LastAttemptGameTime { get; set; } = TimeSpan.MinValue;
            public TimeSpan LastAccessGameTime { get; set; } = TimeSpan.MinValue;

            public ScoutingState(Tile tile)
            {
                Tile = tile;
            }
        }

        private readonly Dictionary<Tile, ScoutingState> ScoutingStates = new Dictionary<Tile, ScoutingState>();

        public MilitaryManager(Unary unary) : base(unary)
        {

        }

        public IEnumerable<ScoutingState> GetScoutingStatesForLos(int los)
        {
            var radius = 2 * los;
            var map = Unary.GameState.Map;

            for (int x = los; x < map.Width + los; x += radius)
            {
                for (int y = los; y < map.Height + los; y += radius)
                {
                    var _x = Math.Min(x, map.Width - 1);
                    var _y = Math.Min(y, map.Height - 1);
                    var tile = map.GetTile(_x, _y);

                    if (!ScoutingStates.ContainsKey(tile))
                    {
                        ScoutingStates.Add(tile, new ScoutingState(tile));
                    }

                    yield return ScoutingStates[tile];
                }
            }
        }

        internal override void Update()
        {
            
        }
    }
}
