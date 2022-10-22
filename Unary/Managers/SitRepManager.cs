using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Managers
{
    internal class SitRepManager : Manager
    {
        private class SitRep
        {
            public readonly Tile Tile;
            public bool IsLandAccessible { get; internal set; }
            public bool IsWaterAccessible { get; internal set; }
            public bool IsConstructionBlocked { get; internal set; }
            public bool IsConstructionExcluded { get; internal set; }
            public int PathDistanceToHome { get; internal set; }

            public SitRep(Tile tile)
            {
                Tile = tile;
                Reset();
            }

            public void Reset()
            {
                IsLandAccessible = Tile.IsOnLand;
                IsWaterAccessible = true;
                IsConstructionBlocked = false;
                IsConstructionExcluded = false;
                PathDistanceToHome = int.MaxValue;
            }
        }

        private SitRep this[Tile tile] => GetSitRep(tile);

        private readonly Dictionary<Tile, SitRep> SitReps = new();
        private readonly Dictionary<Tile, bool> Cliffs = new();
        private readonly Dictionary<Tile, Command> CliffFindCommands = new();

        public SitRepManager(Unary unary) : base(unary)
        {

        }

        protected internal override void Update()
        {
            var sitreps = ObjectPool.Get(() => new List<SitRep>(), x => x.Clear());
            sitreps.AddRange(SitReps.Values);

            foreach (var sitrep in sitreps)
            {
                sitrep.Reset();

                if (Cliffs.TryGetValue(sitrep.Tile, out var cliff))
                {
                    if (cliff)
                    {
                        const int SIZE = 2;

                        var footprint = Utils.GetUnitFootprint(sitrep.Tile.X, sitrep.Tile.Y, SIZE, SIZE);

                        for (int x = footprint.X; x < footprint.Width; x++)
                        {
                            for (int y = footprint.Y; y < footprint.Height; y++)
                            {
                                if (Unary.GameState.Map.TryGetTile(x, y, out var tile))
                                {
                                    var sr = this[tile];

                                    sr.IsLandAccessible = false;
                                    sr.IsWaterAccessible = false;
                                    sr.IsConstructionBlocked = true;
                                }
                            }
                        }
                    }
                }
            }

            ObjectPool.Add(sitreps);

            var sw = new Stopwatch();

            sw.Restart();
            UpdateCliffs();
            Unary.Log.Debug($"SitRepManager.UpdateCliffs took {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            UpdateAccessibilities();
            Unary.Log.Debug($"SitRepManager.UpdateAccessibilities took {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            UpdatePathDistances();
            Unary.Log.Debug($"SitRepManager.UpdatePathDistances took {sw.ElapsedMilliseconds} ms");
        }

        private void UpdateCliffs()
        {
            const int CLIFF_START = 264;
            const int CLIFF_END = 272;

            foreach (var kvp in CliffFindCommands)
            {
                var tile = kvp.Key;
                var command = kvp.Value;

                if (command.HasResponses)
                {
                    var responses = command.GetResponses();
                    var contains = false;

                    for (int i = 2; i < responses.Count; i++)
                    {
                        if (responses[i].Unpack<UpPointContainsResult>().Result)
                        {
                            contains = true;
                        }
                    }

                    Cliffs.Add(tile, contains);
                }
            }

            CliffFindCommands.Clear();

            for (int i = 0; i < 2000; i++)
            {
                if (CliffFindCommands.Count >= 100)
                {
                    break;
                }

                var x = Unary.Rng.Next(Unary.GameState.Map.Width);
                var y = Unary.Rng.Next(Unary.GameState.Map.Height);

                if (Unary.GameState.Map.TryGetTile(x, y, out var tile))
                {
                    if (tile.Explored && !Cliffs.ContainsKey(tile) && !CliffFindCommands.ContainsKey(tile))
                    {
                        var command = new Command();
                        command.Add(new SetGoal() { InConstGoalId = Bot.GOAL_START, InConstValue = tile.X });
                        command.Add(new SetGoal() { InConstGoalId = Bot.GOAL_START + 1, InConstValue = tile.Y });

                        for (int id = CLIFF_START; id <= CLIFF_END; id++)
                        {
                            command.Add(new UpPointContains() { InConstObjectId = id, InGoalPoint = Bot.GOAL_START });
                        }

                        CliffFindCommands.Add(tile, command);
                        Unary.ExecuteCommand(command);
                    }
                }
            }

            Unary.Log.Debug($"Found {Cliffs.Values.Count(b => b):N0} cliffs");
        }

        private void UpdateAccessibilities()
        {
            var map = Unary.GameState.Map;

            foreach (var player in Unary.GameState.GetPlayers())
            {
                foreach (var unit in player.Units)
                {
                    var blocks_construction = true;
                    var blocks_movement = true;
                    var size = Unary.Mod.GetBuildingSizeOld(unit[ObjectData.BASE_TYPE]);

                    if (unit.Targetable == false || unit[ObjectData.SPEED] > 0)
                    {
                        blocks_construction = false;
                        blocks_movement = false;
                    }

                    if (unit[ObjectData.BASE_TYPE] == Unary.Mod.Farm)
                    {
                        blocks_movement = false;
                    }

                    if (blocks_construction || blocks_movement)
                    {
                        var footprint = Utils.GetUnitFootprint(unit.Tile.X, unit.Tile.Y, size, size, 0);

                        for (int x = footprint.X; x < footprint.Right; x++)
                        {
                            for (int y = footprint.Y; y < footprint.Bottom; y++)
                            {
                                if (map.TryGetTile(x, y, out var t))
                                {
                                    var sr = this[t];

                                    if (blocks_construction)
                                    {
                                        sr.IsConstructionBlocked = true;
                                    }

                                    if (blocks_movement)
                                    {
                                        sr.IsLandAccessible = false;
                                    }
                                }
                            }
                        }
                    }

                    if (blocks_construction && unit.IsBuilding)
                    {
                        foreach (var zone in GetExclusionZones(unit))
                        {
                            for (int x = zone.X; x < zone.Right; x++)
                            {
                                for (int y = zone.Y; y < zone.Bottom; y++)
                                {
                                    if (map.TryGetTile(x, y, out var t))
                                    {
                                        var sr = this[t];

                                        sr.IsConstructionExcluded = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void UpdatePathDistances()
        {
            if (Unary.GameState.Map.TryGetTile(Unary.TownManager.MyPosition, out  var tile))
            {
                var dict = new Dictionary<Tile, int>() { { tile, 0 } };
                Algorithms.AddAllPathDistances(dict, GetPathNeighbours);

                foreach (var kvp in dict)
                {
                    var sitrep = this[kvp.Key];
                    sitrep.PathDistanceToHome = kvp.Value;
                }

                Unary.Log.Debug($"Got {dict.Count:N0} reachable tiles");
            }
        }

        private SitRep GetSitRep(Tile tile)
        {
            if (SitReps.TryGetValue(tile, out var sitrep))
            {
                return sitrep;
            }
            else
            {
                sitrep = new SitRep(tile);
                SitReps.Add(tile, sitrep);

                return sitrep;
            }
        }

        private readonly List<Tile> __PathNeighbours = new();
        private IReadOnlyList<Tile> GetPathNeighbours(Tile tile)
        {
            var neighbours = tile.GetNeighbours();

            __PathNeighbours.Clear();

            for (int i = 0; i < neighbours.Count; i++)
            {
                var neighbour = this[neighbours[i]];

                if (neighbour.IsLandAccessible || neighbour.Tile.Center.DistanceTo(Unary.TownManager.MyPosition) < 3)
                {
                    __PathNeighbours.Add(neighbour.Tile);
                }
            }

            return __PathNeighbours;
        }

        private IEnumerable<Rectangle> GetExclusionZones(Unit building)
        {
            var size = Unary.Mod.GetBuildingSizeOld(building[ObjectData.BASE_TYPE]);
            var exclusion = 1;

            yield return Utils.GetUnitFootprint(building.Position.PointX, building.Position.PointY, size, size, exclusion);
        }
    }
}
