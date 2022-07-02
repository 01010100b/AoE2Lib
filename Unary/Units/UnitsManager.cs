using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Units
{
    // controllers
    internal class UnitsManager : Manager
    {

        private readonly Dictionary<Unit, Controller> Controllers = new();

        public UnitsManager(Unary unary) : base(unary)
        {

        }

        public bool TryGetController(Unit unit, out Controller controller) => Controllers.TryGetValue(unit, out controller);

        public IEnumerable<Controller> GetControllers() => Controllers.Values;

        protected internal override void Update()
        {
            foreach (var unit in Unary.GameState.MyPlayer.Units.Where(u => u.Targetable))
            {
                if (!Controllers.ContainsKey(unit))
                {
                    var controller = new Controller(unit, Unary);
                    Controllers.Add(unit, controller);
                }
            }

            var sw = new Stopwatch();
            sw.Start();
            UpdateBuilders();
            Unary.Log.Info($"Update Builders took {sw.ElapsedMilliseconds:N0} ms");
            UpdateControllers();
        }

        private void UpdateBuilders()
        {
            var spots = ObjectPool.Get(() => new List<ConstructionSpotBehaviour>(), x => x.Clear());
            var builders = ObjectPool.Get(() => new List<BuildBehaviour>(), x => x.Clear());
            var assigned = ObjectPool.Get(() => new Dictionary<ConstructionSpotBehaviour, int>(), x => x.Clear());

            foreach (var controller in Controllers.Values)
            {
                if (controller.TryGetBehaviour<ConstructionSpotBehaviour>(out var spot))
                {
                    if (spot.RequestedBuilders > 0)
                    {
                        spots.Add(spot);

                        if (!assigned.ContainsKey(spot))
                        {
                            assigned.Add(spot, 0);
                        }
                    }
                }

                if (controller.TryGetBehaviour<BuildBehaviour>(out var build))
                {
                    builders.Add(build);

                    if (build.ConstructionSpot != null)
                    {
                        if (!assigned.ContainsKey(build.ConstructionSpot))
                        {
                            assigned.Add(build.ConstructionSpot, 0);
                        }

                        assigned[build.ConstructionSpot]++;
                    }
                }
            }

            if (builders.Count == 0)
            {
                ObjectPool.Add(spots);
                ObjectPool.Add(builders);
                ObjectPool.Add(assigned);

                return;
            }

            // retask finished builders

            foreach (var builder in builders.Where(b => b.ConstructionSpot != null))
            {
                if (assigned[builder.ConstructionSpot] > builder.ConstructionSpot.RequestedBuilders)
                {
                    ConstructionSpotBehaviour best_spot = null;

                    foreach (var spot in spots)
                    {
                        if (assigned[spot] < spot.RequestedBuilders)
                        {
                            if (best_spot == null)
                            {
                                best_spot = spot;
                            }
                            else
                            {
                                var d1 = builder.Controller.Unit.Position.DistanceTo(best_spot.Controller.Unit.Position);
                                var d2 = builder.Controller.Unit.Position.DistanceTo(spot.Controller.Unit.Position);

                                if (d2 < d1)
                                {
                                    best_spot = spot;
                                }
                            }
                        }
                    }

                    assigned[builder.ConstructionSpot]--;
                    builder.ConstructionSpot = null;

                    if (best_spot != null)
                    {
                        BuildBehaviour best_builder = null;

                        foreach (var build in builders.Where(b => b.ConstructionSpot == null))
                        {
                            if (best_builder == null)
                            {
                                best_builder = build;
                            }
                            else
                            {
                                var d1 = best_spot.Controller.Unit.Position.DistanceTo(best_builder.Controller.Unit.Position);
                                var d2 = best_spot.Controller.Unit.Position.DistanceTo(build.Controller.Unit.Position);

                                if (d2 < d1)
                                {
                                    best_builder = build;
                                }
                            }
                        }

                        if (best_builder != null)
                        {
                            best_builder.ConstructionSpot = best_spot;
                            assigned[best_spot]++;
                        }
                    }
                }
            }

            if (spots.Count == 0)
            {
                ObjectPool.Add(spots);
                ObjectPool.Add(builders);
                ObjectPool.Add(assigned);

                return;
            }

            // assign new builders

            var max_builders = Math.Max(Unary.Settings.MinBuilders, (int)Math.Round(Unary.Settings.MaxBuildersPercentage * builders.Count));

            if (assigned.Values.Sum() < max_builders)
            {
                ConstructionSpotBehaviour best_spot = null;

                for (int i = 0; i < 10; i++)
                {
                    var spot = spots[Unary.Rng.Next(spots.Count)];

                    if (assigned[spot] < spot.RequestedBuilders)
                    {
                        if (best_spot == null)
                        {
                            best_spot = spot;
                        }
                        else
                        {
                            var best_spot_dist = 0d;
                            var spot_dist = 0d;

                            foreach (var kvp in assigned)
                            {
                                if (kvp.Value > 0)
                                {
                                    best_spot_dist += kvp.Key.Controller.Unit.Position.DistanceTo(best_spot.Controller.Unit.Position);
                                    spot_dist += kvp.Key.Controller.Unit.Position.DistanceTo(spot.Controller.Unit.Position);
                                }
                            }

                            if (spot_dist > best_spot_dist)
                            {
                                best_spot = spot;
                            }
                        }
                    }
                }

                if (best_spot != null)
                {
                    BuildBehaviour best_builder = null;

                    foreach (var builder in builders.Where(b => b.ConstructionSpot == null))
                    {
                        if (best_builder == null)
                        {
                            best_builder = builder;
                        }
                        else
                        {
                            var d1 = best_spot.Controller.Unit.Position.DistanceTo(best_builder.Controller.Unit.Position);
                            var d2 = best_spot.Controller.Unit.Position.DistanceTo(builder.Controller.Unit.Position);

                            if (d2 < d1)
                            {
                                best_builder = builder;
                            }

                        }
                    }

                    if (best_builder != null)
                    {
                        best_builder.ConstructionSpot = best_spot;
                    }
                }
            }

            ObjectPool.Add(spots);
            ObjectPool.Add(builders);
            ObjectPool.Add(assigned);
        }

        private void UpdateControllers()
        {
            var times = ObjectPool.Get(() => new Dictionary<Type, KeyValuePair<int, TimeSpan>>(), x => x.Clear());
            var controllers = ObjectPool.Get(() => new List<Controller>(), x => x.Clear());
            var behaviours = ObjectPool.Get(() => new List<KeyValuePair<Type, KeyValuePair<int, TimeSpan>>>(), x => x.Clear());

            controllers.AddRange(Controllers.Values);

            foreach (var controller in controllers)
            {
                if (controller.Exists)
                {
                    controller.Tick(times);
                }
                else
                {
                    Controllers.Remove(controller.Unit);
                }
            }

            behaviours.AddRange(times);
            behaviours.Sort((a, b) => b.Value.Value.CompareTo(a.Value.Value));

            foreach (var behaviour in behaviours)
            {
                Unary.Log.Info($"{behaviour.Key.Name} ran {behaviour.Value.Key} times for a total of {behaviour.Value.Value.TotalMilliseconds:N2} ms");
            }

            ObjectPool.Add(times);
            ObjectPool.Add(controllers);
            ObjectPool.Add(behaviours);
        }
    }
}
