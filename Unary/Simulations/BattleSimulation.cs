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

            public bool Alive => (int)Math.Round(CurrentHitpoints) > 0;
            public Position CurrentPosition { get; internal set; }
            public double CurrentHitpoints { get; internal set; }
            public TimeSpan NextAttack { get; internal set; }
            public BattleUnit Target { get; private set; }
            public Position MoveTarget { get; private set; }
            public bool HasProjectileInFlight { get; internal set; }
            public Position ProjectilePosition { get; internal set; }
            public Position ProjectileTarget { get; internal set; }

            public BattleUnit(int player, double max_hitpoints, double radius, double speed, double projectile_speed, TimeSpan reload_time, double range, Position position)
            {
                Player = player;
                MaxHitpoints = max_hitpoints;
                Radius = radius;
                Speed = speed;
                ProjectileSpeed = projectile_speed;
                ReloadTime = reload_time;
                Range = range;
                InitialPosition = position;

                CurrentPosition = position;
                CurrentHitpoints = MaxHitpoints;
                NextAttack = TimeSpan.Zero;
                HasProjectileInFlight = false;
                ProjectilePosition = position;
                ProjectileTarget = position;
            }

            public void Attack(BattleUnit target)
            {
                MoveTarget = CurrentPosition;
                Target = target;
            }

            public void MoveTo(Position position)
            {
                MoveTarget = position;
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
        public readonly Func<BattleUnit, BattleUnit, double> GetDamage;
        public TimeSpan GameTimeBetweenPolicyUpdates { get; private set; } = TimeSpan.FromSeconds(0.7);
        public TimeSpan GameTime { get; private set; }
        public TimeSpan GameTimeSincePolicyUpdate { get; private set; }

        private readonly Dictionary<int, IBattlePolicy> PlayerPolicies = new Dictionary<int, IBattlePolicy>();
        private readonly List<BattleUnit> Units = new List<BattleUnit>();
        private readonly Dictionary<Point, HashSet<BattleUnit>> MapUnits = new Dictionary<Point, HashSet<BattleUnit>>();
        private readonly Random Rng = new Random();

        public BattleSimulation(Func<Position, double> get_height, Func<Position, bool> is_obstructed, Func<BattleUnit, BattleUnit, double> get_damage)
        {
            GetHeight = get_height;
            IsObstructed = is_obstructed;
            GetDamage = get_damage;
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
            GameTimeSincePolicyUpdate = TimeSpan.FromDays(1);

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
            }

            foreach (var unit in Units)
            {
                while (GetCollision(unit, unit.CurrentPosition, false) != null)
                {
                    var pos = unit.CurrentPosition + new Position((Rng.NextDouble() * 4) - 2, (Rng.NextDouble() * 4) - 2);
                    if (IsObstructed(pos))
                    {
                        pos = unit.CurrentPosition;
                    }

                    SetUnitPosition(unit, pos);
                }

                unit.NextAttack = TimeSpan.Zero;
                unit.Attack(null);
                unit.MoveTo(unit.CurrentPosition);
                unit.HasProjectileInFlight = false;
            }
        }

        public void Tick(TimeSpan time)
        {
            if (MapUnits.Count == 0)
            {
                Restart();
            }

            GameTime += time;
            GameTimeSincePolicyUpdate += time;

            if (GameTimeSincePolicyUpdate > GameTimeBetweenPolicyUpdates)
            {
                foreach (var kvp in PlayerPolicies)
                {
                    kvp.Value.Update(this, kvp.Key);
                }

                GameTimeSincePolicyUpdate = TimeSpan.FromMilliseconds(100 * Rng.NextDouble());
            }

            // units

            foreach (var unit in Units.Where(u => u.Alive))
            {
                unit.NextAttack -= time;
                if (unit.NextAttack < TimeSpan.Zero)
                {
                    unit.NextAttack = TimeSpan.Zero;
                }

                if (unit.Target != null && !unit.Target.Alive)
                {
                    unit.Attack(null);
                }

                var next_pos = unit.CurrentPosition;

                if (unit.Target != null)
                {
                    if (unit.NextAttack <= TimeSpan.Zero && unit.CurrentPosition.DistanceTo(unit.Target.CurrentPosition) <= unit.Range)
                    {
                        unit.HasProjectileInFlight = true;
                        unit.ProjectilePosition = unit.CurrentPosition;
                        unit.ProjectileTarget = unit.Target.CurrentPosition;
                        unit.NextAttack = unit.ReloadTime;

                        //Debug.WriteLine($"player {unit.Player} unit {unit.GetHashCode()} attack {unit.Target.GetHashCode()}");
                    }
                }
                else
                {
                    if (unit.MoveTarget.DistanceTo(unit.CurrentPosition) > 0.01)
                    {
                        next_pos = unit.MoveTarget;
                    }
                }

                if (next_pos.DistanceTo(unit.CurrentPosition) > 0.01)
                {
                    var dist = unit.Speed * time.TotalSeconds;
                    if (next_pos.DistanceTo(unit.CurrentPosition) > dist)
                    {
                        next_pos -= unit.CurrentPosition;
                        next_pos /= next_pos.Norm;
                        next_pos *= dist;
                        next_pos += unit.CurrentPosition;
                    }

                    if (IsObstructed(next_pos) || GetCollision(unit, next_pos, false) != null)
                    {
                        next_pos -= unit.CurrentPosition;
                        next_pos = next_pos.Rotate(0.6 * Math.PI);
                        if (Rng.NextDouble() < 0.1)
                        {
                            next_pos = next_pos.Rotate(Rng.NextDouble() * 2 * Math.PI);
                        }
                        next_pos += unit.CurrentPosition;
                    }

                    if (IsObstructed(next_pos) == false && GetCollision(unit, next_pos, false) == null)
                    {
                        SetUnitPosition(unit, next_pos);
                    }
                }

                while (GetCollision(unit, unit.CurrentPosition, false) != null)
                {
                    var pos = unit.CurrentPosition + new Position((Rng.NextDouble() * 4) - 2, (Rng.NextDouble() * 4) - 2);
                    if (IsObstructed(pos))
                    {
                        pos = unit.CurrentPosition;
                    }

                    SetUnitPosition(unit, pos);
                }
            }

            // projectiles

            foreach (var unit in Units.Where(u => u.Alive))
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
                        collider.CurrentHitpoints -= GetDamage(unit, collider);
                        unit.HasProjectileInFlight = false;

                        //Debug.WriteLine($"player {collider.Player} unit {collider.GetHashCode()} was hit by {unit.GetHashCode()} hp {collider.CurrentHitpoints:N1}");
                    }
                    else
                    {
                        unit.ProjectilePosition = next_pos;
                    }

                    if (unit.ProjectilePosition.DistanceTo(unit.ProjectileTarget) < 0.01)
                    {
                        unit.HasProjectileInFlight = false;
                    }
                }
            }
        }

        public IEnumerable<BattleUnit> GetUnits(int player = -1)
        {
            return Units.Where(u => (player == -1 || u.Player == player) && u.Alive);
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
            if (!MapUnits.ContainsKey(point))
            {
                MapUnits.Add(point, new HashSet<BattleUnit>());
            }
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
                        if (other == unit || other.Alive == false)
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
                            radius *= 1.1;
                        }

                        if (position.DistanceTo(other.CurrentPosition) <= radius)
                        {
                            return other;
                        }
                    }
                }
            }

            return null;
        }
    }
}
