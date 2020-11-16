using AoE2Lib.Bots.GameElements;
using AoE2Lib.Utils;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoE2Lib.Bots.Modules
{
    public class UnitsModule : Module
    {
        public IReadOnlyDictionary<int, Unit> Units => _Units;
        private readonly Dictionary<int, Unit> _Units = new Dictionary<int, Unit>();

        private readonly List<Command> UnitFindCommands = new List<Command>();
        private readonly Random RNG = new Random(Guid.NewGuid().GetHashCode() ^ DateTime.UtcNow.Ticks.GetHashCode());

        public void FindUnits(Position position, int player, UnitFindType type)
        {
            var command = new Command();

            command.Add(new SetGoal() { GoalId = 50, GoalValue = position.PointX });
            command.Add(new SetGoal() { GoalId = 51, GoalValue = position.PointY });
            command.Add(new UpSetTargetPoint() { GoalPoint = 50 });
            command.Add(new SetStrategicNumber() { StrategicNumber = (int)StrategicNumber.FOCUS_PLAYER_NUMBER, Value = player });
            command.Add(new UpFullResetSearch());

            switch (type)
            {
                case UnitFindType.MILLITARY:

                    command.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = -1, TypeOp2 = (int)TypeOp.C, MaxDistance = 10 });
                    command.Add(new UpFilterInclude() { CmdId = (int)CmdId.MILITARY, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 35 });
                    command.Add(new UpFilterInclude() { CmdId = (int)CmdId.MONK, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 5 });

                    command.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = 9, TypeOp2 = (int)TypeOp.C, MaxDistance = -1 });
                    command.Add(new UpFilterInclude() { CmdId = (int)CmdId.MONK, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 5 });
                    command.Add(new UpFilterInclude() { CmdId = (int)CmdId.MILITARY, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 40 });

                    break;

                case UnitFindType.CIVILIAN:

                    command.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = -1, TypeOp2 = (int)TypeOp.C, MaxDistance = 10 });
                    command.Add(new UpFilterInclude() { CmdId = (int)CmdId.VILLAGER, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 30 });
                    command.Add(new UpFilterInclude() { CmdId = (int)CmdId.TRADE, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 5 });
                    command.Add(new UpFilterInclude() { CmdId = (int)CmdId.FISHING_SHIP, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 5 });

                    command.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = 9, TypeOp2 = (int)TypeOp.C, MaxDistance = -1 });
                    command.Add(new UpFilterInclude() { CmdId = (int)CmdId.FISHING_SHIP, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 5 });
                    command.Add(new UpFilterInclude() { CmdId = (int)CmdId.TRADE, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 5 });
                    command.Add(new UpFilterInclude() { CmdId = (int)CmdId.VILLAGER, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 40 });

                    break;

                case UnitFindType.BUILDING:

                    command.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = -1, TypeOp2 = (int)TypeOp.C, MaxDistance = 10 });
                    command.Add(new UpFilterInclude() { CmdId = (int)CmdId.MILITARY_BUILDING, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 20 });
                    command.Add(new UpFilterInclude() { CmdId = (int)CmdId.CIVILIAN_BUILDING, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 20 });

                    command.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = 9, TypeOp2 = (int)TypeOp.C, MaxDistance = -1 });
                    command.Add(new UpFilterInclude() { CmdId = (int)CmdId.CIVILIAN_BUILDING, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 20 });
                    command.Add(new UpFilterInclude() { CmdId = (int)CmdId.MILITARY_BUILDING, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 40 });

                    break;

                case UnitFindType.WOOD:

                    command.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = -1, TypeOp2 = (int)TypeOp.C, MaxDistance = 10 });
                    command.Add(new UpFilterStatus() { TypeOp1 = (int)TypeOp.C, ObjectStatus = 2, TypeOp2 = (int)TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindResource() { TypeOp1 = (int)TypeOp.C, Resource = 1, TypeOp2 = (int)TypeOp.C, Count = 30 });
                    command.Add(new UpFilterStatus() { TypeOp1 = (int)TypeOp.C, ObjectStatus = 3, TypeOp2 = (int)TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindResource() { TypeOp1 = (int)TypeOp.C, Resource = 1, TypeOp2 = (int)TypeOp.C, Count = 10 });

                    command.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = 9, TypeOp2 = (int)TypeOp.C, MaxDistance = -1 });
                    command.Add(new UpFilterStatus() { TypeOp1 = (int)TypeOp.C, ObjectStatus = 3, TypeOp2 = (int)TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindResource() { TypeOp1 = (int)TypeOp.C, Resource = 1, TypeOp2 = (int)TypeOp.C, Count = 10 });
                    command.Add(new UpFilterStatus() { TypeOp1 = (int)TypeOp.C, ObjectStatus = 2, TypeOp2 = (int)TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindResource() { TypeOp1 = (int)TypeOp.C, Resource = 1, TypeOp2 = (int)TypeOp.C, Count = 40 });

                    break;

                case UnitFindType.FOOD:

                    command.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = -1, TypeOp2 = (int)TypeOp.C, MaxDistance = 10 });
                    command.Add(new UpFilterStatus() { TypeOp1 = (int)TypeOp.C, ObjectStatus = 2, TypeOp2 = (int)TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindResource() { TypeOp1 = (int)TypeOp.C, Resource = 0, TypeOp2 = (int)TypeOp.C, Count = 30 });
                    command.Add(new UpFilterStatus() { TypeOp1 = (int)TypeOp.C, ObjectStatus = 3, TypeOp2 = (int)TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindResource() { TypeOp1 = (int)TypeOp.C, Resource = 0, TypeOp2 = (int)TypeOp.C, Count = 10 });

                    command.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = 9, TypeOp2 = (int)TypeOp.C, MaxDistance = -1 });
                    command.Add(new UpFilterStatus() { TypeOp1 = (int)TypeOp.C, ObjectStatus = 3, TypeOp2 = (int)TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindResource() { TypeOp1 = (int)TypeOp.C, Resource = 0, TypeOp2 = (int)TypeOp.C, Count = 10 });
                    command.Add(new UpFilterStatus() { TypeOp1 = (int)TypeOp.C, ObjectStatus = 2, TypeOp2 = (int)TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindResource() { TypeOp1 = (int)TypeOp.C, Resource = 0, TypeOp2 = (int)TypeOp.C, Count = 40 });

                    break;

                case UnitFindType.GOLD:

                    command.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = -1, TypeOp2 = (int)TypeOp.C, MaxDistance = 10 });
                    command.Add(new UpFilterStatus() { TypeOp1 = (int)TypeOp.C, ObjectStatus = 3, TypeOp2 = (int)TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindResource() { TypeOp1 = (int)TypeOp.C, Resource = 3, TypeOp2 = (int)TypeOp.C, Count = 40 });

                    command.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = 9, TypeOp2 = (int)TypeOp.C, MaxDistance = -1 });
                    command.Add(new UpFilterStatus() { TypeOp1 = (int)TypeOp.C, ObjectStatus = 3, TypeOp2 = (int)TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindResource() { TypeOp1 = (int)TypeOp.C, Resource = 3, TypeOp2 = (int)TypeOp.C, Count = 40 });

                    break;

                case UnitFindType.STONE:

                    command.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = -1, TypeOp2 = (int)TypeOp.C, MaxDistance = 10 });
                    command.Add(new UpFilterStatus() { TypeOp1 = (int)TypeOp.C, ObjectStatus = 3, TypeOp2 = (int)TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindResource() { TypeOp1 = (int)TypeOp.C, Resource = 2, TypeOp2 = (int)TypeOp.C, Count = 40 });

                    command.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = 9, TypeOp2 = (int)TypeOp.C, MaxDistance = -1 });
                    command.Add(new UpFilterStatus() { TypeOp1 = (int)TypeOp.C, ObjectStatus = 3, TypeOp2 = (int)TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindResource() { TypeOp1 = (int)TypeOp.C, Resource = 2, TypeOp2 = (int)TypeOp.C, Count = 40 });

                    break;

                case UnitFindType.ALL:

                    command.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = -1, TypeOp2 = (int)TypeOp.C, MaxDistance = 10 });
                    command.Add(new UpFilterExclude() { CmdId = (int)CmdId.MILITARY, ActionId = -1, OrderId = -1, ClassId = (int)UnitClass.Tree });
                    command.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 40 });

                    command.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = 9, TypeOp2 = (int)TypeOp.C, MaxDistance = -1 });
                    command.Add(new UpFilterExclude() { CmdId = (int)CmdId.MONK, ActionId = -1, OrderId = -1, ClassId = (int)UnitClass.Tree });
                    command.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 40 });

                    break;

                default: throw new NotImplementedException();
            }

            command.Add(new UpGetSearchState() { GoalState = 100 }); // remote-total = 102
            command.Add(new UpModifyGoal() { GoalId = 102, MathOp = (int)MathOp.C_MAX, Value = 1 });
            command.Add(new Goal() { GoalId = 102 });

            for (int i = 0; i < 40; i++)
            {
                command.Add(new SetGoal() { GoalId = 200, GoalValue = i });
                command.Add(new UpModifyGoal() { GoalId = 200, MathOp = (int)MathOp.G_MOD, Value = 102 });
                command.Add(new Goal() { GoalId = 200 });
                command.Add(new UpSetTargetObject() { SearchSource = 2, TypeOp = (int)TypeOp.G, Index = 200 });
                command.Add(new UpObjectData() { ObjectData = (int)ObjectData.ID });
            }

            UnitFindCommands.Add(command);
        }

        protected override IEnumerable<Command> RequestUpdate()
        {
            AddDefaultCommands();

            foreach (var unit in Units.Values)
            {
                yield return unit.Command;
            }

            foreach (var command in UnitFindCommands)
            {
                yield return command;
            }
        }

        protected override void Update()
        {
            foreach (var unit in Units.Values)
            {
                unit.Update();
            }

            foreach (var command in UnitFindCommands)
            {
                for (int i = 0; i < 40; i++)
                {
                    var pos = command.Responses.Count - (5 * i) - 1;
                    var id = command.Responses[pos].Unpack<UpObjectDataResult>().Result;

                    if (id > 0 && !Units.ContainsKey(id))
                    {
                        _Units.Add(id, new Unit(Bot, id));
                    }
                }
            }

            UnitFindCommands.Clear();
        }

        private void AddDefaultCommands()
        {
            const int NUM_FINDS = 5;
            const int NUM_UPDATES = 10;

            var explored = Bot.GetModule<MapModule>().GetTiles().Where(t => t.Explored).Select(t => t.Position).ToList();
            if (explored.Count == 0)
            {
                explored.Add(new Position(0, 0));
            }

            var gametime = Bot.GetModule<InfoModule>().GameTime;
            var players = Bot.GetModule<PlayersModule>().Players;

            for (int i = 0; i < NUM_FINDS; i++)
            {
                var player = Bot.PlayerNumber;

                if (RNG.NextDouble() < 0.5 && gametime > TimeSpan.FromSeconds(3))
                {
                    player = 0;

                    if (RNG.NextDouble() < 0.5 && players.Count > 0 && gametime > TimeSpan.FromSeconds(10))
                    {
                        player = players.Values.ElementAt(RNG.Next(players.Count)).PlayerNumber;
                    }
                }

                var type = UnitFindType.CIVILIAN;

                if (RNG.NextDouble() < 0.5)
                {
                    type = UnitFindType.MILLITARY;

                    if (RNG.NextDouble() < 0.5 && gametime > TimeSpan.FromSeconds(3))
                    {
                        type = UnitFindType.BUILDING;

                        if (RNG.NextDouble() < 0.5)
                        {
                            type = UnitFindType.FOOD;
                        }
                    }
                }

                if (player == 0)
                {
                    type = UnitFindType.WOOD;

                    if (RNG.NextDouble() < 0.5)
                    {
                        type = UnitFindType.FOOD;

                        if (RNG.NextDouble() < 0.5)
                        {
                            type = UnitFindType.GOLD;

                            if (RNG.NextDouble() < 0.5)
                            {
                                type = UnitFindType.STONE;

                                if (RNG.NextDouble() < 0.5)
                                {
                                    type = UnitFindType.ALL;
                                }
                            }
                        }
                    }
                }

                var position = explored[RNG.Next(explored.Count)];

                FindUnits(position, player, type);
            }

            var units = Bot.GetModule<UnitsModule>().Units.Values.ToList();
            units.Sort((a, b) => a.LastUpdate.CompareTo(b.LastUpdate));

            for (int i = 0; i < Math.Min(units.Count, NUM_UPDATES / 2); i++)
            {
                units[i].RequestUpdate();
                units[RNG.Next(units.Count)].RequestUpdate();
            }
        }
    }
}
