using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Behaviours;
using Unary.Squads;

namespace Unary.Managers
{
    // controllers, squads
    internal class UnitsManager : Manager
    {
        //private const int GROUPS = 10;

        public IEnumerable<Controller> Villagers => Controllers.Values.Where(c => c.Unit[ObjectData.CMDID] == (int)CmdId.VILLAGER);

        private readonly Dictionary<Unit, Controller> Controllers = new();
        private readonly List<Squad> Squads = new();

        public UnitsManager(Unary unary) : base(unary)
        {

        }

        public IEnumerable<Controller> GetControllers() => Controllers.Values;

        public IEnumerable<Squad> GetSquads() => Squads;

        public void AddSquad(Squad squad)
        {
            Squads.Add(squad);
        }

        public void RemoveSquad(Squad squad)
        {
            Squads.Remove(squad);
        }

        internal override void Update()
        {
            UpdateControllers();
            UpdateSquads();
        }

        private void UpdateControllers()
        {
            foreach (var unit in Unary.GameState.MyPlayer.Units.Where(u => u.Targetable))
            {
                if (!Controllers.ContainsKey(unit))
                {
                    var controller = new Controller(unit, Unary);
                    Controllers.Add(unit, controller);
                }
            }

            foreach (var controller in Controllers.Values.ToList())
            {
                if (controller.Unit.Targetable)
                {
                    controller.Tick();
                }
                else
                {
                    Controllers.Remove(controller.Unit);
                }
            }
        }

        private void UpdateSquads()
        {
            var dict = new Dictionary<Squad, List<Controller>>();

            foreach (var controller in Controllers.Values)
            {
                if (controller.Squad != null)
                {
                    if (!dict.ContainsKey(controller.Squad))
                    {
                        dict.Add(controller.Squad, new List<Controller>());
                    }

                    dict[controller.Squad].Add(controller);
                }
            }


            Squads.Sort((a, b) => b.Priority.CompareTo(a.Priority));

            foreach (var squad in Squads)
            {
                if (!dict.ContainsKey(squad))
                {
                    dict.Add(squad, new List<Controller>());
                }

                squad.TickInternal(Unary, dict[squad]);
            }
        }
    }
}
