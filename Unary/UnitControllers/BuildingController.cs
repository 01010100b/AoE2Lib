using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.UnitControllers.BuildingControllers;

namespace Unary.UnitControllers
{
    abstract class BuildingController : UnitController
    {
        public int AssignedBuilders { get; private set; } = 0;
        public int MaxBuilders { get; private set; } = 0;

        public BuildingController(Unit unit, Unary unary) : base(unit, unary)
        {

        }

        protected override sealed void Tick()
        {
            Repair();
            BuildingTick();
        }

        protected abstract void BuildingTick();

        private void Repair()
        {
            MaxBuilders = 0;
            AssignedBuilders = 0;

            if (Unit[ObjectData.STATUS] == 0)
            {
                var type = Unit[ObjectData.BASE_TYPE];

                if (type == Unary.Mod.Farm || type == Unary.Mod.LumberCamp || type == Unary.Mod.MiningCamp)
                {
                    MaxBuilders = 0;

                    if (Unary.GameState.GameTime - Unit.FirstUpdateGameTime > TimeSpan.FromMinutes(1))
                    {
                        MaxBuilders = 1;
                    }
                }
                else
                {
                    MaxBuilders = 1;
                }
            }

            if (MaxBuilders <= 0)
            {
                return;
            }

            var total_max_builders = 5;
            var villagers = Unary.UnitsManager.GetControllers<VillagerController>();
            var total_builders = 0;
            
            foreach (var builder in villagers.Where(x => x.Building != null))
            {
                total_builders++;

                if (builder.Building == this)
                {
                    AssignedBuilders++;
                }
            }

            if (AssignedBuilders >= MaxBuilders || total_builders >= total_max_builders)
            {
                return;
            }

            villagers.Sort((a, b) => a.Unit.Position.DistanceTo(Unit.Position).CompareTo(b.Unit.Position.DistanceTo(Unit.Position)));

            foreach (var vill in villagers)
            {
                if (vill.Building == null)
                {
                    vill.Building = this;

                    AssignedBuilders++;
                    total_builders++;

                    if (AssignedBuilders >= MaxBuilders || total_builders >= total_max_builders)
                    {
                        return;
                    }
                }
            }
        }
    }
}
