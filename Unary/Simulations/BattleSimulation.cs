using AoE2Lib.Bots;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Unary.Simulations
{
    class BattleSimulation
    {
        public class BattleUnit
        {
            public readonly int Player;
            public readonly double MaxHitpoints;
            public readonly double Radius;
            public readonly double Speed;
            public readonly double ProjectileSpeed;
            public readonly TimeSpan ReloadTime;
            public readonly double Range;
            public readonly Position InitialPosition;
            public readonly Func<BattleUnit, double> GetDamage;

            public Position CurrentPosition { get; internal set; }
            public double CurrentHitpoints { get; internal set; }
            public TimeSpan NextAttack { get; internal set; }
            public BattleUnit Target { get; private set; }
            public Position MoveTo { get; private set; }
            public bool HasProjectileInFlight { get; internal set; }
            public Position ProjectilePosition { get; internal set; }
            public Position ProjectileTarget { get; internal set; }

            public BattleUnit(int player, double max_hitpoints, double radius, double speed, double projectile_speed, TimeSpan reload_time, double range, Position position, Func<BattleUnit, double> get_damage)
            {
                Player = player;
                MaxHitpoints = max_hitpoints;
                Radius = radius;
                Speed = speed;
                ProjectileSpeed = projectile_speed;
                ReloadTime = reload_time;
                Range = range;
                InitialPosition = position;
                GetDamage = get_damage;

                CurrentPosition = position;
                CurrentHitpoints = MaxHitpoints;
                NextAttack = TimeSpan.Zero;
                HasProjectileInFlight = false;
                ProjectilePosition = position;
                ProjectileTarget = position;
            }

            public void Attack(BattleUnit target)
            {
                MoveTo = CurrentPosition;
                Target = target;
            }

            public void Move(Position position)
            {
                MoveTo = position;
                Target = null;
            }
        }

        public interface IBattlePolicy
        {
            public void Restart();
            public void Update(BattleSimulation sim, int player);
        }

        public readonly Func<Position, double> GetHeight;
        public readonly Func<Position, bool> IsObstructed;
        public TimeSpan GameTime { get; private set; }

        private readonly Dictionary<int, IBattlePolicy> PlayerPolicies = new Dictionary<int, IBattlePolicy>();
        private readonly List<BattleUnit> Units = new List<BattleUnit>();
        private readonly Dictionary<Point, HashSet<BattleUnit>> MapUnits = new Dictionary<Point, HashSet<BattleUnit>>();
        private readonly Random Rng = new Random();

        public BattleSimulation(Func<Position, double> get_height, Func<Position, bool> is_obstructed)
        {
            GetHeight = get_height;
            IsObstructed = is_obstructed;
        }

        public void SetPolicy(int player, IBattlePolicy policy)
        {
            PlayerPolicies[player] = policy;
        }

        public void AddUnit(BattleUnit unit)
        {
            Units.Add(unit);
        }

        public void Restart()
        {
            GameTime = TimeSpan.Zero;

            foreach (var policy in PlayerPolicies.Values)
            {
                policy.Restart();
            }

            foreach (var units in MapUnits.Values)
            {
                units.Clear();
            }

            foreach (var unit in Units)
            {
                unit.CurrentHitpoints = unit.MaxHitpoints;
                SetUnitPosition(unit, unit.InitialPosition);
                unit.NextAttack = TimeSpan.Zero;
                unit.Attack(null);
                unit.HasProjectileInFlight = false;
            }
        }

        public void Tick(TimeSpan time)
        {
            GameTime += time;

            foreach (var kvp in PlayerPolicies)
            {
                kvp.Value.Update(this, kvp.Key);
            }

            // units

            foreach (var unit in Units.Where(u => u.CurrentHitpoints > 0))
            {
                

                unit.NextAttack -= time;
                if (unit.NextAttack < TimeSpan.Zero)
                {
                    unit.NextAttack = TimeSpan.Zero;
                }

                if (unit.Target != null && unit.Target.CurrentHitpoints <= 0)
                {
                    unit.Attack(null);
                }

                if (unit.Target != null)
                {
                    if (unit.NextAttack <= TimeSpan.Zero && unit.CurrentPosition.DistanceTo(unit.Target.CurrentPosition) <= unit.Range)
                    {
                        unit.HasProjectileInFlight = true;
                        unit.ProjectilePosition = unit.CurrentPosition;
                        unit.ProjectileTarget = unit.Target.CurrentPosition;
                        unit.NextAttack = unit.ReloadTime;
                    }
                }
                else
                {
                    if (unit.MoveTo.DistanceTo(unit.CurrentPosition) > 0.01)
                    {
                        var next_pos = unit.MoveTo;

                        var dist = unit.Speed * time.TotalSeconds;
                        if (unit.MoveTo.DistanceTo(unit.CurrentPosition) > dist)
                        {
                            next_pos = unit.MoveTo - unit.CurrentPosition;
                            next_pos /= next_pos.Norm;
                            next_pos *= dist;
                            next_pos += unit.CurrentPosition;
                        }

                        if (IsObstructed(next_pos) || GetCollision(unit, next_pos, false) != null)
                        {
                            next_pos -= unit.CurrentPosition;
                            var rotate = 0.6 * Math.PI;
                            if (unit.GetHashCode() % 2 == 0)
                            {
                                //rotate *= -1;
                            }
                            next_pos = next_pos.Rotate(rotate);
                            if (Rng.NextDouble() < 0.1)
                            {
                                next_pos = next_pos.Rotate(Rng.NextDouble() * 2 * Math.PI);
                            }
                            next_pos += unit.CurrentPosition;
                        }

                        if (!IsObstructed(next_pos))
                        {
                            if (GetCollision(unit, next_pos, false) == null)
                            {
                                SetUnitPosition(unit, next_pos);
                            }
                        }
                    }
                }
            }

            // projectiles

            foreach (var unit in Units.Where(u => u.CurrentHitpoints > 0))
            {
                

                if (unit.HasProjectileInFlight)
                {
                    var next_pos = unit.ProjectileTarget;
                    var dist = unit.ProjectileSpeed * time.TotalSeconds;
                    if (next_pos.DistanceTo(unit.ProjectilePosition) > dist)
                    {
                        next_pos -= unit.ProjectilePosition;
                        next_pos /= next_pos.Norm;
                        next_pos *= dist;
                        next_pos += unit.ProjectilePosition;
                    }

                    var collider = GetCollision(unit, next_pos, true);
                    if (collider != null)
                    {
                        collider.CurrentHitpoints -= unit.GetDamage(collider);
                        unit.HasProjectileInFlight = false;
                    }
                    else
                    {
                        unit.ProjectilePosition = next_pos;
                    }

                    if (unit.ProjectilePosition == unit.ProjectileTarget)
                    {
                        unit.HasProjectileInFlight = false;
                    }
                }
            }
        }

        public IEnumerable<BattleUnit> GetUnits(int player = -1)
        {
            return Units.Where(u => (player == -1 || u.Player == player) && u.CurrentHitpoints > 0);
        }

        private void SetUnitPosition(BattleUnit unit, Position position)
        {
            var point = new Point(unit.CurrentPosition.PointX, unit.CurrentPosition.PointY);
            if (!MapUnits.ContainsKey(point))
            {
                MapUnits.Add(point, new HashSet<BattleUnit>());
            }

            MapUnits[point].Remove(unit);
            point = new Point(position.PointX, position.PointY);
            MapUnits[point].Add(unit);

            unit.CurrentPosition = position;
        }

        private BattleUnit GetCollision(BattleUnit unit, Position position, bool projectile)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    var point = new Point(position.PointX + dx, position.PointY + dy);
                    if (!MapUnits.ContainsKey(point))
                    {
                        MapUnits.Add(point, new HashSet<BattleUnit>());
                    }

                    foreach (var other in MapUnits[point])
                    {
                        if (other == unit || other.CurrentHitpoints <= 0)
                        {
                            continue;
                        }

                        if (projectile == true && unit.Player == other.Player)
                        {
                            continue;
                        }

                        var radius = other.Radius;
                        if (!projectile)
                        {
                            radius += unit.Radius;
                        }

                        if (position.DistanceTo(other.CurrentPosition) <= radius)
                        {
                            return unit;
                        }
                    }
                }
            }

            return null;
        }
    }
}
