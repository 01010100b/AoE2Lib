using AoE2Lib.Bots.GameElements;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoE2Lib.Bots
{
    public class GameState
    {
        public readonly Map Map;
        public Player MyPlayer => Players[Bot.PlayerNumber];
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
        private readonly Dictionary<StrategicNumber, int> StrategicNumbers = new Dictionary<StrategicNumber, int>();
        private readonly List<ProductionTask> ProductionTasks = new List<ProductionTask>();
        
        private readonly HashSet<Command> Commands = new HashSet<Command>();
        private readonly List<Command> FindCommands = new List<Command>();
        private readonly Command CommandInfo = new Command();

        public GameState(Bot bot)
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
                return null;
            }

            if (!Technologies.ContainsKey(id))
            {
                Technologies.Add(id, new Technology(Bot, id));
            }

            return Technologies[id];
        }

        public UnitType GetUnitType(int id)
        {
            if (id < 0)
            {
                return null;
            }

            if (!UnitTypes.ContainsKey(id))
            {
                UnitTypes.Add(id, new UnitType(Bot, id));
            }

            return UnitTypes[id];
        }

        public Unit GetUnit(int id)
        {
            if (Units.TryGetValue(id, out Unit unit))
            {
                return unit;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }
        }

        public IEnumerable<Unit> GetAllUnits()
        {
            return Units.Values;
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

        public int GetStrategicNumber(StrategicNumber sn)
        {
            if (StrategicNumbers.TryGetValue(sn, out int val))
            {
                return val;
            }
            else
            {
                return -1;
            }
        }

        public void SetStrategicNumber(StrategicNumber sn, int val)
        {
            StrategicNumbers[sn] = val;
        }

        public void FindUnits(int player, Position position, int range)
        {
            var command = new Command();

            command.Add(new SetGoal() { InConstGoalId = 100, InConstValue = position.PointX });
            command.Add(new SetGoal() { InConstGoalId = 101, InConstValue = position.PointY });
            command.Add(new UpSetTargetPoint() { InGoalPoint = 100 });
            command.Add(new SetStrategicNumber() { InConstSnId = (int)StrategicNumber.FOCUS_PLAYER_NUMBER, InConstValue = player });
            command.Add(new UpFullResetSearch());

            command.Add(new UpFilterDistance() { InConstMinDistance = -1, InConstMaxDistance = range });

            for (int i = 0; i < 10; i++)
            {
                command.Add(new UpResetSearch() { InConstLocalIndex = 0, InConstLocalList = 0, InConstRemoteIndex = 0, InConstRemoteList = 1 });
                command.Add(new UpFindRemote() { InConstUnitId = -1, InConstCount = 40 });
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
                var command = new Command();

                command.Add(new SetGoal() { InConstGoalId = 100, InConstValue = position.PointX });
                command.Add(new SetGoal() { InConstGoalId = 101, InConstValue = position.PointY });
                command.Add(new UpSetTargetPoint() { InGoalPoint = 100 });
                command.Add(new SetStrategicNumber() { InConstSnId = (int)StrategicNumber.FOCUS_PLAYER_NUMBER, InConstValue = player });
                command.Add(new UpFullResetSearch());

                command.Add(new UpFilterDistance() { InConstMinDistance = -1, InConstMaxDistance = range });
                command.Add(new UpFilterStatus() { InConstObjectStatus = status, InConstObjectList = 0 });

                for (int i = 0; i < 10; i++)
                {
                    command.Add(new UpResetSearch() { InConstLocalIndex = 0, InConstLocalList = 0, InConstRemoteIndex = 0, InConstRemoteList = 1 });
                    command.Add(new UpFindResource() { InConstResource = (int)resource, InConstCount = 40 });
                    command.Add(new UpSearchObjectIdList() { InConstSearchSource = 2 });
                }

                FindCommands.Add(command);
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

        internal void FindUnitsOld(int player, Position position, int range)
        {
            foreach (var cmdid in (IEnumerable<CmdId>)Enum.GetValues(typeof(CmdId)))
            {
                var command = new Command();

                command.Add(new SetGoal() { InConstGoalId = 100, InConstValue = position.PointX });
                command.Add(new SetGoal() { InConstGoalId = 101, InConstValue = position.PointY });
                command.Add(new UpSetTargetPoint() { InGoalPoint = 100 });
                command.Add(new SetStrategicNumber() { InConstSnId = (int)StrategicNumber.FOCUS_PLAYER_NUMBER, InConstValue = player });
                command.Add(new UpFullResetSearch());

                command.Add(new UpFilterInclude() { InConstCmdId = (int)cmdid, InConstActionId = -1, InConstOrderId = -1, InConstOnMainland = -1 });
                command.Add(new UpFilterDistance() { InConstMinDistance = -1, InConstMaxDistance = range });
                
                for (int i = 0; i < 10; i++)
                {
                    command.Add(new UpResetSearch() { InConstLocalIndex = 0, InConstLocalList = 0, InConstRemoteIndex = 0, InConstRemoteList = 1 });
                    command.Add(new UpFindRemote() { InConstUnitId = -1, InConstCount = 40 });
                    command.Add(new UpSearchObjectIdList() { InConstSearchSource = 2 });
                }

                FindCommands.Add(command);

                if (cmdid == CmdId.CIVILIAN_BUILDING || cmdid == CmdId.MILITARY_BUILDING)
                {
                    command = new Command();

                    command.Add(new SetGoal() { InConstGoalId = 100, InConstValue = position.PointX });
                    command.Add(new SetGoal() { InConstGoalId = 101, InConstValue = position.PointY });
                    command.Add(new UpSetTargetPoint() { InGoalPoint = 100 });
                    command.Add(new SetStrategicNumber() { InConstSnId = (int)StrategicNumber.FOCUS_PLAYER_NUMBER, InConstValue = player });
                    command.Add(new UpFullResetSearch());

                    command.Add(new UpFilterStatus() { InConstObjectStatus = 0, InConstObjectList = 0 });
                    command.Add(new UpFilterInclude() { InConstCmdId = (int)cmdid, InConstActionId = -1, InConstOrderId = -1, InConstOnMainland = -1 });
                    command.Add(new UpFilterDistance() { InConstMinDistance = -1, InConstMaxDistance = range });
                    
                    for (int i = 0; i < 10; i++)
                    {
                        command.Add(new UpResetSearch() { InConstLocalIndex = 0, InConstLocalList = 0, InConstRemoteIndex = 0, InConstRemoteList = 1 });
                        command.Add(new UpFindStatusRemote() { InConstUnitId = -1, InConstCount = 40 });
                        command.Add(new UpSearchObjectIdList() { InConstSearchSource = 2 });
                    }

                    FindCommands.Add(command);
                }
            }
            /*
            foreach (var resource in (IEnumerable<Resource>)Enum.GetValues(typeof(Resource)))
            {
                foreach (var status in new[] {2, 3})
                {
                    var command = new Command();

                    command.Add(new SetGoal() { InConstGoalId = 100, InConstValue = position.PointX });
                    command.Add(new SetGoal() { InConstGoalId = 101, InConstValue = position.PointY });
                    command.Add(new UpSetTargetPoint() { InGoalPoint = 100 });
                    command.Add(new SetStrategicNumber() { InConstSnId = (int)StrategicNumber.FOCUS_PLAYER_NUMBER, InConstValue = player });
                    command.Add(new UpFullResetSearch());

                    command.Add(new UpFilterDistance() { InConstMinDistance = -1, InConstMaxDistance = range });
                    command.Add(new UpFilterStatus() { InConstObjectStatus = status, InConstObjectList = 0 });

                    for (int i = 0; i < 10; i++)
                    {
                        command.Add(new UpResetSearch() { InConstLocalIndex = 0, InConstLocalList = 0, InConstRemoteIndex = 0, InConstRemoteList = 1 });
                        command.Add(new UpFindResource() { InConstResource = (int)resource, InConstCount = 40 });
                        command.Add(new UpSearchObjectIdList() { InConstSearchSource = 2 });
                    }

                    FindCommands.Add(command);
                }
            }
            */
        }

        internal IEnumerable<Command> RequestUpdate()
        {
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
                CommandInfo.Add(new SetStrategicNumber() { InConstSnId = (int)sn.Key, InConstValue = sn.Value });
            }

            foreach (var sn in Enum.GetValues(typeof(StrategicNumber)).Cast<StrategicNumber>())
            {
                CommandInfo.Add(new Protos.Expert.Fact.StrategicNumber() { InConstSnId = (int)sn });
            }

            yield return CommandInfo;

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

            foreach (var command in FindCommands)
            {
                yield return command;
            }

            foreach (var command in DoProduction())
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

            foreach (var unit in Units.Values)
            {
                unit.Update();
            }

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
                if (x >= 0 && y >= 0 && x < Bot.GameState.Map.Width && y < Bot.GameState.Map.Height)
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

                foreach (var sn in Enum.GetValues(typeof(StrategicNumber)).Cast<StrategicNumber>())
                {
                    index++;
                    StrategicNumbers[sn] = responses[index].Unpack<StrategicNumberResult>().Result;
                }

                Tick++;
            }

            foreach (var player in Players)
            {
                player.Units.Clear();
            }

            foreach (var tile in Map.GetTiles())
            {
                tile.Units.Clear();
            }

            foreach (var unit in GetAllUnits().Where(u => u.Updated && u.PlayerNumber >= 0 && u.Targetable))
            {
                Players[unit[ObjectData.PLAYER]].Units.Add(unit);

                try
                {
                    var tile = Map.GetTile(unit.Position.PointX, unit.Position.PointY);
                    tile.Units.Add(unit);
                }
                catch (Exception)
                {
                    continue;
                }
            }

            Bot.Log.Info($"Tick {Tick} Game time {GameTime:g} with {GameTimePerTick:c} game time per tick");

            foreach (var player in Players.Where(p => p.IsValid && p.Updated))
            {
                Bot.Log.Debug($"Player {player.PlayerNumber} has {player.Units.Count} units and {player.Score} score");
            }
        }

        private IEnumerable<Command> DoProduction()
        {
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
        }

        private void DoAutoFindUnits()
        {
            if (AutoFindUnits == false)
            {
                return;
            }

            if (Map.Width <= 0 || Map.Height <= 0)
            {
                return;
            }

            Bot.Log.Debug($"Auto finding units");

            var position = MyPosition;
            for (int i = 0; i < 100; i++)
            {
                var x = Bot.Rng.Next(Map.Width);
                var y = Bot.Rng.Next(Map.Height);
                var tile = Map.GetTile(x, y);

                if (tile.Explored)
                {
                    position = tile.Position;
                }
            }

            foreach (var player in Players)
            {
                var range = Map.Width + Map.Height;
                if (Tick % 10 == 9)
                {
                    range = 20;
                }

                FindUnits(player.PlayerNumber, position, range);

                if (player.PlayerNumber == 0)
                {
                    range = Map.Width + Map.Height;

                    var resource = Resource.NONE;

                    if (Tick % 2 == 0)
                    {
                        resource = Resource.WOOD;
                    }
                    else if (Tick % 3 == 0)
                    {
                        resource = Resource.FOOD;
                    }
                    else if (Tick % 5 == 0)
                    {
                        resource = Resource.BOAR;
                    }
                    else if (Tick % 7 == 0)
                    {
                        resource = Resource.DEER;
                    }
                    else if (Tick % 11 == 0)
                    {
                        resource = Resource.GOLD;
                    }
                    else
                    {
                        resource = Resource.STONE;
                    }

                    if (resource == Resource.WOOD)
                    {
                        if (Bot.Rng.NextDouble() < 0.9)
                        {
                            range = 10;
                        }
                    }
                    else
                    {
                        if (Bot.Rng.NextDouble() < 0.1)
                        {
                            range = 20;
                        }
                    }

                    FindResources(resource, 0, position, range);
                }
            }

            /*
            var players = Bot.GameState.GetPlayers().ToList();

            for (int i = 0; i < AutoFindUnits; i++)
            {
                if (players.Count == 0)
                {
                    break;
                }

                var player = players[Bot.Rng.Next(players.Count)];
                players.Remove(player);

                var pos = MyPosition;
                for (int j = 0; j < 100; j++)
                {
                    var x = Bot.Rng.Next(Map.Width);
                    var y = Bot.Rng.Next(Map.Height);
                    var tile = Map.GetTile(x, y);

                    if (tile.Explored)
                    {
                        pos = Position.FromPoint(x, y);
                    }
                }

                var range = Map.Width + Map.Height;
                if (player.PlayerNumber == 0)
                {
                    if (Bot.Rng.NextDouble() < 0.8)
                    {
                        range = 10;
                    }
                }
                else
                {
                    if (Bot.Rng.NextDouble() < 0.1)
                    {
                        range = 10;
                    }
                }

                player.FindUnits(pos, range);
            }
            */
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
                player.Units.Sort((a, b) =>
                {
                    if (b.Updated && !a.Updated)
                    {
                        return -1;
                    }
                    else if (a.Updated && !b.Updated)
                    {
                        return 1;
                    }
                    else
                    {
                        return a.LastUpdateTick.CompareTo(b.LastUpdateTick);
                    }
                });

                var count = Math.Min(player.Units.Count, AutoUpdateUnits);

                for (int i = 0; i < count; i++)
                {
                    player.Units[i].RequestUpdate();
                }

                Bot.Log.Debug($"Auto updating {count} units for player {player.PlayerNumber}");
            }
            /*
            var units = Units.Values.ToList();
            units.Sort((a, b) =>
            {
                if (b.Updated && !a.Updated)
                {
                    return -1;
                }
                else if (a.Updated && !b.Updated)
                {
                    return 1;
                }
                else
                {
                    return a.LastUpdateTick.CompareTo(b.LastUpdateTick);
                }
            });

            var count = Math.Max(1, AutoUpdateUnits / 2);
            count = Math.Min(units.Count, count);

            for (int i = 0; i < count; i++)
            {
                units[i].RequestUpdate();
                units[Bot.Rng.Next(units.Count)].RequestUpdate();
            }
            */
        }
    }
}
