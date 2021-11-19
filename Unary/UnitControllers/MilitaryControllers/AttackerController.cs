using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Unary.Managers.MilitaryManager;

namespace Unary.UnitControllers.MilitaryControllers
{
    class AttackerController : MilitaryController
    {
        public Attack Attack { get; private set; } = null;
        public Unit Target { get; private set; } = null;

        public AttackerController(Unit unit, Unary unary) : base(unit, unary)
        {

        }

        protected override void MilitaryTick()
        {
            if (Unary.StrategyManager.Attacking == null)
            {
                new IdlerController(Unit, Unary);

                return;
            }

            if (Target == null || Target.Targetable == false || GetHashCode() % 17 == Unary.GameState.Tick % 17)
            {
                FindTarget();
            }

            if (Target != null)
            {
                AttackTarget();
                Target.RequestUpdate();
            }
        }

        private void ChooseAttack()
        {
            var attacks = Unary.MilitaryManager.GetAttacks().ToList();

            if (attacks.Count == 0)
            {
                Attack = null;

                return;
            }

            if (!attacks.Contains(Attack))
            {
                Attack = null;
            }

            if (Attack != null)
            {
                return;
            }


        }

        private void FindTarget()
        {
            var position = Unary.GameState.MyPosition;
            var radius = double.MaxValue;

            Unit closest_building = null;
            foreach (var building in Unary.StrategyManager.Attacking.Units.Where(u => u.IsBuilding && u.Targetable))
            {
                if (closest_building == null || building.Position.DistanceTo(Unary.GameState.MyPosition) < closest_building.Position.DistanceTo(Unary.GameState.MyPosition))
                {
                    closest_building = building;
                }
            }

            if (closest_building != null)
            {
                position = closest_building.Position;
                radius = 10;

                if (Unary.StrategyManager.Attacking.IsAlly)
                {
                    radius = 30;
                }
            }
            else
            {
                Unary.Log.Debug($"Attacker {Unit.Id} can not find closest building of player {Unary.StrategyManager.Attacking.PlayerNumber}");

                return;
            }

            var targets = new List<Unit>();
            foreach (var enemy in Unary.GameState.Enemies)
            {
                foreach (var unit in enemy.Units.Where(u => u.Targetable))
                {
                    if (position.DistanceTo(unit.Position) <= radius)
                    {
                        targets.Add(unit);
                    }
                }
            }

            Unit best_target = null;
            foreach (var target in targets)
            {
                if (best_target == null || target.Position.DistanceTo(Unit.Position) < best_target.Position.DistanceTo(Unit.Position))
                {
                    best_target = target;
                }
            }

            if (best_target != null)
            {
                Target = best_target;
                Unary.Log.Debug($"Attacker {Unit.Id} choose target {Target.Id}");
            }
            else
            {
                Target = null;

                if (Unit.Position.DistanceTo(closest_building.Position) > radius)
                {
                    MoveTo(closest_building.Position, radius);
                    Unary.Log.Debug($"Attacker {Unit.Id} moving to closest building");
                }
                else
                {
                    Unary.Log.Debug($"Attacker {Unit.Id} can not find target");
                }
            }
        }

        private void AttackTarget()
        {
            if (Unit.GetTarget() != Target)
            {
                Unit.Target(Target);
            }
        }
    }
}
