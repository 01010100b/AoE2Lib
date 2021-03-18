using AoE2Lib.Bots.GameElements;
using AoE2Lib.Utils;
using Google.Protobuf.WellKnownTypes;
using Protos.Expert;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AoE2Lib.Bots.Modules
{
    public class UnitsModule : Module
    {
        public IReadOnlyDictionary<int, UnitType> UnitTypes => _UnitTypes;
        private readonly Dictionary<int, UnitType> _UnitTypes = new Dictionary<int, UnitType>();
        public IReadOnlyDictionary<int, Unit> Units => _Units;
        private readonly Dictionary<int, Unit> _Units = new Dictionary<int, Unit>();
        public bool AutoUpdateUnits { get; set; } = true;
        public bool AutoFindUnits { get; set; } = true;

        private readonly List<Command> CreateCommands = new List<Command>();
        private readonly List<Command> FindUnitCommands = new List<Command>();
        private readonly Command ScanUnitsCommand = new Command();

        public void AddUnitType(int id)
        {
            if (!UnitTypes.ContainsKey(id))
            {
                _UnitTypes.Add(id, new UnitType(Bot, id));
                Bot.Log.Info($"InfoModule: Added unit {id}");
            }
        }

        public void Build(int id, Position position)
        {
            AddUnitType(id);

            var command = new Command();
            command.Add(new SetGoal() { InConstGoalId = 100, InConstValue = position.PointX });
            command.Add(new SetGoal() { InConstGoalId = 101, InConstValue = position.PointY });
            command.Add(new UpBuildLine() { InConstBuildingId = id, InGoalPoint1 = 100, InGoalPoint2 = 100 });

            CreateCommands.Add(command);
        }

        public void Train(int id)
        {
            AddUnitType(id);

            var command = new Command();
            command.Add(new Train() { InConstUnitId = id });
            CreateCommands.Add(command);
        }

        public void FindUnits(int player, Position position, int range, CmdId cmdid)
        {
            var command = new Command();

            command.Add(new SetGoal() { InConstGoalId = 100, InConstValue = position.PointX });
            command.Add(new SetGoal() { InConstGoalId = 101, InConstValue = position.PointY });
            command.Add(new UpSetTargetPoint() { InGoalPoint = 100 });
            command.Add(new SetStrategicNumber() { InConstSnId = (int)StrategicNumber.FOCUS_PLAYER_NUMBER, InConstValue = player });
            command.Add(new UpFullResetSearch());

            command.Add(new UpFilterInclude() { InConstCmdId = (int)cmdid, InConstActionId = -1, InConstOrderId = -1, InConstOnMainland = -1 });
            command.Add(new UpFilterDistance() { InConstMinDistance = -1, InConstMaxDistance = range });
            command.Add(new UpFindRemote() { InConstUnitId = -1, InConstCount = 40 });
            command.Add(new UpFilterDistance() { InConstMinDistance = range - 1, InConstMaxDistance = -1 });
            command.Add(new UpFindRemote() { InConstUnitId = -1, InConstCount = 40 });

            command.Add(new UpSearchObjectIdList() { InConstSearchSource = 2 });

            FindUnitCommands.Add(command);

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
                command.Add(new UpFindStatusRemote() { InConstUnitId = -1, InConstCount = 40 });
                command.Add(new UpFilterDistance() { InConstMinDistance = range - 1, InConstMaxDistance = -1 });
                command.Add(new UpFindStatusRemote() { InConstUnitId = -1, InConstCount = 40 });

                command.Add(new UpSearchObjectIdList() { InConstSearchSource = 2 });

                FindUnitCommands.Add(command);
            }
        }

        public void FindResources(int player, Position position, int range, Resource resource)
        {
            var command = new Command();

            command.Add(new SetGoal() { InConstGoalId = 100, InConstValue = position.PointX });
            command.Add(new SetGoal() { InConstGoalId = 101, InConstValue = position.PointY });
            command.Add(new UpSetTargetPoint() { InGoalPoint = 100 });
            command.Add(new SetStrategicNumber() { InConstSnId = (int)StrategicNumber.FOCUS_PLAYER_NUMBER, InConstValue = player });
            command.Add(new UpFullResetSearch());

            command.Add(new UpFilterDistance() { InConstMinDistance = -1, InConstMaxDistance = range });
            command.Add(new UpFilterStatus() { InConstObjectStatus = 2, InConstObjectList = 0 });
            command.Add(new UpFindResource() { InConstResource = (int)resource, InConstCount = 30 });
            command.Add(new UpFilterStatus() { InConstObjectStatus = 3, InConstObjectList = 0 });
            command.Add(new UpFindResource() { InConstResource = (int)resource, InConstCount = 40 });

            command.Add(new UpFilterDistance() { InConstMinDistance = range - 1, InConstMaxDistance = -1 });
            command.Add(new UpFilterStatus() { InConstObjectStatus = 3, InConstObjectList = 0 });
            command.Add(new UpFindResource() { InConstResource = (int)resource, InConstCount = 40 });
            command.Add(new UpFilterStatus() { InConstObjectStatus = 2, InConstObjectList = 0 });
            command.Add(new UpFindResource() { InConstResource = (int)resource, InConstCount = 40 });

            command.Add(new UpSearchObjectIdList() { InConstSearchSource = 2 });

            FindUnitCommands.Add(command);
        }

        protected override IEnumerable<Command> RequestUpdate()
        {
            foreach (var command in CreateCommands)
            {
                yield return command;
            }

            foreach (var type in UnitTypes.Values)
            {
                type.RequestUpdate();
            }

            if (AutoUpdateUnits && Units.Count > 0)
            {
                var units = Units.Values.Where(u => u.Updated == false || u.Targetable == true).ToList();
                units.Sort((a, b) => a.LastUpdateGameTime.CompareTo(b.LastUpdateGameTime));

                for (int i = 0; i < Math.Min(units.Count, 100); i++)
                {
                    units[i].RequestUpdate();
                }

                units.Clear();
                units.AddRange(Units.Values.Where(u => u.Targetable == false));

                if (units.Count > 0)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        units[Bot.Rng.Next(units.Count)].RequestUpdate();
                    }
                }
            }

            if (AutoFindUnits)
            {
                var player = Bot.PlayerNumber;
                var pos = Bot.InfoModule.MyPosition;
                var range = 25;
                if (Bot.InfoModule.StrategicNumbers.TryGetValue(StrategicNumber.MAXIMUM_TOWN_SIZE, out int r))
                {
                    range = Math.Max(range, r);
                }

                FindUnits(player, pos, range, CmdId.VILLAGER);
                FindUnits(player, pos, range, CmdId.MILITARY);
                FindUnits(player, pos, range, CmdId.CIVILIAN_BUILDING);
                FindUnits(player, pos, range, CmdId.MILITARY_BUILDING);
                FindUnits(player, pos, range, CmdId.FISHING_SHIP);
                FindUnits(player, pos, range, CmdId.LIVESTOCK_GAIA);
                FindUnits(player, pos, range, CmdId.MONK);
                FindUnits(player, pos, range, CmdId.TRADE);
                FindUnits(player, pos, range, CmdId.TRANSPORT);

                range += 10;

                FindResources(0, pos, range, Resource.WOOD);
                FindResources(0, pos, range, Resource.FOOD);
                FindResources(0, pos, range, Resource.GOLD);
                FindResources(0, pos, range, Resource.STONE);
                FindResources(0, pos, range, Resource.BOAR);
                FindResources(0, pos, range, Resource.DEER);

                var positions = new List<Position>();
                var map = Bot.MapModule;
                positions.AddRange(map.Tiles.Where(t => t.Explored).Select(t => t.Position));
                positions.Add(Bot.InfoModule.MyPosition);
                
                for (int i = 0; i < 50; i++)
                {
                    var position = positions[Bot.Rng.Next(positions.Count)];
                    AddAutoFindUnits(position);
                }
                
                for (int i = 1; i <= 8; i++)
                {
                    ScanUnitsCommand.Add(new PlayerValid() { InPlayerAnyPlayer = i }, "!=", 0,
                        new SetStrategicNumber() { InConstSnId = (int)StrategicNumber.FOCUS_PLAYER_NUMBER, InConstValue = i },
                        new UpFullResetSearch(),
                        new UpFindRemote() { InConstUnitId = -1, InConstCount = 40},
                        new UpSearchObjectIdList() { InConstSearchSource = 2}
                        );
                }
            }

            foreach (var command in FindUnitCommands)
            {
                yield return command;
            }

            yield return ScanUnitsCommand;
        }

        protected override void Update()
        {
            foreach (var command in FindUnitCommands.Where(c => c.HasResponses))
            {
                var responses = command.GetResponses();
                var ids = responses[responses.Count - 1].Unpack<UpSearchObjectIdListResult>().Result.ToArray();

                foreach (var id in ids)
                {
                    if (!Units.ContainsKey(id))
                    {
                        _Units.Add(id, new Unit(Bot, id));
                    }
                }
            }

            if (ScanUnitsCommand.HasResponses)
            {
                var responses = ScanUnitsCommand.GetResponses();

                for (int i = 1; i <= 8; i++)
                {
                    var index = (i * 4) - 1;
                    var cc = responses[index].Unpack<ConditionalCommandResult>();
                    if (cc.Fired)
                    {
                        var ids = cc.Result.Unpack<UpSearchObjectIdListResult>().Result.ToList();

                        foreach (var id in ids)
                        {
                            if (!Units.ContainsKey(id))
                            {
                                _Units.Add(id, new Unit(Bot, id));
                            }
                        }
                    }
                }
            }

            CreateCommands.Clear();
            FindUnitCommands.Clear();
            ScanUnitsCommand.Reset();
        }

        private void AddAutoFindUnits(Position position)
        {
            var range = 20;

            var player = Bot.PlayerNumber;
            if (Bot.Rng.NextDouble() < 0.5)
            {
                var players = Bot.PlayersModule.Players.Values.Where(p => p.InGame).Select(p => p.PlayerNumber).ToList();
                players.Add(0);

                player = players[Bot.Rng.Next(players.Count)];
            }

            if (player == 0)
            {
                if (Bot.Rng.NextDouble() < 0.5)
                {
                    FindResources(player, position, range, Resource.WOOD);
                }
                else if (Bot.Rng.NextDouble() < 0.5)
                {
                    FindResources(player, position, range, Resource.FOOD);
                }
                else if (Bot.Rng.NextDouble() < 0.5)
                {
                    FindResources(player, position, range, Resource.DEER);
                }
                else if (Bot.Rng.NextDouble() < 0.5)
                {
                    FindResources(player, position, range, Resource.BOAR);
                }
                else if (Bot.Rng.NextDouble() < 0.5)
                {
                    FindResources(player, position, range, Resource.GOLD);
                }
                else if (Bot.Rng.NextDouble() < 0.5)
                {
                    FindResources(player, position, range, Resource.STONE);
                }
                else
                {
                    FindUnits(player, position, range, CmdId.RELIC);
                }
            }
            else
            {
                if (Bot.Rng.NextDouble() < 0.5)
                {
                    FindUnits(player, position, range, CmdId.MILITARY);
                }
                else if (Bot.Rng.NextDouble() < 0.5)
                {
                    FindUnits(player, position, range, CmdId.VILLAGER);
                }
                else if (Bot.Rng.NextDouble() < 0.5)
                {
                    FindUnits(player, position, range, CmdId.MILITARY_BUILDING);
                }
                else if (Bot.Rng.NextDouble() < 0.5)
                {
                    FindUnits(player, position, range, CmdId.CIVILIAN_BUILDING);
                }
                else if (Bot.Rng.NextDouble() < 0.5)
                {
                    FindUnits(player, position, range, CmdId.MONK);
                }
                else if(Bot.Rng.NextDouble() < 0.5)
                {
                    FindUnits(player, position, range, CmdId.LIVESTOCK_GAIA);
                }
                else if (Bot.Rng.NextDouble() < 0.5)
                {
                    FindUnits(player, position, range, CmdId.TRADE);
                }
                else if (Bot.Rng.NextDouble() < 0.5)
                {
                    FindUnits(player, position, range, CmdId.FISHING_SHIP);
                }
                else
                {
                    FindUnits(player, position, range, CmdId.TRANSPORT);
                }
                
            }
        }
    }
}
