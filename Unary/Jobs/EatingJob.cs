using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Behaviours;

namespace Unary.Jobs
{
    internal class EatingJob : ResourceGenerationJob
    {
        public override Resource Resource => Resource.FOOD;
        public override int MaxWorkers => Math.Min(Target != null ? 7 : 0, Unary.EconomyManager.GetDesiredGatherers(Resource.FOOD));
        public override string Name => $"Eating at {Location}";
        public override Position Location => TC.Unit.Position + new Position(-1, 1);
        public readonly Controller TC;
        private Unit Target { get; set; } = null;
        private readonly List<Unit> Targets = new();

        public EatingJob(Unary unary, Controller tc) : base(unary)
        {
            TC = tc;
        }

        public override double GetPay(Controller worker)
        {
            if (!worker.HasBehaviour<EatBehaviour>())
            {
                return -1;
            }
            else if (WorkerCount > MaxWorkers)
            {
                return -1;
            }
            else if (HasWorker(worker))
            {
                return 1;
            }
            else if (Vacancies < 1)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }
        public override void OnRemoved()
        {
        }

        protected override void OnWorkerJoining(Controller worker)
        {
        }

        protected override void OnWorkerLeaving(Controller worker)
        {
            if (worker.TryGetBehaviour<EatBehaviour>(out var behaviour))
            {
                behaviour.Target = null;
            }
        }

        protected override void UpdateResourceGeneration()
        {
            if (ShouldRareTick(7))
            {
                FindTargets();
            }

            Target = null;

            var meats = ObjectPool.Get(() => new List<Unit>(), x => x.Clear());
            var animals = ObjectPool.Get(() => new List<Unit>(), x => x.Clear());
            var livestock = ObjectPool.Get(() => new List<Unit>(), x => x.Clear());

            foreach (var unit in Targets.Where(u => u.Targetable))
            {
                if (unit[ObjectData.HITPOINTS] <= 0)
                {
                    meats.Add(unit);
                }
                else if (unit[ObjectData.CMDID] == (int)CmdId.LIVESTOCK_GAIA)
                {
                    livestock.Add(unit);
                }
                else
                {
                    animals.Add(unit);
                }
            }

            foreach (var animal in animals.Where(x => x[ObjectData.CARRY] > 0))
            {
                if (animal.Position.DistanceTo(Location) <= Unary.Settings.KillAnimalRange)
                {
                    Target = animal;

                    break;
                }
            }

            if (Target == null)
            {
                foreach (var meat in meats.Where(x => x[ObjectData.CARRY] > 0))
                {
                    if (meat.Position.DistanceTo(Location) <= Unary.Settings.EatAnimalRange)
                    {
                        if (Target == null || meat[ObjectData.CARRY] < Target[ObjectData.CARRY])
                        {
                            Target = meat;
                        }
                    }
                }
            }

            if (Target == null)
            {
                foreach (var sheep in livestock.Where(x => x[ObjectData.CARRY] > 0))
                {
                    if (sheep.Position.DistanceTo(Location) <= Unary.Settings.KillSheepRange)
                    {
                        if (Target == null || sheep.Position.DistanceTo(Location) < Target.Position.DistanceTo(Location))
                        {
                            Target = sheep;
                        }
                    }
                }
            }

            foreach (var worker in GetWorkers())
            {
                if (worker.TryGetBehaviour<EatBehaviour>(out var behaviour))
                {
                    behaviour.Target = Target;
                }
            }

            foreach (var target in Targets)
            {
                if (Unary.ShouldRareTick(target, 3))
                {
                    target.RequestUpdate();
                }
            }

            ObjectPool.Add(meats);
            ObjectPool.Add(animals);
            ObjectPool.Add(livestock);
        }

        private void FindTargets()
        {
            Targets.Clear();

            foreach (var unit in Unary.GameState.MyPlayer.Units.Concat(Unary.GameState.Gaia.Units))
            {
                if (unit.Position.DistanceTo(Location) > 20)
                {
                    continue;
                }

                if (unit[ObjectData.CLASS] == (int)UnitClass.Livestock
                    || unit[ObjectData.CLASS] == (int)UnitClass.PreyAnimal)
                {
                    Targets.Add(unit);
                }
                else if (unit[ObjectData.CLASS] == (int)UnitClass.PredatorAnimal && unit[ObjectData.CARRY] > 0)
                {
                    Targets.Add(unit);
                }
            }
        }
    }
}
