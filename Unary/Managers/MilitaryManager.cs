using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.UnitControllers;
using Unary.UnitControllers.MilitaryControllers;

namespace Unary.Managers
{
    internal class MilitaryManager : Manager
    {
        public class Attack
        {
            public readonly bool Raid;
            public Position Position { get; private set; }
            public double Radius { get; private set; }
            public Position MovePosition { get; private set; }
            public double MoveRadius { get; private set; }
            public int DesiredUnits { get; private set; }
            public int DesiredSiege { get; private set; }

            private readonly Dictionary<Unit, double> TargetValues = new();

            public Attack(Position position, double radius, bool raid = false)
            {
                Position = position;
                Radius = radius;
                Raid = raid;
                MovePosition = Position;
                MoveRadius = Radius;
                DesiredUnits = 1;
                DesiredSiege = 0;
            }

            public IReadOnlyDictionary<Unit, double> GetTargetValues()
            {
                return TargetValues;
            }
        }

        public class ScoutingState
        {
            public readonly Tile Tile;
            public TimeSpan LastAttemptGameTime { get; set; } = TimeSpan.MinValue;

            public ScoutingState(Tile tile)
            {
                Tile = tile;
            }
        }

        private readonly Dictionary<Tile, ScoutingState> ScoutingStates = new();
        private readonly List<Attack> Attacks = new();

        public MilitaryManager(Unary unary) : base(unary)
        {

        }

        public IEnumerable<ScoutingState> GetScoutingStatesForLos(int los)
        {
            var size = 2 * los;
            var map = Unary.GameState.Map;

            for (int x = los; x < map.Width + los; x += size)
            {
                for (int y = los; y < map.Height + los; y += size)
                {
                    if (!map.IsOnMap(x, y))
                    {
                        x = Math.Min(map.Width - 1, x);
                        y = Math.Min(map.Height - 1, y);
                    }

                    var tile = map.GetTile(x, y);

                    if (!ScoutingStates.ContainsKey(tile))
                    {
                        ScoutingStates.Add(tile, new ScoutingState(tile));
                    }

                    yield return ScoutingStates[tile];
                }
            }
        }

        public IEnumerable<Attack> GetAttacks()
        {
            return Attacks;
        }

        internal override void Update()
        {
            Unary.GameState.SetStrategicNumber(StrategicNumber.NUMBER_EXPLORE_GROUPS, 0);
        }
    }
}
