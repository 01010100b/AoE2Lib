using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Unary.GameElements;
using Unary.Mods;
using Unary.Utils;

namespace Unary.Modules
{
    public class BuildModule : Module
    {
        private class BuildCommand : Command
        {
            public UnitDef Building { get; set; }
            public Position Position { get; set; }
            public int MaxCount { get; set; }
            public int Concurrent { get; set; }
            public int CountTotal { get; set; } = -1;
            public int Pending { get; set; }
            public bool CanAfford { get; set; }
            public bool CanPlace { get; set; } = false;
        }

        public int MaxBuildRange { get; set; } = 20;
        public int MaxCampRange { get; set; } = 35;

        private readonly List<BuildCommand> Commands = new List<BuildCommand>();
        private readonly Random RNG = new Random(Guid.NewGuid().GetHashCode() ^ DateTime.UtcNow.GetHashCode());

        public void Build(UnitDef building, Position position, int max = int.MaxValue, int concurrent = int.MaxValue)
        {
            if (Commands.Select(c => c.Building.Id).Contains(building.Id))
            {
                return;
            }

            var command = new BuildCommand
            {
                Building = building,
                Position = position,
                MaxCount = max,
                Concurrent = concurrent,
                CountTotal = -1,
                Pending = -1,
                CanAfford = false,
                CanPlace = false
            };

            Commands.Add(command);
        }

        public void Build(UnitDef building, Bot bot, int max = int.MaxValue, int concurrent = int.MaxValue)
        {
            if (Commands.Select(c => c.Building.Id).Contains(building.Id))
            {
                return;
            }

            if (building.Id == bot.Mod.LumberCamp.Id)
            {
                var trees = bot.GameState.GetUnitsInRange(bot.GameState.MyPosition, MaxCampRange).Where(u => u.Class == UnitClass.Tree).ToList();
                if (trees.Count < 10)
                {
                    return;
                }

                var positions = trees.Select(t => t.Position).Take(50).ToList();
                var scores = new Dictionary<Position, double>();
                foreach (var pos in positions)
                {
                    var score = 0d;

                    trees.Sort((a, b) => a.Position.DistanceTo(pos).CompareTo(b.Position.DistanceTo(pos)));
                    foreach (var tree in trees.Take(10))
                    {
                        var distance = tree.Position.DistanceTo(pos);
                        score += 1 / Math.Pow(distance + 0.5, 4);
                    }

                    scores[pos] = score;
                }

                positions.Sort((a, b) => scores[b].CompareTo(scores[a]));
                var position = positions[0];

                var placements = GetPlacementPositions(bot, building, position, false).Take(20).ToList();
                
                if (placements.Count > 0)
                {
                    scores.Clear();

                    foreach (var placement in placements)
                    {
                        var score = 0d;

                        trees.Sort((a, b) => a.Position.DistanceTo(placement).CompareTo(b.Position.DistanceTo(placement)));
                        foreach (var tree in trees.Take(10))
                        {
                            var distance = GetBuildingFootprint(building, placement).Min(p => p.DistanceTo(tree.Position));
                            score += 1 / Math.Pow(distance + 0.5, 4);
                        }

                        scores[placement] = score;
                    }

                    placements.Sort((a, b) => scores[b].CompareTo(scores[a]));

                    var index = RNG.Next(5);
                    var place = placements[0];
                    Build(building, place, max, concurrent);
                }
            }
            else
            {
                var tiles = bot.GameState.GetTilesInRange(bot.GameState.MyPosition, Math.Max(5, MaxBuildRange - 10)).ToList();
                if (tiles.Count == 0)
                {
                    return;
                }

                var position = tiles[RNG.Next(tiles.Count)].Position;
                var placements = GetPlacementPositions(bot, building, position).Take(20).ToList();

                if (placements.Count > 0)
                {
                    var place = placements[RNG.Next(placements.Count)];
                    Build(building, place, max, concurrent);
                }
            }
        }

        public IEnumerable<Position> GetPlacementPositions(Bot bot, UnitDef building, Position position, bool restricted = true, int range = 10)
        {
            var tiles = bot.GameState.GetTilesInRange(position, range).ToList();
            tiles.Sort((a, b) => a.Position.DistanceTo(position).CompareTo(b.Position.DistanceTo(position)));

            var restrictions = new HashSet<Position>();
            foreach (var unit in bot.GameState.Units.Values.Where(u => u.Position.DistanceTo(position) < range + 10))
            {
                if (IsUnitClassObstruction(unit.Class))
                {
                    var def = bot.Mod.Villager;
                    if (bot.Mod.UnitDefs.ContainsKey(unit.TypeId))
                    {
                        def = bot.Mod.UnitDefs[unit.TypeId];
                    }

                    var margin = 0;
                    if (restricted)
                    {
                        margin = 1;
                    }

                    foreach (var pos in GetBuildingFootprint(def, unit.Position, margin))
                    {
                        restrictions.Add(pos);
                    }
                }
            }

            foreach (var tile in tiles)
            {
                if (CanBuildAtPosition(bot.GameState, building, tile.Position, restrictions))
                {
                    yield return tile.Position;
                }
            }
        }

