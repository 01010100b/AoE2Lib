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
        private BattleUnit FocusTarget { get; set; }
        private BattleUnit FocusBackup { get; set; }
        private double TargetRegisteredDamage { get; set; }

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
                if (!unit.Alive)
                {
                    TargetAssignments.Remove(unit);

                    continue;
                }

                var target = TargetAssignments[unit];
                if (FocusFire)
                {
                    target = FocusTarget;
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
                        target = enemies[Rng.Next(enemies.Count)];
                        TargetAssignments[unit] = target;
                    }
                }

                if (unit.NextAttack <= TimeSpan.Zero)
                {
                    if (target.CurrentPosition.DistanceTo(unit.CurrentPosition) <= unit.Range)
                    {
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
                            unit.Move(target.CurrentPosition);
                        }
                    }
                }
                else if (target.CurrentPosition.DistanceTo(unit.CurrentPosition) > unit.Range)
                {
                    unit.Move(target.CurrentPosition);
                }
            }
        }
    }
}
