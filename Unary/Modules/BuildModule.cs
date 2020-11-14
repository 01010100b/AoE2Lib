﻿using Protos.Expert.Action;
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
        public int MaxLumberRange { get; set; } = 30;
        public int MaxMillRange { get; set; } = 30;
        public int DropsiteSeparationDistance { get; set; } = 4;

        private readonly List<BuildCommand> Commands = new List<BuildCommand>();
        private readonly Random RNG = new Random(Guid.NewGuid().GetHashCode() ^ DateTime.UtcNow.GetHashCode());

        public void Build(UnitDef building, Position position, int max = int.MaxValue, int concurrent = int.MaxValue)
        {
            if (Commands.Select(c => c.Building.Id).Contains(building.Id))
            {
                return;
            }

            if (max <= 0 || concurrent <= 0)
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

            if (max <= 0 || concurrent <= 0)
            {
                return;
            }

            var place = new Position(-1, -1);

            if (building.BaseId == bot.Mod.LumberCamp.BaseId || building.BaseId == bot.Mod.Mill.BaseId)
            {
                var resources = new List<Unit>();
                var restricted = true;

                if (building.BaseId == bot.Mod.LumberCamp.BaseId)
                {
                    resources.AddRange(bot.GameState.GetUnitsInRange(bot.GameState.MyPosition, MaxLumberRange).Where(u => u.Class == UnitClass.Tree && u.Targetable));
                    restricted = false;
                }
                else if (building.BaseId == bot.Mod.Mill.BaseId)
                {
                    resources.AddRange(bot.GameState.GetUnitsInRange(bot.GameState.MyPosition, MaxMillRange).Where(u => u.Class == UnitClass.BerryBush && u.Targetable));
                }

                if (resources.Count > 0)
                {
                    place = GetResourcePlacement(bot, building, resources, restricted);
                }
                else
                {
                    place = GetNormalPlacement(bot, building, true);
                }
                
            }
            else if (building.BaseId == bot.Mod.Farm.BaseId)
            {
                place = GetFarmPlacement(bot, building);
            }
            else
            {
                place = GetNormalPlacement(bot, building, true);
            }

            if (bot.GameState.Tiles.ContainsKey(place))
            {
                Build(building, place, max, concurrent);
            }
            else
            {
                Log.Debug($"{bot.GameState.GameTime.TotalSeconds}:N2 Could not place {building.Id}");
            }
        }

        public Position GetResourcePlacement(Bot bot, UnitDef building, List<Unit> resources, bool restricted)
        {
            var place = new Position(-1, -1);

            if (resources.Count == 0)
            {
                return place;
            }

            var _resources = resources.ToList();
            while (_resources.Count > 50)
            {
                _resources.RemoveAt(RNG.Next(_resources.Count));
            }

            var costs = new Dictionary<Position, double>();

            foreach (var res in _resources)
            {
                if (!costs.ContainsKey(res.Position))
                {
                    costs[res.Position] = GetResourceCost(bot, building, res.Position, resources, 0, 0);
                }
            }

            _resources.Sort((a, b) => costs[a.Position].CompareTo(costs[b.Position]));

            var positions = new List<Position>();
            foreach (var res in _resources)
            {
                var d = double.MaxValue;
                foreach (var p in positions)
                {
                    if (p.DistanceTo(res.Position) < d)
                    {
                        d = p.DistanceTo(res.Position);
                    }
                }

                if (d > 1.5)
                {
                    positions.Add(res.Position);
                }
            }

            for (int i = 0; i < Math.Min(10, positions.Count); i++)
            {
                var position = positions[i];

                var placements = GetPlacementPositions(bot, building, position, restricted, 5).ToList();

                if (placements.Count > 0)
                {
                    foreach (var placement in placements)
                    {
                        if (!costs.ContainsKey(placement))
                        {
                            costs[placement] = GetResourceCost(bot, building, placement, _resources, 0, 0);
                        }
                    }

                    placements.Sort((a, b) => costs[a].CompareTo(costs[b]));
                    place = placements[RNG.Next(Math.Min(1, placements.Count))];

                    break;
                }
            }
            
            return place;
        }

        public Position GetFarmPlacement(Bot bot, UnitDef building)
        {
            var place = new Position(-1, -1);

            var mills = bot.GameState.Units.Values
                .Where(u => u.PlayerNumber == bot.GameState.PlayerNumber 
                    && (u.BaseTypeId == bot.Mod.TownCenter.BaseId || u.BaseTypeId == bot.Mod.Mill.BaseId))
                .ToList();

            if (mills.Count == 0)
            {
                return place;
            }

            for (int i = 0; i < 10; i++)
            {
                var mill = mills[RNG.Next(mills.Count)];
                var range = 4;
                if (mill.BaseTypeId == bot.Mod.TownCenter.BaseId)
                {
                    range += 1;
                }
                var placements = GetPlacementPositions(bot, building, mill.Position, false, range).Take(3).ToList();

                if (placements.Count > 0)
                {
                    place = placements[RNG.Next(placements.Count)];

                    break;
                }
            }

            return place;
        }

        public Position GetNormalPlacement(Bot bot, UnitDef building, bool restricted)
        {
            var place = new Position(-1, -1);

            var range = MaxBuildRange;
            if (building.BaseId == bot.Mod.Mill.BaseId)
            {
                range = MaxMillRange;
            }

            var tiles = bot.GameState.GetTilesInRange(bot.GameState.MyPosition, Math.Max(5, range)).ToList();
            if (tiles.Count == 0)
            {
                return place;
            }

            for (int i = 0; i < 10; i++)
            {
                var position = tiles[RNG.Next(tiles.Count)].Position;
                var placements = GetPlacementPositions(bot, building, position, restricted)
                    .Where(p => p.DistanceTo(bot.GameState.MyPosition) < range)
                    .Take(20).ToList();

                if (placements.Count > 0)
                {
                    place = placements[RNG.Next(placements.Count)];
                    break;
                }
            }

            return place;
        }

        public double GetResourceCost(Bot bot, UnitDef building, Position position, List<Unit> resources, double tc_factor, double center_factor)
        {
            var cost = 0d;

            resources.Sort((a, b) => a.Position.DistanceTo(position).CompareTo(b.Position.DistanceTo(position)));
            foreach (var res in resources.Take(10))
            {
                var distance = GetBuildingFootprint(building, position).Min(p => p.DistanceTo(res.Position));
                cost += Math.Pow(distance + 0.5, 2);
            }

            cost += Math.Pow(position.DistanceTo(bot.GameState.MyPosition) + 0.5, tc_factor);

            var center = new Position(bot.GameState.MapWidthHeight / 2, bot.GameState.MapWidthHeight / 2);
            var tc = bot.GameState.MyPosition - center;
            var pos = position - center;
            var d = (((tc.X * pos.X) + (tc.Y * pos.Y)) / tc.Norm()) - tc.Norm();

            cost -= Math.Pow(d + 0.5, center_factor);

            foreach (var unit in bot.GameState.GetUnitsInRange(position, 10)
                .Where(u => u.PlayerNumber == bot.GameState.PlayerNumber && u.BaseTypeId == building.BaseId))
            {
                if (position.DistanceTo(unit.Position) <= DropsiteSeparationDistance)
                {
                    cost += 1000000;
                }
            }

            return cost;
        }

        public IEnumerable<Position> GetPlacementPositions(Bot bot, UnitDef building, Position position, bool restricted = true, int range = 10)
        {
            var tiles = bot.GameState.GetTilesInRange(position, range).ToList();
            tiles.Sort((a, b) => a.Position.DistanceTo(position).CompareTo(b.Position.DistanceTo(position)));

            var restrictions = new HashSet<Position>();
            foreach (var unit in bot.GameState.Units.Values.Where(u => u.Position.DistanceTo(position) < range + 10))
            {
                if (IsUnitClassObstruction(unit.Class) && !(unit.PlayerNumber != 0 && unit.Targetable == false))
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

                        if (unit.BaseTypeId == bot.Mod.TownCenter.BaseId || unit.BaseTypeId == bot.Mod.Mill.BaseId)
                        {
                            margin = 3;
                        }
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

                if (command.CountTotal == -1 && bot.GameState.Tick % 2 == 0)
                {
                    command.Messages.Add(new BuildingTypeCountTotal() { BuildingType = command.Building.Id });
                    command.Messages.Add(new UpPendingObjects() { TypeOp = (int)TypeOp.C, ObjectId = command.Building.Id });
                    command.Messages.Add(new CanAffordBuilding() { BuildingType = command.Building.Id });

                    command.Messages.Add(new SetGoal() { GoalId = 100, GoalValue = command.Position.X });
                    command.Messages.Add(new SetGoal() { GoalId = 101, GoalValue = command.Position.Y });
                    command.Messages.Add(new UpCanBuildLine() { TypeOp = (int)TypeOp.C, BuildingId = command.Building.FoundationId, EscrowState = 0, GoalPoint = 100 });

                }
                else if (afford && command.CountTotal < command.MaxCount && command.Pending < command.Concurrent && command.CanPlace && bot.GameState.Tick % 2 == 1)
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
                    if (command.Responses.Count == 0)
                    {
                        continue;
                    }

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