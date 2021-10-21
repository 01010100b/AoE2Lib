using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.UnitControllers
{
    class BuilderController : UnitController
    {
        public Unit Foundation { get; private set; }

        public BuilderController(Unit unit, Unary unary) : base(unit, unary)
        {

        }

        protected override void Tick()
        {
            if (Foundation == null)
            {
                ChooseFoundation();
            }
            else
            {
                BuildFoundation();
            }
        }

        private void ChooseFoundation()
        {
            var foundations = Unary.BuildingManager.GetFoundations().ToList();
            var assigned = new Dictionary<Unit, int>();
            
            foreach (var foundation in foundations)
            {
                assigned.Add(foundation, 0);
            }

            foreach (var builder in Unary.UnitsManager.GetControllers<BuilderController>())
            {
                if (builder.Foundation != null && assigned.ContainsKey(builder.Foundation))
                {
                    assigned[builder.Foundation]++;
                }
            }

            foundations.RemoveAll(f => assigned[f] >= 1);
            foundations.Sort((a, b) => a.Position.DistanceTo(Unit.Position).CompareTo(b.Position.DistanceTo(Unit.Position)));

            if (foundations.Count > 0)
            {
                Foundation = foundations[0];
                Unary.Log.Info($"Builder {Unit.Id} building foundation {Foundation.Id}");
            }
            else
            {
                Unary.Log.Info($"Builder {Unit.Id} no foundation found");
                var ctrl = new IdlerController(Unit, Unary);
                Unary.UnitsManager.SetController(Unit, ctrl);
            }
        }

        private void BuildFoundation()
        {
            if (Foundation[ObjectData.STATUS] != 0 && Foundation[ObjectData.HITPOINTS] == Foundation[ObjectData.MAXHP])
            {
                Foundation = null;

                return;
            }

            if (Unit[ObjectData.TARGET_ID] != Foundation.Id)
            {
                Unit.Target(Foundation);
            }

            Foundation.RequestUpdate();
        }
    }
}
