using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using AoE2Lib.Bots.Modules;
using AoE2Lib.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Quaternary.Modules.UnitManagerModule;

namespace Quaternary.Modules
{
    class EconomyModule : Module
    {
        public class GatheringOperation
        {
            public readonly EconomyModule Module;
            public readonly List<Unit> Resources = new List<Unit>();
            public int MinimumWorkers { get; set; }
            public int MaximumWorkers { get; set; }
            public UnitGroup Workers { get; set; }
            public readonly List<Unit> Dropsites = new List<Unit>();

            private readonly Dictionary<Unit, Unit> Assignments = new Dictionary<Unit, Unit>();
            private readonly Random RNG = new Random(Guid.NewGuid().GetHashCode());

            public GatheringOperation(EconomyModule module)
            {
                Module = module;
                Workers = Module.Bot.GetModule<UnitManagerModule>().CreateGroup();
            }

            public void RequestUpdate()
            {
                foreach (var unit in Resources.Concat(Workers.Concat(Dropsites)))
                {
                    unit.RequestUpdate();
                }
            }

            public void Update()
            {
                const int WORKERS_PER_RESOURCE = 2;
                const int DROPSITE_MAX_DISTANCE = 3;

                Resources.RemoveAll(r => !r.Exists);
                foreach (var worker in Assignments.Keys.ToList())
                {
                    if (!Assignments[worker].Exists)
                    {
                        Assignments.Remove(worker);
                    }
                }

                if (Resources.Count == 0)
                {
                    return;
                }

                Dropsites.Clear();
                var type = Module.Bot.Mod.Mill;
                if (Resources[0].Resource == Resource.WOOD)
                {
                    type = Module.Bot.Mod.LumberCamp;
                }
                else if (Resources[0].Resource == Resource.GOLD)
                {
                    type = Module.Bot.Mod.GoldCamp;
                }
                else if (Resources[0].Resource == Resource.STONE)
                {
                    type = Module.Bot.Mod.StoneCamp;
                }

                foreach (var unit in Module.Bot.GetModule<UnitsModule>().Units.Values)
                {
                    if (unit.PlayerNumber == Module.Bot.PlayerNumber && unit.Exists && unit.BaseTypeId == type.Id)
                    {
                        var dist = Resources.Min(r => r.Position.DistanceTo(unit.Position));
                        if (dist <= DROPSITE_MAX_DISTANCE)
                        {
                            Dropsites.Add(unit);
                        }
                    }
                }

                if (Dropsites.Count == 0)
                {
                    Module.Bot.GetModule<UnitsModule>().FindUnits(Resources[0].Position, 10, Module.Bot.PlayerNumber, UnitFindType.BUILDING);
                    if (RNG.NextDouble() < 0.1)
                    {
                        BuildDropsite(type);
                    }
                    
                    return;
                }

                var counts = new Dictionary<Unit, int>();
                foreach (var resource in Resources)
                {
                    counts.Add(resource, 0);
                }
                foreach (var kvp in Assignments)
                {
                    counts[kvp.Value]++;
                }

                Resources.Sort((a, b) => Dropsites.Min(d => d.Position.DistanceTo(a.Position)).CompareTo(Dropsites.Min(d => d.Position.DistanceTo(b.Position))));
                foreach (var worker in Workers)
                {
                    if (!Assignments.ContainsKey(worker))
                    {
                        foreach (var resource in Resources)
                        {
                            if (counts[resource] < WORKERS_PER_RESOURCE)
                            {
                                Assignments.Add(worker, resource);
                                counts[resource]++;
                                
                                break;
                            }
                        }
                    }
                }
            }

            private void BuildDropsite(UnitDef type)
            {
                var placement = Module.Bot.GetModule<PlacementModule>();

                var best_score = double.MinValue;
                var best_pos = Resources[0].Position;
                for (int i = 0; i < 10; i++)
                {
                    var resource = Resources[RNG.Next(Resources.Count)];

                    foreach (var pos in placement.GetPlacementPositions(type, resource.Position, 0, false, 5))
                    {
                        var score = 0d;

                        Resources.Sort((a, b) => a.Position.DistanceTo(pos).CompareTo(b.Position.DistanceTo(pos)));
                        var count = Math.Min(10, Resources.Count);

                        for (int r = 0; r < count; r++)
                        {
                            var d = PlacementModule.GetFootprint(pos, type.Width, type.Height, 0).Min(f => f.DistanceTo(Resources[r].Position));
                            score -= Math.Pow(d + 0.5, 2);
                        }

                        if (score > best_score)
                        {
                            best_score = score;
                            best_pos = pos;
                        }
                    }
                }

                Module.Bot.GetModule<UnitsModule>().Build(type, best_pos);
            }
        }

        public readonly List<GatheringOperation> Operations = new List<GatheringOperation>();
        
        private GatheringOperation CurrentOperation { get; set; } = null;
        private readonly Random RNG = new Random(Guid.NewGuid().GetHashCode());

        public void Gather(List<Unit> resources)
        {
            var operation = new GatheringOperation(this);
            operation.Resources.AddRange(resources);

            Operations.Add(operation);
        }

        protected override IEnumerable<Command> RequestUpdate()
        {
            if (Operations.Count > 0)
            {
                CurrentOperation = Operations[RNG.Next(Operations.Count)];
                CurrentOperation.RequestUpdate();
            }

            return Enumerable.Empty<Command>();
        }

        protected override void Update()
        {
            if (CurrentOperation != null)
            {
                CurrentOperation.Update();
                CurrentOperation = null;
            }
        }
    }
}
