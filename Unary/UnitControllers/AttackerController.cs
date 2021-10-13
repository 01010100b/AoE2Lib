using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.UnitControllers
{
    class AttackerController : UnitController
    {
        public AttackerController(Unit unit, Unary unary) : base(unit, unary)
        {

        }

        protected override void Tick()
        {
            if (Target == null || Target.Targetable == false)
            {
                FindTarget();
            }

            if (Target != null)
            {
                Target.RequestUpdate();
                AttackTarget();
            }
        }

        private void FindTarget()
        {
            var targets = new List<Unit>();
            foreach (var enemy in Unary.GameState.GetPlayers().Where(p => p.Stance == PlayerStance.ENEMY))
            {
                foreach (var unit in enemy.Units.Where(u => u.Targetable && u.Visible))
                {
                    targets.Add(unit);
                }
            }

            var attackers = Unary.UnitsManager.GetControllers<AttackerController>();
            var assignments = new Dictionary<Unit, int>();
            
            foreach (var target in targets)
            {
                assignments.Add(target, 0);
            }

            foreach (var attacker in attackers.Where(c => c.Target != null))
            {
                if (assignments.ContainsKey(attacker.Target))
                {
                    assignments[attacker.Target]++;
                }
            }

            targets.Sort((a, b) => assignments[b].CompareTo(assignments[a]));
            if (targets.Count > 0)
            {
                Target = targets[0];
                Unary.Log.Debug($"Targeting {Target.Id}");
            }
            else
            {
                Target = null;
            }
        }

        private void AttackTarget()
        {
            const int ATTACK_MS = 10;
            var max_next_attack = Unit[ObjectData.RELOAD_TIME] - 500;

            Debug.WriteLine($"attacking target {Target.Id} with {Unit.Id}");
            Unit.Target(Target, null, null, null, 0, ATTACK_MS);

            PerformPotentialFieldStep(ATTACK_MS + 1, max_next_attack);
        }
    }
}
