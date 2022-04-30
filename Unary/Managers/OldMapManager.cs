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
using Unary.Algorithms;

namespace Unary.Managers
{
    class OldMapManager : Manager
    {
        private readonly HashSet<Tile> ConstructionBlockedTiles = new();
        private readonly HashSet<Tile> ConstructionExcludedTiles = new();
        private readonly HashSet<Tile> LandBlockedTiles = new();
        private Dictionary<Tile, int> Distances { get; set; } = new();
        private readonly Dictionary<Tile, bool> Cliffs = new();
        private readonly Dictionary<Tile, Command> CliffFindCommands = new();

        public OldMapManager(Unary unary) : base(unary)
        {

        }

        public bool IsConstructionBlocked(Tile tile)
        {
            return ConstructionBlockedTiles.Contains(tile);
        }

        public bool IsConstructionExcluded(Tile tile)
        {
            return ConstructionExcludedTiles.Contains(tile);
        }

        public bool IsLandBlocked(Tile tile)
        {
            if (!tile.IsOnLand)
            {
                return true;
            }

            if (tile.Position.DistanceTo(Unary.GameState.MyPosition) < 4)
            {
                return false;
            }

            if (Cliffs.TryGetValue(tile, out bool cliff))
            {
                if (cliff)
                {
                    return true;
                }
            }

            return LandBlockedTiles.Contains(tile);
        }

        public bool CanReach(Tile tile)
        {
            return Distances.ContainsKey(tile);
        }

        public int GetPathDistance(Tile tile)
        {
            if (Distances.TryGetValue(tile, out int distance))
            {
                return distance;
            }
            else
            {
                return int.MaxValue;
            }
        }

        internal override void Update()
        {
            if (!Unary.GameState.Map.Updated)
            {
                return;
            }

            var sw = new Stopwatch();
            sw.Start();

            FindCliffs();
            Unary.Log.Info($"Find cliffs took {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            UpdateTiles();
            Unary.Log.Info($"Update Tiles took {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            UpdateDistances();
            Unary.Log.Info($"Update Distances took {sw.ElapsedMilliseconds} ms");

            sw.Stop();
        }

        private void FindCliffs()
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

            for (int i = 0; i < 1000; i++)
            {
                var x = Unary.Rng.Next(Unary.GameState.Map.Width);
                var y = Unary.Rng.Next(Unary.GameState.Map.Height);
                var tile = Unary.GameState.Map.GetTile(x, y);
                
                if (tile.Explored && !Cliffs.ContainsKey(tile) && !CliffFindCommands.ContainsKey(tile))
                {
                    var command = new Command();
                    command.Add(new SetGoal() { InConstGoalId = 100, InConstValue = tile.X });
                    command.Add(new SetGoal() { InConstGoalId = 101, InConstValue = tile.Y });

                    for (int id = CLIFF_START; id <= CLIFF_END; id++)
                    {
                        command.Add(new UpPointContains() { InConstObjectId = id, InGoalPoint = 100 });
                    }

                    CliffFindCommands.Add(tile, command);
                    Unary.ExecuteCommand(command);
                }

                if (CliffFindCommands.Count >= 100)
                {
                    break;
                }
            }

            Unary.Log.Debug($"Found {Cliffs.Values.Count(b => b)} cliffs");
        }

        private void UpdateTiles()
        {
            ConstructionBlockedTiles.Clear();
            ConstructionExcludedTiles.Clear();
            LandBlockedTiles.Clear();
            var map = Unary.GameState.Map;

            foreach (var player in Unary.GameState.GetPlayers())
            {
                foreach (var unit in player.Units)
                {
                    var construction = BlocksConstruction(unit);
                    var land = BlocksLand(unit);
                    var size = Unary.Mod.GetBuildingSize(unit[ObjectData.BASE_TYPE]);
                    var tile = unit.Tile;
                    var footprint = Utils.GetUnitFootprint(tile.X, tile.Y, size, size, 0);

                    for (int x = footprint.X; x < footprint.Right; x++)
                    {
                        for (int y = footprint.Y; y < footprint.Bottom; y++)
                        {
                            if (map.IsOnMap(x, y))
                            {
                                var t = map.GetTile(x, y);

                                if (construction)
                                {
                                    ConstructionBlockedTiles.Add(t);
                                }

                                if (land)
                                {
                                    LandBlockedTiles.Add(t);
                                }
                            }
                        }
                    }

                    if (construction)
                    {
                        if (unit[ObjectData.CMDID] == (int)CmdId.CIVILIAN_BUILDING || unit[ObjectData.CMDID] == (int)CmdId.MILITARY_BUILDING)
                        {
                            var excl = GetExclusionZoneSize(unit[ObjectData.BASE_TYPE]);
                            var t = unit.Tile;
                            footprint = Utils.GetUnitFootprint(t.X, t.Y, size, size, excl);
                            for (int x = footprint.X; x < footprint.Right; x++)
                            {
                                for (int y = footprint.Y; y < footprint.Bottom; y++)
                                {
                                    if (map.IsOnMap(x, y))
                                    {
                                        ConstructionExcludedTiles.Add(map.GetTile(x, y));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void UpdateDistances()
        {
            if (Unary.GameState.Map.IsOnMap(Unary.GameState.MyPosition))
            {
                var tile = Unary.GameState.Map.GetTile(Unary.GameState.MyPosition);
                var dict = Pathing.GetAllPathDistances(new[] { tile }, GetTileNeighbours);
                Distances = dict;
                Unary.Log.Debug($"Got {dict.Count} reachable tiles");
            }
        }

        private bool BlocksConstruction(Unit unit)
        {
            if (!unit.Targetable)
            {
                return false;
            }

            if (unit[ObjectData.SPEED] > 0)
            {
                return false;
            }

            return true;
        }

        private bool BlocksLand(Unit unit)
        {
            if (!unit.Targetable)
            {
                return false;
            }

            if (unit[ObjectData.SPEED] > 0)
            {
                return false;
            }

            if (unit[ObjectData.BASE_TYPE] == Unary.Mod.Farm)
            {
                return false;
            }

            return true;
        }

        private int GetExclusionZoneSize(int base_type_id)
        {
            if (base_type_id == Unary.Mod.TownCenter || base_type_id == Unary.Mod.TownCenterFoundation || base_type_id == Unary.Mod.Mill)
            {
                return 3;
            }
            else if (base_type_id == Unary.Mod.LumberCamp || base_type_id == Unary.Mod.MiningCamp || base_type_id == Unary.Mod.Dock)
            {
                return 3;
            }
            else if (base_type_id == Unary.Mod.Farm)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        private IEnumerable<Tile> GetTileNeighbours(Tile tile)
        {
            var neighbours = tile.GetNeighbours();

            for (int i = 0; i < neighbours.Count; i++)
            {
                var neighbour = neighbours[i];

                if (!IsLandBlocked(neighbour))
                {
                    yield return neighbour;
                }
            }
        }
    }
}
