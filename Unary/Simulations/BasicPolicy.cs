using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static Unary.Simulations.BattleSimulation;

namespace Unary.Simulations
{
    class BasicPolicy : IBattlePolicy
    {
        public bool FocusFire { get; set; } = false;

        private readonly Dictionary<BattleUnit, BattleUnit> TargetAssignments = new Dictionary<BattleUnit, BattleUnit>();
        private readonly Random Rng = new Random();

        public void Restart()
        {
            TargetAssignments.Clear();
        }

        public void Update(BattleSimulation sim, int player)
        {
            var enemies = new List<BattleUnit>();

            foreach (var unit in sim.GetUnits())
            {
                if (unit.Player == player)
                {
                    if (!TargetAssignments.ContainsKey(unit))
                    {
                        TargetAssignments.Add(unit, null);
                    }
                }
                else
                {
                    enemies.Add(unit);
                }
            }

            var units = new List<BattleUnit>();
            units.AddRange(TargetAssignments.Keys);

            if (units.Count == 0 || enemies.Count == 0)
            {
                return;
            }

            foreach (var unit in units)
            {
                if (unit.CurrentHitpoints <= 0)
                {
                    TargetAssignments.Remove(unit);

                    continue;
                }

                var target = TargetAssignments[unit];
                if (target == null || target.CurrentHitpoints <= 0)
                {
                    target = enemies[0];
                    for (int i = 1; i < enemies.Count; i++)
                    {
                        var t = enemies[i];
                        if (FocusFire)
                        {
                            if (t.GetHashCode() < target.GetHashCode())
                            {
                                target = t;
                            }
                        }
                        else if (t.CurrentPosition.DistanceTo(unit.CurrentPosition) < target.CurrentPosition.DistanceTo(unit.CurrentPosition))
                        {
                            target = t;
                        }
                    }

                    TargetAssignments[unit] = target;
                }

                if (target.CurrentPosition.DistanceTo(unit.CurrentPosition) <= unit.Range)
                {
                    unit.Attack(target);
                }
                else
                {
                    unit.Move(target.CurrentPosition);
                }
            }
        }
    }
}
