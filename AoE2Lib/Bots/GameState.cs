using AoE2Lib.Bots.GameElements;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AoE2Lib.Bots
{
    public class GameState
    {
        public readonly Map Map;
        public Player MyPlayer => Players[Bot.PlayerNumber];
        public Player Gaia => Players[0];
        public IEnumerable<Player> Enemies => GetPlayers().Where(p => p.Stance == PlayerStance.ENEMY && p != Gaia);
        public IEnumerable<Player> Allies => GetPlayers().Where(p => p.Stance == PlayerStance.ALLY && p != MyPlayer);
        public Position MyPosition { get; private set; } = Position.Zero;
        public int Tick { get; private set; } = 0;
        public TimeSpan GameTime { get; private set; } = TimeSpan.Zero;
        public TimeSpan GameTimePerTick { get; private set; } = TimeSpan.FromSeconds(0.7);

        public bool AutoFindUnits { get; set; } = true;
        public int AutoUpdateUnits { get; set; } = 100; // units to update per tick per player

        private readonly Bot Bot;
        private readonly List<Player> Players = new List<Player>();
        private readonly Dictionary<int, Technology> Technologies = new Dictionary<int, Technology>();
        private readonly Dictionary<int, UnitType> UnitTypes = new Dictionary<int, UnitType>();
        private readonly Dictionary<int, Unit> Units = new Dictionary<int, Unit>();
        private readonly Dictionary<Resource, bool> ResourceFound = new Dictionary<Resource, bool>();
        private readonly Dictionary<Resource, int> DropsiteMinDistance = new Dictionary<Resource, int>();
        private readonly Dictionary<int, int> StrategicNumbers = new Dictionary<int, int>();
        private readonly List<ProductionTask> ProductionTasks = new List<ProductionTask>();
        
        private readonly HashSet<Command> Commands = new HashSet<Command>();
        private readonly List<Command> FindCommands = new List<Command>();
        private readonly Command CommandInfo = new Command();

        internal GameState(Bot bot)
        {
            Bot = bot;
            Map = new Map(Bot);

            for (int i = 0; i <= 8; i++)
            {
                Players.Add(new Player(Bot, i));
            }
        }

        public Player GetPlayer(int player_number)
        {
            return Players[player_number];
        }

        public IEnumerable<Player> GetPlayers()
        {
            return Players.Where(p => p.IsValid);
        }

        public Technology GetTechnology(int id)
        {
            if (id < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            if (!Technologies.ContainsKey(id))
            {
                Technologies.Add(id, new Technology(Bot, id));
                Bot.Log.Info($"Added technology {id}");
            }

            return Technologies[id];
        }

        public UnitType GetUnitType(int id)
        {
            if (id < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            if (!UnitTypes.ContainsKey(id))
            {
                UnitTypes.Add(id, new UnitType(Bot, id));
                Bot.Log.Info($"Added unit type {id}");
            }

            return UnitTypes[id];
        }

        public bool TryGetUnit(int id, out Unit unit)
        {
            if (Units.TryGetValue(id, out Unit u))
            {
                unit = u;

                return true;
            }
            else
            {
                unit = null;

                return false;
            }
        }

        public bool GetResourceFound(Resource resource)
        {
            if (ResourceFound.TryGetValue(resource, out bool found))
            {
                return found;
            }
            else
            {
                return false;
            }
        }

        public int GetDropsiteMinDistance(Resource resource)
        {
            if (DropsiteMinDistance.TryGetValue(resource, out int d))
            {
                return d;
            }
            else
            {
                return -1;
            }
        }

        public int GetStrategicNumber(int sn)
        {
            if (sn < 0 || sn > 511)
            {
                throw new ArgumentOutOfRangeException(nameof(sn));
            }

            if (StrategicNumbers.TryGetValue(sn, out int val))
            {
                return val;
            }
            else
            {
                return -1;
            }
        }

        public int GetStrategicNumber(StrategicNumber sn)
        {
            return GetStrategicNumber((int)sn);
        }

        public void SetStrategicNumber(int sn, int val)
        {
            if (sn < 0 || sn > 511)
            {
                throw new ArgumentOutOfRangeException(nameof(sn));
            }

            StrategicNumbers[sn] = val;
        }

        public void SetStrategicNumber(StrategicNumber sn, int val)
        {
            SetStrategicNumber((int)sn, val);
        }

        public void FindUnits(int player, Position position, int range)
        {
            var command = new Command();

            command.Add(new SetGoal() { InConstGoalId = 100, InConstValue = position.PointX });
            command.Add(new SetGoal() { InConstGoalId = 101, InConstValue = position.PointY });
            command.Add(new UpSetTargetPoint() { InGoalPoint = 100 });
            command.Add(new SetStrategicNumber() { InConstSnId = (int)StrategicNumber.FOCUS_PLAYER_NUMBER, InConstValue = player });
            command.Add(new UpFullResetSearch());

            command.Add(new UpFilterStatus() { InConstObjectStatus = 2, InConstObjectList = Bot.Rng.NextDouble() < 0.9 ? 0 : 1 });
            command.Add(new UpFilterDistance() { InConstMinDistance = -1, InConstMaxDistance = range });

            for (int i = 0; i < 10; i++)
            {
                command.Add(new UpResetSearch() { InConstLocalIndex = 0, InConstLocalList = 0, InConstRemoteIndex = 0, InConstRemoteList = 1 });
                command.Add(new UpFindStatusRemote() { InConstUnitId = -1, InConstCount = 40 });
                command.Add(new UpSearchObjectIdList() { InConstSearchSource = 2 });
            }

            FindCommands.Add(command);

            command = new Command();

            command.Add(new SetGoal() { InConstGoalId = 100, InConstValue = position.PointX });
            command.Add(new SetGoal() { InConstGoalId = 101, InConstValue = position.PointY });
            command.Add(new UpSetTargetPoint() { InGoalPoint = 100 });
            command.Add(new SetStrategicNumber() { InConstSnId = (int)StrategicNumber.FOCUS_PLAYER_NUMBER, InConstValue = player });
            command.Add(new UpFullResetSearch());

            command.Add(new UpFilterStatus() { InConstObjectStatus = 0, InConstObjectList = 0 });
            command.Add(new UpFilterDistance() { InConstMinDistance = -1, InConstMaxDistance = range });

            for (int i = 0; i < 10; i++)
            {
                command.Add(new UpResetSearch() { InConstLocalIndex = 0, InConstLocalList = 0, InConstRemoteIndex = 0, InConstRemoteList = 1 });
                command.Add(new UpFindStatusRemote() { InConstUnitId = -1, InConstCount = 40 });
                command.Add(new UpSearchObjectIdList() { InConstSearchSource = 2 });
            }

            FindCommands.Add(command);
        }

        public void FindResources(Resource resource, int player, Position position, int range)
        {
            foreach (var status in new[] { 2, 3 })
            {
                foreach (var list in new[] { 0, 1 })
                {
                    var command = new Command();

                    command.Add(new SetGoal() { InConstGoalId = 100, InConstValue = position.PointX });
                    command.Add(new SetGoal() { InConstGoalId = 101, InConstValue = position.PointY });
                    command.Add(new UpSetTargetPoint() { InGoalPoint = 100 });
                    command.Add(new SetStrategicNumber() { InConstSnId = (int)StrategicNumber.FOCUS_PLAYER_NUMBER, InConstValue = player });
                    command.Add(new UpFullResetSearch());

                    command.Add(new UpFilterDistance() { InConstMinDistance = -1, InConstMaxDistance = range });
                    command.Add(new UpFilterStatus() { InConstObjectStatus = status, InConstObjectList = list });

                    for (int i = 0; i < 10; i++)
                    {
                        command.Add(new UpResetSearch() { InConstLocalIndex = 0, InConstLocalList = 0, InConstRemoteIndex = 0, InConstRemoteList = 1 });
                        command.Add(new UpFindResource() { InConstResource = (int)resource, InConstCount = 40 });
                        command.Add(new UpSearchObjectIdList() { InConstSearchSource = 2 });
                    }

                    FindCommands.Add(command);
                }
            }
        }

        internal void AddProductionTask(ProductionTask task)
        {
            ProductionTasks.Add(task);
        }

        internal void AddCommand(Command command)
        {
            Commands.Add(command);
        }

        internal void RemoveUnit(Unit unit)
        {
            Units.Remove(unit.Id);
        }

        internal IEnumerable<Command> RequestUpdate()
        {
            var sw = new Stopwatch();

            DoAutoFindUnits();
            DoAutoUpdateUnits();

            CommandInfo.Reset();

            CommandInfo.Add(new GameTime());
            CommandInfo.Add(new UpGetPoint() { OutGoalPoint = 50, InConstPositionType = (int)PositionType.SELF });
            CommandInfo.Add(new Goal() { InConstGoalId = 50 });
            CommandInfo.Add(new Goal() { InConstGoalId = 51 });
            
            foreach (var resource in new[] { Resource.FOOD, Resource.WOOD, Resource.GOLD, Resource.STONE })
            {
                CommandInfo.Add(new ResourceFound() { InConstResource = (int)resource });
            }

            foreach (var resource in Enum.GetValues(typeof(Resource)).Cast<Resource>())
            {
                CommandInfo.Add(new DropsiteMinDistance() { InConstResource = (int)resource });
            }

            foreach (var sn in StrategicNumbers)
            {
                CommandInfo.Add(new SetStrategicNumber() { InConstSnId = sn.Key, InConstValue = sn.Value });
            }

            for (int sn = 0; sn < 512; sn++)
            {
                CommandInfo.Add(new Protos.Expert.Fact.StrategicNumber() { InConstSnId = sn });
            }

            yield return CommandInfo;

            foreach (var command in DoProduction())
            {
                yield return command;
            }

            sw.Restart();

            Map.RequestUpdate();

            foreach (var player in Players)
            {
                player.RequestUpdate();
            }

            foreach (var tech in Technologies.Values)
            {
                tech.RequestUpdate();
            }

            foreach (var type in UnitTypes.Values)
            {
                type.RequestUpdate();
            }

            sw.Stop();
            Bot.Log.Info($"GameElement RequestUpdate took {sw.ElapsedMilliseconds} ms");

            foreach (var command in FindCommands)
            {
                yield return command;
            }

            foreach (var command in Commands)
            {
                yield return command;
            }

            Commands.Clear();
        }

        internal void Update()
        {
            Bot.Log.Info("");
            Bot.Log.Info($"Tick {Tick} Game time {GameTime:g} with {GameTimePerTick:c} game time per tick");
            foreach (var player in Players.Where(p => p.IsValid && p.Updated))
            {
                Bot.Log.Debug($"Player {player.PlayerNumber} has {player.Units.Count(u => u.Targetable)} units and {player.Score} score");
            }

            var sw = new Stopwatch();

            if (CommandInfo.HasResponses)
            {
                var responses = CommandInfo.GetResponses();

                var current_time = GameTime;
                GameTime = TimeSpan.FromSeconds(responses[0].Unpack<GameTimeResult>().Result);
                GameTimePerTick *= 49;
                GameTimePerTick += GameTime - current_time;
                GameTimePerTick /= 50;

                var x = responses[2].Unpack<GoalResult>().Result;
                var y = responses[3].Unpack<GoalResult>().Result;
                if (Map.IsOnMap(x, y))
                {
                    MyPosition = Position.FromPoint(x, y);
                }

                var index = 3;
                foreach (var resource in new[] { Resource.FOOD, Resource.WOOD, Resource.GOLD, Resource.STONE })
                {
                    index++;
                    ResourceFound[resource] = responses[index].Unpack<ResourceFoundResult>().Result;
                }

                foreach (var resource in Enum.GetValues(typeof(Resource)).Cast<Resource>())
                {
                    index++;
                    DropsiteMinDistance[resource] = responses[index].Unpack<DropsiteMinDistanceResult>().Result;
                }

                foreach (var sn in StrategicNumbers)
                {
                    index++;
                }

                for (int sn = 0; sn < 512; sn++)
                {
                    index++;
                    StrategicNumbers[sn] = responses[index].Unpack<StrategicNumberResult>().Result;
                }

                Tick++;
            }

            sw.Restart();

            Map.Update();

            foreach (var player in Players)
            {
                player.Update();
            }

            foreach (var technology in Technologies.Values)
            {
                technology.Update();
            }

            foreach (var type in UnitTypes.Values)
            {
                type.Update();
            }

            foreach (var unit in Units.Values.ToList())
            {
                unit.Update();
            }

            sw.Stop();
            Bot.Log.Info($"GameElement Update took {sw.ElapsedMilliseconds} ms");

            sw.Restart();

            foreach (var command in FindCommands.Where(c => c.HasResponses))
            {
                var responses = command.Responses;

                for (int i = 0; i < 10; i++)
                {
                    var index = responses.Count - 1 - (i * 3);

                    var ids = responses[index].Unpack<UpSearchObjectIdListResult>().Result;
                    foreach (var id in ids.Where(d => d >= 0))
                    {
                        if (!Units.ContainsKey(id))
                        {
                            Units.Add(id, new Unit(Bot, id));
                        }
                    }
                }
            }

            FindCommands.Clear();

            foreach (var player in Players)
            {
                player.Units.Clear();
            }

            foreach (var tile in Map.GetTiles())
            {
                tile.Units.Clear();
            }

            foreach (var unit in Units.Values.Where(u => u.Updated))
            {
                if (unit.PlayerNumber >= 0)
                {
                    Players[unit.PlayerNumber].Units.Add(unit);
                }

                if (Map.IsOnMap(unit.Position))
                {
                    var tile = Map.GetTile(unit.Position);
                    tile.Units.Add(unit);
                }
            }

            sw.Stop();
            Bot.Log.Info($"GameState Update took {sw.ElapsedMilliseconds} ms");
        }

        private IEnumerable<Command> DoProduction()
        {
            var sw = new Stopwatch();
            sw.Start();

            var remaining_wood = MyPlayer.GetFact(FactId.WOOD_AMOUNT);
            var remaining_food = MyPlayer.GetFact(FactId.FOOD_AMOUNT);
            var remaining_gold = MyPlayer.GetFact(FactId.GOLD_AMOUNT);
            var remaining_stone = MyPlayer.GetFact(FactId.STONE_AMOUNT);

            ProductionTasks.Sort((a, b) => b.Priority.CompareTo(a.Priority));

            foreach (var prod in ProductionTasks)
            {
                Bot.Log.Debug($"Checking production: {prod.Id}");

                var can_afford = true;
                if (prod.WoodCost > 0 && prod.WoodCost > remaining_wood)
                {
                    can_afford = false;
                }
                else if (prod.FoodCost > 0 && prod.FoodCost > remaining_food)
                {
                    can_afford = false;
                }
                else if (prod.GoldCost > 0 && prod.GoldCost > remaining_gold)
                {
                    can_afford = false;
                }
                else if (prod.StoneCost > 0 && prod.StoneCost > remaining_stone)
                {
                    can_afford = false;
                }

                var deduct = true;
                if (can_afford == false && prod.Blocking == false)
                {
                    deduct = false;
                }

                if (can_afford)
                {
                    Bot.Log.Debug($"Production: {prod.Id}");

                    yield return prod.GetCommand();
                }

                if (deduct)
                {
                    remaining_wood -= prod.WoodCost;
                    remaining_food -= prod.FoodCost;
                    remaining_gold -= prod.GoldCost;
                    remaining_stone -= prod.StoneCost;
                }
            }

            ProductionTasks.Clear();

            sw.Stop();
            Bot.Log.Info($"DoProduction took {sw.ElapsedMilliseconds} ms");
        }

        private void DoAutoFindUnits()
        {
            if (AutoFindUnits == false)
            {
                return;
            }

            if (!Map.Updated)
            {
                return;
            }

            var sw = new Stopwatch();
            sw.Start();

            Bot.Log.Debug($"Auto finding units");

            var position = MyPosition;
            for (int i = 0; i < 1000; i++)
            {
                var x = Bot.Rng.Next(Map.Width);
                var y = Bot.Rng.Next(Map.Height);
                var tile = Map.GetTile(x, y);

                if (tile.Explored)
                {
                    position = tile.Position;
                }
            }

            foreach (var player in GetPlayers())
            {
                var range = Map.Width + Map.Height;
                if (Tick > 100 && Tick % 10 == 0)
                {
                    range = 20;
                }

                FindUnits(player.PlayerNumber, position, range);

                if (player.PlayerNumber == 0)
                {
                    range = Map.Width + Map.Height;

                    var resource = Resource.NONE;

                    if (Tick % 32 == 0)
                    {
                        resource = Resource.STONE;
                    }
                    else if (Tick % 16 == 0)
                    {
                        resource = Resource.GOLD;
                    }
                    else if (Tick % 8 == 0)
                    {
                        resource = Resource.DEER;
                    }
                    else if (Tick % 4 == 0)
                    {
                        resource = Resource.BOAR;
                    }
                    else if (Tick % 2 == 0)
                    {
                        resource = Resource.FOOD;
                    }
                    else
                    {
                        resource = Resource.WOOD;
                    }

                    if (Tick > 100 && Bot.Rng.NextDouble() < 0.5)
                    {
                        range = 10;
                    }
                    
                    FindResources(resource, 0, position, range);
                }
            }

            sw.Stop();
            Bot.Log.Info($"DoAutoFindUnits took {sw.ElapsedMilliseconds} ms");
        }

        private void DoAutoUpdateUnits()
        {
            if (AutoUpdateUnits <= 0)
            {
                return;
            }

            if (Units.Count == 0)
            {
                return;
            }

            var sw = new Stopwatch();
            sw.Start();

            var first = 0;
            foreach (var unit in Units.Values)
            {
                if (unit.Updated == false)
                {
                    unit.RequestUpdate();
                    first++;
                }

                if (first >= AutoUpdateUnits)
                {
                    break;
                }
            }

            Bot.Log.Debug($"Auto updating {first} first units");

            foreach (var player in GetPlayers())
            {
                player.Units.Sort((a, b) => a.LastUpdateTick.CompareTo(b.LastUpdateTick));
                var count = Math.Min(player.Units.Count, AutoUpdateUnits);

                for (int i = 0; i < count; i++)
                {
                    player.Units[i].RequestUpdate();
                }

                Bot.Log.Debug($"Auto updating {count} units for player {player.PlayerNumber}");
            }

            sw.Stop();
            Bot.Log.Info($"DoAutoUpdateUnits took {sw.ElapsedMilliseconds} ms");
        }
    }
}
