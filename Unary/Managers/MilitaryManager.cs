using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.UnitControllers;

namespace Unary.Managers
{
    internal class MilitaryManager : Manager
    {
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
            const bool SCENARIO = false;

            Unary.GameState.SetStrategicNumber(StrategicNumber.NUMBER_EXPLORE_GROUPS, 0);

            if (SCENARIO)
            {
                foreach (var soldier in Unary.UnitsManager.GetControllers<IdlerController>().Where(c => c.Unit[ObjectData.CMDID] == (int)CmdId.MILITARY))
                {
                    var ctrl = new AttackerController(soldier.Unit, Unary);
                    Unary.UnitsManager.SetController(soldier.Unit, ctrl);
                }
            }
            else
            {
                DoScouting();
            }
            
        }

        private void DoScouting()
        {
            var scouts = Unary.UnitsManager.GetControllers<ScoutController>();
            if (scouts.Count == 0)
            {
                Unit scout = null;
                var idlers = Unary.UnitsManager.GetControllers<IdlerController>();
                foreach (var idler in idlers)
                {
                    if (idler.Unit[ObjectData.CMDID] == (int)CmdId.MILITARY)
                    {
                        if (scout == null || idler.Unit[ObjectData.SPEED] > scout[ObjectData.SPEED])
                        {
                            scout = idler.Unit;
                        }
                    }
                }

                if (scout != null)
                {
                    var ctrl = new ScoutController(scout, Unary);
                    ctrl.AttractorPosition = Unary.GameState.MyPosition;
                    Unary.UnitsManager.SetController(scout, ctrl);
                    scouts.Add(ctrl);
                }
            }
        }
    }
}
