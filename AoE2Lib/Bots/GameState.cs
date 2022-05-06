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
        public IEnumerable<Player> CurrentEnemies => GetPlayers().Where(p => p.IsEnemy && p.InGame);
        public IEnumerable<Player> CurrentAllies => GetPlayers().Where(p => p.IsAlly && p.InGame);
        public Position MyPosition { get; private set; } = Position.Zero;
        public int Tick { get; private set; } = 0;
        public TimeSpan GameTime { get; private set; } = TimeSpan.Zero;
        public TimeSpan GameTimePerTick { get; private set; } = TimeSpan.FromSeconds(0.7);

        private readonly Bot Bot;
        private readonly List<Player> Players = new();
        private readonly Dictionary<int, Technology> Technologies = new();
        private readonly Dictionary<int, UnitType> UnitTypes = new();
        private readonly Dictionary<int, Unit> Units = new();
        private readonly Dictionary<int, int> StrategicNumbers = new();
        
        private readonly List<Command> Commands = new();
        private readonly List<Command> UnitFindCommands = new();
        private readonly Command CommandInfo = new();

        internal GameState(Bot bot)
        {
            Bot = bot;
            Map = new Map(Bot);

            for (int i = 0; i <= 8; i++)
            {
                Players.Add(new Player(Bot, i));
            }
        }

        public IEnumerable<Player> GetPlayers()
        {
            return Players.Where(p => p.IsValid && p.Updated);
        }

        public bool TryGetPlayer(int player_number, out Player player)
        {
            if (player_number < 0 || player_number >= Players.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(player_number));
            }

            var p = Players[player_number];

            if (p.IsValid && p.Updated)
            {
                player = p;

                return true;
            }
            else
            {
                player = default;

                return false;
            }
        }

        public bool TryGetTechnology(int id, out Technology technology)
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

            var t = Technologies[id];

            if (t.Updated)
            {
                technology = t;

                return true;
            }
            else
            {
                technology = default;

                return false;
            }
        }

        public bool TryGetUnitType(int id, out UnitType type)
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

            var t = UnitTypes[id];

            if (t.Updated)
            {
                type = t;

                return true;
            }
            else
            {
                type = default;

                return false;
            }
        }

        public bool TryGetUnit(int id, out Unit unit)
        {
            if (Units.TryGetValue(id, out Unit u))
            {
                if (u.Updated)
                {
                    unit = u;

                    return true;
                }
            }

            unit = default;

            return false;
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
            const int GL_X = Bot.GOAL_START;
            const int GL_Y = Bot.GOAL_START + 1;

            var command = new Command();

            command.Add(new SetGoal() { InConstGoalId = GL_X, InConstValue = position.PointX });
            command.Add(new SetGoal() { InConstGoalId = GL_Y, InConstValue = position.PointY });
            command.Add(new UpSetTargetPoint() { InGoalPoint = GL_X });
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

            UnitFindCommands.Add(command);

            command = new Command();

            command.Add(new SetGoal() { InConstGoalId = GL_X, InConstValue = position.PointX });
            command.Add(new SetGoal() { InConstGoalId = GL_Y, InConstValue = position.PointY });
            command.Add(new UpSetTargetPoint() { InGoalPoint = GL_X });
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

            UnitFindCommands.Add(command);
        }

        public void FindResources(Resource resource, int player, Position position, int range)
        {
            const int GL_X = Bot.GOAL_START;
            const int GL_Y = Bot.GOAL_START + 1;

            foreach (var status in new[] { 2, 3 })
            {
                foreach (var list in new[] { 0, 1 })
                {
                    var command = new Command();

                    command.Add(new SetGoal() { InConstGoalId = GL_X, InConstValue = position.PointX });
                    command.Add(new SetGoal() { InConstGoalId = GL_Y, InConstValue = position.PointY });
                    command.Add(new UpSetTargetPoint() { InGoalPoint = GL_X });
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

                    UnitFindCommands.Add(command);
                }
            }
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
            const int GL_X = Bot.GOAL_START;
            const int GL_Y = Bot.GOAL_START + 1;

            var sw = new Stopwatch();
            sw.Start();

            DoAutoFindUnits();
            DoAutoUpdateUnits();

            CommandInfo.Reset();

            CommandInfo.Add(new GameTime());
            CommandInfo.Add(new UpGetPoint() { OutGoalPoint = GL_X, InConstPositionType = (int)PositionType.SELF });
            CommandInfo.Add(new Goal() { InConstGoalId = GL_X });
            CommandInfo.Add(new Goal() { InConstGoalId = GL_Y });

            foreach (var sn in StrategicNumbers)
            {
                CommandInfo.Add(new SetStrategicNumber() { InConstSnId = sn.Key, InConstValue = sn.Value });
            }

            for (int sn = 0; sn < 512; sn++)
            {
                CommandInfo.Add(new Protos.Expert.Fact.StrategicNumber() { InConstSnId = sn });
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

            foreach (var command in UnitFindCommands)
            {
                yield return command;
            }

            foreach (var command in Commands)
            {
                yield return command;
            }

            Commands.Clear();

            sw.Stop();
            Bot.Log.Debug($"GameState RequestUpdate took {sw.ElapsedMilliseconds} ms");
        }

        internal void Update()
        {
            var sw = new Stopwatch();
            sw.Start();

            Bot.Log.Info("");
            Bot.Log.Info($"Tick {Tick} Game time {GameTime:g} with {GameTimePerTick:c} game time seconds per tick");
            foreach (var player in GetPlayers())
            {
                Bot.Log.Debug($"Player {player.PlayerNumber} has {player.Units.Count(u => u.Targetable)} units and {player.Score} score");
            }

            if (CommandInfo.HasResponses)
            {
                var responses = CommandInfo.GetResponses();

                var current_time = GameTime;
                GameTime = TimeSpan.FromSeconds(responses[0].Unpack<GameTimeResult>().Result);
                GameTimePerTick *= 99;
                GameTimePerTick += GameTime - current_time;
                GameTimePerTick /= 100;

                var x = responses[2].Unpack<GoalResult>().Result;
                var y = responses[3].Unpack<GoalResult>().Result;
                if (Map.IsOnMap(x, y))
                {
                    MyPosition = Position.FromPoint(x, y);
                }

                var index = 3;

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

            foreach (var command in UnitFindCommands.Where(c => c.HasResponses))
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

            UnitFindCommands.Clear();

            foreach (var player in Players)
            {
                player._Units.Clear();
            }

            foreach (var tile in Map.GetTiles())
            {
                tile._Units.Clear();
            }

            foreach (var unit in Units.Values.Where(u => u.Updated))
            {
                if (unit[ObjectData.PLAYER] >= 0)
                {
                    Players[unit[ObjectData.PLAYER]]._Units.Add(unit);

                    if (Map.TryGetTile(unit.Position, out var tile))
                    {
                        tile._Units.Add(unit);
                    }
                }
            }

            sw.Stop();
            Bot.Log.Debug($"GameState Update took {sw.ElapsedMilliseconds} ms");
        }
        private void DoAutoFindUnits()
        {
            if (Bot.AutoFindUnits == false)
            {
                return;
            }

            if (!Map.IsOnMap(MyPosition))
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

                if (Map.TryGetTile(x, y, out var tile))
                {
                    if (tile.Explored)
                    {
                        position = tile.Position;

                        break;
                    }
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
            Bot.Log.Debug($"GameState.DoAutoFindUnits took {sw.ElapsedMilliseconds} ms");
        }

        private void DoAutoUpdateUnits()
        {
            if (Bot.AutoUpdateUnits <= 0)
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

                if (first >= Bot.AutoUpdateUnits)
                {
                    break;
                }
            }

            Bot.Log.Debug($"Auto updating {first} first units");

            foreach (var player in GetPlayers())
            {
                var units = player.Units.ToList();
                units.Sort((a, b) => a.LastUpdateTick.CompareTo(b.LastUpdateTick));
                var count = Math.Min(units.Count, Bot.AutoUpdateUnits);

                for (int i = 0; i < count; i++)
                {
                    units[i].RequestUpdate();
                }

                Bot.Log.Debug($"Auto updating {count} units for player {player.PlayerNumber}");
            }

            sw.Stop();
            Bot.Log.Debug($"GameState.DoAutoUpdateUnits took {sw.ElapsedMilliseconds} ms");
        }
    }
}
