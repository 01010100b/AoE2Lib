using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Behaviours;
using static Grpc.Core.Metadata;

namespace Unary.Jobs
{
    internal class BuildersManagementJob : ManagementJob
    {
        public override string Name => "Builders management";

        private int MaxBuilders { get; set; } = 4;
        private readonly HashSet<int> ExcludedTypes = new();

        public BuildersManagementJob(Unary unary) : base(unary)
        {
        }

        protected override void Initialize()
        {
            var civ = Unary.CivInfo;
            ExcludedTypes.Add(civ.GetFoundationId(civ.FarmId));
            //ExcludedTypes.Add(civ.GetFoundationId(civ.MillId));
            ExcludedTypes.Add(civ.GetFoundationId(civ.LumberCampId));
            ExcludedTypes.Add(civ.GetFoundationId(civ.GoldMiningCampId));
            ExcludedTypes.Add(civ.GetFoundationId(civ.StoneMiningCampId));
        }

        protected override void Update()
        {
            var remaining_spots = ObjectPool.Get(() => new Dictionary<Unit, int>(), x => x.Clear());
            var current_builders = ObjectPool.Get(() => new List<Controller>(), x => x.Clear());
            var available_builders = ObjectPool.Get(() => new List<Controller>(), x => x.Clear());

            foreach (var unit in Unary.GameState.MyPlayer.Units)
            {
                var req = GetRequiredBuilders(unit);

                if (req > 0)
                {
                    remaining_spots.Add(unit, req);
                }

                if (Unary.UnitsManager.TryGetController(unit, out var controller))
                {
                    if (controller.TryGetBehaviour<ConstructingBehaviour>(out var behaviour))
                    {
                        if (behaviour.Construction == null)
                        {
                            available_builders.Add(controller);
                        }
                        else if (!ExcludedTypes.Contains(behaviour.Construction[ObjectData.BASE_TYPE]))
                        {
                            current_builders.Add(controller);
                        }
                    }
                }
            }

            foreach (var builder in current_builders)
            {
                if (builder.TryGetBehaviour<ConstructingBehaviour>(out var behaviour))
                {
                    if (remaining_spots.ContainsKey(behaviour.Construction))
                    {
                        remaining_spots[behaviour.Construction]--;
                    }
                }
            }

            foreach (var builder in current_builders)
            {
                if (builder.TryGetBehaviour<ConstructingBehaviour>(out var behaviour))
                {
                    var retask = true;

                    if (remaining_spots.TryGetValue(behaviour.Construction, out var spots))
                    {
                        if (spots >= 0)
                        {
                            retask = false;
                        }
                        else
                        {
                            remaining_spots[behaviour.Construction]++;
                        }
                    }

                    if (retask)
                    {
                        Assign(builder, remaining_spots.Where(x => x.Value > 0).Select(x => x.Key));

                        if (behaviour.Construction != null)
                        {
                            remaining_spots[behaviour.Construction]--;
                        }
                    }
                }
            }

            if (current_builders.Count < MaxBuilders)
            {
                var construction = remaining_spots.Where(x => x.Value > 0).Select(x => x.Key).FirstOrDefault();

                if (construction != null)
                {
                    Assign(construction, available_builders);
                }
            }

            ObjectPool.Add(remaining_spots);
            ObjectPool.Add(current_builders);
            ObjectPool.Add(available_builders);
        }

        protected override void OnClosed()
        {
        }


        private int GetRequiredBuilders(Unit unit)
        {
            if (!unit.IsBuilding)
            {
                return 0;
            }
            else if (unit[ObjectData.STATUS] != 0)
            {
                return 0;
            }
            else if (ExcludedTypes.Contains(unit[ObjectData.BASE_TYPE]))
            {
                return 0;
            }

            return 1;
        }

        private void Assign(Controller builder, IEnumerable<Unit> constructions)
        {
            Unit best = null;

            foreach (var construction in constructions)
            {
                if (best == null || builder.Unit.Position.DistanceTo(construction.Position) < builder.Unit.Position.DistanceTo(best.Position))
                {
                    best = construction;
                }
            }

            if (builder.TryGetBehaviour<ConstructingBehaviour>(out var behaviour))
            {
                behaviour.Construction = best;
            }
        }

        private void Assign(Unit construction, IEnumerable<Controller> builders)
        {
            Controller best = null;
            var best_distance = double.MaxValue;

            foreach (var builder in builders)
            {
                var distance = builder.Unit.Position.DistanceTo(construction.Position);

                if (builder.CurrentJob == null)
                {
                    distance /= 2;
                }

                if (best == null || distance < best_distance)
                {
                    best = builder;
                    best_distance = distance;
                }
            }

            if (best != null)
            {
                if (best.TryGetBehaviour<ConstructingBehaviour>(out var behaviour))
                {
                    behaviour.Construction = construction;
                }
            }
        }
    }
}
