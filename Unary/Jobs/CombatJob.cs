using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Behaviours;

namespace Unary.Jobs
{
    internal abstract class CombatJob : Job
    {
        public CombatJob(Unary unary) : base(unary)
        {
        }

        protected void PerformCombat(IEnumerable<Controller> units, IReadOnlyDictionary<Unit, double> targets)
        {
            TargetClosest(units, targets);
        }
        
        private void TargetClosest(IEnumerable<Controller> units, IReadOnlyDictionary<Unit, double> targets)
        {
            foreach (var unit in units)
            {
                if (unit.TryGetBehaviour<FightingBehaviour>(out var behaviour))
                {
                    behaviour.Target = null;
                    behaviour.Backup = null;
                    behaviour.Threat = null;

                    if (targets.Count > 0)
                    {
                        var pos = unit.Unit.Position;

                        foreach (var enemy in targets.Keys)
                        {
                            var distance = pos.DistanceTo(enemy.Position);

                            if (behaviour.Target == null || distance < pos.DistanceTo(behaviour.Target.Position))
                            {
                                behaviour.Target = enemy;
                                behaviour.Threat = enemy;
                            }

                            if (behaviour.Backup == null || distance < pos.DistanceTo(behaviour.Backup.Position))
                            {
                                if (distance > pos.DistanceTo(behaviour.Target.Position))
                                {
                                    behaviour.Backup = enemy;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
