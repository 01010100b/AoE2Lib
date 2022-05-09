using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Behaviours
{
    internal class CombatRangedBehaviour : CombatBehaviour
    {

        protected override Unit ChooseTarget(out Unit backup)
        {
            var settings = Controller.Unary.Settings;
            var pos = Controller.Unit.Position;
            var targets = ObjectPool.Get(() => new List<Unit>(), x => x.Clear());
            targets.AddRange(Controller.Unary.SitRepManager.Targets);

            if (targets.Count > 0)
            {
                var attackers = ObjectPool.Get(() => new Dictionary<Unit, int>(), x => x.Clear());
                var scores = ObjectPool.Get(() => new Dictionary<Unit, double>(), x => x.Clear());

                foreach (var attacker in Controller.Manager.Combatants.Where(a => a != Controller))
                {
                    if (attacker.TryGetBehaviour<CombatBehaviour>(out var behaviour))
                    {
                        if (behaviour.Target != null && behaviour.Target.Targetable)
                        {
                            if (!attackers.ContainsKey(behaviour.Target))
                            {
                                attackers.Add(behaviour.Target, 0);
                            }

                            attackers[behaviour.Target]++;
                        }
                    }
                }

                foreach (var t in targets)
                {
                    if (!attackers.ContainsKey(t))
                    {
                        attackers.Add(t, 0);
                    }

                    scores.Add(t, GetTargetScore(t, attackers[t]));
                }

                targets.Sort((a, b) => scores[b].CompareTo(scores[a]));

                var target = targets[0];
                backup = targets[0];

                if (targets.Count > 1)
                {
                    backup = targets[1];
                }

                ObjectPool.Return(attackers);
                ObjectPool.Return(scores);
                ObjectPool.Return(targets);

                return target;
            }
            else
            {
                backup = null;
                ObjectPool.Return(targets);

                return null;
            }
        }

        protected override Position PerformCombat(out bool attack)
        {
            attack = Controller.Unary.Rng.NextDouble() < Controller.Unary.Settings.CombatShootChance;

            var pos = Controller.Unit.Position;
            var delta = GetThreatAvoidanceDelta().Normalize();

            return pos + (2 * delta);
        }

        private double GetTargetScore(Unit target, int attackers)
        {
            var score = 1d / Controller.Unit.Position.DistanceTo(target.Position);

            //score *= attackers + 1;

            return score;
        }
    }
}