        public IEnumerable<Position> GetBuildingFootprint(UnitDef building, Position position, int margin = 0)
        {
            var xmin = position.X - (building.Width / 2) - margin;
            var xmax = position.X + ((building.Width - 1) / 2) + margin;
            var ymin = position.Y - (building.Height / 2) - margin;
            var ymax = position.Y + ((building.Height - 1) / 2) + margin;

            for (int x = xmin; x <= xmax; x++)
            {
                for (int y = ymin; y <= ymax; y++)
                {
                    var pos = new Position(x, y);
                    yield return pos;
                }
            }
        }

        public bool IsUnitClassObstruction(UnitClass unit)
        {
            switch (unit)
            {
                case UnitClass.Artifact:
                case UnitClass.BerryBush:
                case UnitClass.Building:
                case UnitClass.Cliff:
                case UnitClass.DeepSeaFish:
                case UnitClass.Farm:
                case UnitClass.Flag:
                case UnitClass.Gate:
                case UnitClass.GoldMine:
                case UnitClass.MiscBuilding:
                case UnitClass.OceanFish:
                case UnitClass.OreMine:
                case UnitClass.ResourcePile:
                case UnitClass.SalvagePile:
                case UnitClass.ShoreFish:
                case UnitClass.StoneMine:
                case UnitClass.Tower:
                case UnitClass.Tree:
                case UnitClass.TreeStump:
                case UnitClass.Wall:
                    return true;
                default:
                    return false;
            }
        }

        private bool CanBuildAtPosition(GameState state, UnitDef building, Position position, HashSet<Position> restrictions)
        {
            if (!state.Tiles.ContainsKey(position))
            {
                return false;
            }

            var elevation = int.MinValue;

            foreach (var pos in GetBuildingFootprint(building, position))
            {
                if (!state.Tiles.ContainsKey(pos))
                {
                    return false;
                }

                var tile = state.Tiles[pos];

                if (!tile.Explored)
                {
                    return false;
                }

                if (elevation == int.MinValue)
                {
                    elevation = tile.Elevation;
                }

                if (tile.Elevation != elevation)
                {
                    return false;
                }

                if (restrictions.Contains(tile.Position))
                {
                    return false;
                }
            }

            return true;
        }

        internal override IEnumerable<Command> RequestUpdate(Bot bot)
        {
            var afford = true;
            foreach (var command in Commands)
            {
                command.Messages.Clear();
                command.Responses.Clear();

                if (command.CountTotal == -1)
                {
                    command.Messages.Add(new BuildingTypeCountTotal() { BuildingType = command.Building.Id });
                    command.Messages.Add(new UpPendingObjects() { TypeOp = (int)TypeOp.C, ObjectId = command.Building.Id });
                    command.Messages.Add(new CanAffordBuilding() { BuildingType = command.Building.Id });

                    command.Messages.Add(new SetGoal() { GoalId = 100, GoalValue = command.Position.X });
                    command.Messages.Add(new SetGoal() { GoalId = 101, GoalValue = command.Position.Y });
                    command.Messages.Add(new UpCanBuildLine() { TypeOp = (int)TypeOp.C, BuildingId = command.Building.FoundationId, EscrowState = 0, GoalPoint = 100 });
                }
                else if (afford && command.CountTotal < command.MaxCount && command.Pending < command.Concurrent && command.CanPlace)
                {
                    if (command.CanAfford)
                    {
                        command.Messages.Add(new SetGoal() { GoalId = 100, GoalValue = command.Position.X });
                        command.Messages.Add(new SetGoal() { GoalId = 101, GoalValue = command.Position.Y });
                        command.Messages.Add(new UpBuildLine() { TypeOp = (int)TypeOp.C, BuildingId = command.Building.FoundationId, GoalPoint1 = 100, GoalPoint2 = 100 });
                    }
                    else
                    {
                        afford = false;
                    }
                }

                if (command.Messages.Count > 0)
                {
                    yield return command;
                }
            }
        }

        internal override void Update(Bot bot)
        {
            foreach (var command in Commands.ToList())
            {
                Debug.Assert(command.Messages.Count == command.Responses.Count);

                if (command.CountTotal == -1)
                {
                    command.CountTotal = command.Responses[0].Unpack<BuildingTypeCountTotalResult>().Result;
                    command.Pending = command.Responses[1].Unpack<UpPendingObjectsResult>().Result;
                    command.CanAfford = command.Responses[2].Unpack<CanAffordBuildingResult>().Result;

                    var build = command.Responses[5].Unpack<UpCanBuildLineResult>().Result;

                    if (build)
                    {
                        command.CanPlace = true;
                    }

                    command.Messages.Clear();
                    command.Responses.Clear();
                }
                else
                {
                    Commands.Remove(command);
                }
            }
        }
    }
}
