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
        public bool NoOverkill { get; set; } = false;
        public bool Avoid { get; set; } = false;

        private readonly Dictionary<BattleUnit, BattleUnit> TargetAssignments = new Dictionary<BattleUnit, BattleUnit>();
        private readonly Random Rng = new Random();
        private BattleUnit FocusTarget { get; set; }
        private double FocusRegisteredDamage { get; set; }

        public void Restart()
        {
            TargetAssignments.Clear();
            FocusTarget = null;
            FocusRegisteredDamage = 0;
        }

        public void Update(BattleSimulation sim, int player)
        {
            FocusRegisteredDamage = 0;

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
                if (!unit.Alive)
                {
                    TargetAssignments.Remove(unit);

                    continue;
                }

                var target = TargetAssignments[unit];
                if (FocusFire)
                {
                    target = FocusTarget;

                    if (FocusTarget != null && NoOverkill == true && FocusRegisteredDamage > FocusTarget.CurrentHitpoints)
                    {
                        target = enemies[Rng.Next(enemies.Count)];
                        for (int i = 0; i < 5; i++)
                        {
                            var t = enemies[Rng.Next(enemies.Count)];
                            if (t.CurrentPosition.DistanceTo(unit.CurrentPosition) < target.CurrentPosition.DistanceTo(unit.CurrentPosition))
                            {
                                target = t;
                            }
                        }

                        if (target.CurrentPosition.DistanceTo(unit.CurrentPosition) > unit.Range)
                        {
                            target = FocusTarget;
                        }
                    }
                }

                if (target == null || !target.Alive)
                {
                    enemies.Sort((a, b) => a.CurrentPosition.DistanceTo(unit.CurrentPosition).CompareTo(b.CurrentPosition.DistanceTo(unit.CurrentPosition)));

                    if (FocusFire)
                    {
                        FocusTarget = enemies[0];

                        target = FocusTarget;
                    }
                    else
                    {
                        target = enemies[0];
                        if (enemies.Count > 1 && Rng.NextDouble() < 0.5)
                        {
                            target = enemies[1];
                            if (enemies.Count > 2 && Rng.NextDouble() < 0.5)
                            {
                                target = enemies[2];
                            }
                        }
                        TargetAssignments[unit] = target;
                    }
                }

                if (unit.NextAttack <= TimeSpan.Zero)
                {
                    if (target.CurrentPosition.DistanceTo(unit.CurrentPosition) <= unit.Range)
                    {
                        if (target == FocusTarget)
                        {
                            FocusRegisteredDamage += 1 * sim.GetDamage(unit, target);
                        }
                        
                        unit.Attack(target);
                    }
                    else if (Rng.NextDouble() < 0.5)
                    {
                        var btarget = enemies[Rng.Next(enemies.Count)];
                        for (int i = 0; i < 3; i++)
                        {
                            var t = enemies[Rng.Next(enemies.Count)];
                            if (t.CurrentPosition.DistanceTo(unit.CurrentPosition) < btarget.CurrentPosition.DistanceTo(unit.CurrentPosition))
                            {
                                btarget = t;
                            }
                        }

                        if (btarget.CurrentPosition.DistanceTo(unit.CurrentPosition) <= unit.Range)
                        {
                            unit.Attack(btarget);
                        }
                        else if (target.CurrentPosition.DistanceTo(unit.CurrentPosition) > unit.Range)
                        {
                            unit.MoveTo(target.CurrentPosition);
                        }
                    }
                    else if (target.CurrentPosition.DistanceTo(unit.CurrentPosition) > unit.Range)
                    {
                        unit.MoveTo(target.CurrentPosition);
                    }
                }
                else if (target.CurrentPosition.DistanceTo(unit.CurrentPosition) > unit.Range)
                {
                    unit.MoveTo(target.CurrentPosition);
                }
                else if (Avoid)
                {
                    if (target.CurrentPosition.DistanceTo(unit.CurrentPosition) < unit.Range - 1)
                    {
                        var next_pos = target.CurrentPosition - unit.CurrentPosition;
                        next_pos /= next_pos.Norm;
                        next_pos = next_pos.Rotate(0.1 * Math.PI);
                        next_pos *= -1;
                        next_pos += unit.CurrentPosition;
                        unit.MoveTo(next_pos);
                    }
                    else
                    {
                        var next_pos = target.CurrentPosition - unit.CurrentPosition;
                        next_pos /= next_pos.Norm;
                        next_pos = next_pos.Rotate(0.4 * Math.PI);
                        next_pos += unit.CurrentPosition;
                        unit.MoveTo(next_pos);
                    }
                }
                
            }
        }
    }
}
