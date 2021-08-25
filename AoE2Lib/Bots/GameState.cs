using AoE2Lib.Bots.GameElements;
using AoE2Lib.Utils;
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
        public Player MyPlayer => Bot.MyPlayer;
        public Position MyPosition { get; private set; } = Position.Zero;
        public TimeSpan GameTime { get; private set; } = TimeSpan.Zero;
        public TimeSpan GameTimePerTick { get; private set; } = TimeSpan.FromSeconds(0.7);

        public int AutoFindUnits { get; set; } = 1; // players to find units for per tick, set to 0 to disable
        public int AutoUpdateUnits { get; set; } = 100; // units to update per tick, set to 0 to disable

        private readonly Bot Bot;
        private readonly Dictionary<int, Unit> Units = new Dictionary<int, Unit>();
        private readonly Dictionary<Resource, bool> ResourceFound = new Dictionary<Resource, bool>();
        private readonly Dictionary<Resource, int> DropsiteMinDistance = new Dictionary<Resource, int>();
        private readonly Dictionary<StrategicNumber, int> StrategicNumbers = new Dictionary<StrategicNumber, int>();
        private readonly List<Command> Commands = new List<Command>();
        private readonly List<Command> FindCommands = new List<Command>();
        private readonly Command CommandInfo = new Command();

        public GameState(Bot bot)
        {
            Bot = bot;

            Map = new Map(Bot);
        }

        public Unit GetUnitById(int id)
        {
            if (Units.TryGetValue(id, out Unit unit))
            {
                return unit;
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<Unit> GetAllUnits()
        {
            return Units.Values.Where(u => u.Updated);
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

        internal void AddCommand(Command command)
        {
            Commands.Add(command);
        }

        internal void FindUnits(int player, Position position, int range)
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
        }

        internal IEnumerable<Command> RequestUpdate()
        {
            DoAutoFindUnits();
            DoAutoUpdateUnits();

            Map.RequestUpdate();

            foreach (var command in Commands)
            {
                yield return command;
            }

            Commands.Clear();

            foreach (var command in FindCommands)
            {
                yield return command;
            }

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
        }

        internal void Update()
        {
            Map.Update();

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
                GameTimePerTick *= 19;
                GameTimePerTick += GameTime - current_time;
                GameTimePerTick /= 20;

                var x = responses[2].Unpack<GoalResult>().Result;
                var y = responses[3].Unpack<GoalResult>().Result;
                var pos = Position.FromPoint(x, y);
                if (pos.PointX >= 0 && pos.PointY >= 0 && pos.PointX < Bot.GameState.Map.Width && pos.PointY < Bot.GameState.Map.Height)
                {
                    MyPosition = pos;
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
            }

            var players = Bot.GetPlayers().ToList();
            players.Sort((a, b) => a.PlayerNumber.CompareTo(b.PlayerNumber));
            var num = new Dictionary<int, int>();
            foreach (var player in players)
            {
                num.Add(player.PlayerNumber, 0);
            }

            foreach (var unit in GetAllUnits())
            {
                if (num.ContainsKey(unit.PlayerNumber))
                {
                    num[unit.PlayerNumber]++;
                }
            }

            foreach (var player in players)
            {
                Bot.Log.Debug($"Player {player.PlayerNumber} units {num[player.PlayerNumber]} score {player.Score}");
            }
        }

        private void DoAutoFindUnits()
        {
            if (AutoFindUnits <= 0)
            {
                return;
            }

            if (Map.Width <= 0 || Map.Height <= 0)
            {
                return;
            }

            var players = Bot.GetPlayers().ToList();

            for (int i = 0; i < AutoFindUnits; i++)
            {
                if (players.Count == 0)
                {
                    break;
                }

                var player = players[Bot.Rng.Next(players.Count)];
                players.Remove(player);

                var position = MyPosition;
                for (int j = 0; j < 100; j++)
                {
                    var x = Bot.Rng.Next(Map.Width);
                    var y = Bot.Rng.Next(Map.Height);
                    var tile = Map.GetTile(x, y);

                    if (tile.Explored)
                    {
                        position = Position.FromPoint(x, y);
                    }
                }

                var range = 1000;
                if (Bot.Rng.NextDouble() < 0.5)
                {
                    range = 10;
                }

                player.FindUnits(position, range);
            }
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
        }
    }
}
