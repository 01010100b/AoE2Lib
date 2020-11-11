using Google.Protobuf.WellKnownTypes;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.GameElements;
using Unary.Utils;

namespace Unary.Modules
{
    public class UnitFindModule : Module
    {
        public enum UnitFindType
        {
            MILLITARY, CIVILIAN, BUILDING, WOOD, FOOD, GOLD, STONE, ALL
        }

        private readonly List<Command> UnitFindCommands = new List<Command>();
        private readonly Random RNG = new Random(Guid.NewGuid().GetHashCode() ^ DateTime.UtcNow.Ticks.GetHashCode());

        public void FindUnits(Position position, int player, UnitFindType type)
        {
            var command = new Command();

            command.Messages.Add(new SetGoal() { GoalId = 50, GoalValue = position.X });
            command.Messages.Add(new SetGoal() { GoalId = 51, GoalValue = position.Y });
            command.Messages.Add(new UpSetTargetPoint() { GoalPoint = 50 });
            command.Messages.Add(new SetStrategicNumber() { StrategicNumber = (int)StrategicNumber.FOCUS_PLAYER_NUMBER, Value = player });
            command.Messages.Add(new UpFullResetSearch());

            switch (type)
            {
                case UnitFindType.MILLITARY:

                    command.Messages.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = -1, TypeOp2 = (int)TypeOp.C, MaxDistance = 10 });
                    command.Messages.Add(new UpFilterInclude() { CmdId = (int)CmdId.MILITARY, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Messages.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 35 });
                    command.Messages.Add(new UpFilterInclude() { CmdId = (int)CmdId.MONK, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Messages.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 5 });

                    command.Messages.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = 9, TypeOp2 = (int)TypeOp.C, MaxDistance = -1 });
                    command.Messages.Add(new UpFilterInclude() { CmdId = (int)CmdId.MONK, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Messages.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 5 });
                    command.Messages.Add(new UpFilterInclude() { CmdId = (int)CmdId.MILITARY, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Messages.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 40 });

                    break;

                case UnitFindType.CIVILIAN:

                    command.Messages.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = -1, TypeOp2 = (int)TypeOp.C, MaxDistance = 10 });
                    command.Messages.Add(new UpFilterInclude() { CmdId = (int)CmdId.VILLAGER, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Messages.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 30 });
                    command.Messages.Add(new UpFilterInclude() { CmdId = (int)CmdId.TRADE, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Messages.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 5 });
                    command.Messages.Add(new UpFilterInclude() { CmdId = (int)CmdId.FISHING_SHIP, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Messages.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 5 });

                    command.Messages.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = 9, TypeOp2 = (int)TypeOp.C, MaxDistance = -1 });
                    command.Messages.Add(new UpFilterInclude() { CmdId = (int)CmdId.FISHING_SHIP, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Messages.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 5 });
                    command.Messages.Add(new UpFilterInclude() { CmdId = (int)CmdId.TRADE, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Messages.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 5 });
                    command.Messages.Add(new UpFilterInclude() { CmdId = (int)CmdId.VILLAGER, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Messages.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 40 });

                    break;

                case UnitFindType.BUILDING:

                    command.Messages.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = -1, TypeOp2 = (int)TypeOp.C, MaxDistance = 10 });
                    command.Messages.Add(new UpFilterInclude() { CmdId = (int)CmdId.MILITARY_BUILDING, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Messages.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 20 });
                    command.Messages.Add(new UpFilterInclude() { CmdId = (int)CmdId.CIVILIAN_BUILDING, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Messages.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 20 });

                    command.Messages.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = 9, TypeOp2 = (int)TypeOp.C, MaxDistance = -1 });
                    command.Messages.Add(new UpFilterInclude() { CmdId = (int)CmdId.CIVILIAN_BUILDING, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Messages.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 20 });
                    command.Messages.Add(new UpFilterInclude() { CmdId = (int)CmdId.MILITARY_BUILDING, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Messages.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 40 });

                    break;

                case UnitFindType.WOOD:

                    command.Messages.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = -1, TypeOp2 = (int)TypeOp.C, MaxDistance = 10 });
                    command.Messages.Add(new UpFilterStatus() { TypeOp1 = (int)TypeOp.C, ObjectStatus = 2, TypeOp2 = (int)TypeOp.C, ObjectList = 0 });
                    command.Messages.Add(new UpFindResource() { TypeOp1 = (int)TypeOp.C, Resource = 1, TypeOp2 = (int)TypeOp.C, Count = 30 });
                    command.Messages.Add(new UpFilterStatus() { TypeOp1 = (int)TypeOp.C, ObjectStatus = 3, TypeOp2 = (int)TypeOp.C, ObjectList = 0 });
                    command.Messages.Add(new UpFindResource() { TypeOp1 = (int)TypeOp.C, Resource = 1, TypeOp2 = (int)TypeOp.C, Count = 10 });

                    command.Messages.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = 9, TypeOp2 = (int)TypeOp.C, MaxDistance = -1 });
                    command.Messages.Add(new UpFilterStatus() { TypeOp1 = (int)TypeOp.C, ObjectStatus = 3, TypeOp2 = (int)TypeOp.C, ObjectList = 0 });
                    command.Messages.Add(new UpFindResource() { TypeOp1 = (int)TypeOp.C, Resource = 1, TypeOp2 = (int)TypeOp.C, Count = 10 });
                    command.Messages.Add(new UpFilterStatus() { TypeOp1 = (int)TypeOp.C, ObjectStatus = 2, TypeOp2 = (int)TypeOp.C, ObjectList = 0 });
                    command.Messages.Add(new UpFindResource() { TypeOp1 = (int)TypeOp.C, Resource = 1, TypeOp2 = (int)TypeOp.C, Count = 40 });

                    break;

                case UnitFindType.FOOD:

                    command.Messages.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = -1, TypeOp2 = (int)TypeOp.C, MaxDistance = 10 });
                    command.Messages.Add(new UpFilterStatus() { TypeOp1 = (int)TypeOp.C, ObjectStatus = 2, TypeOp2 = (int)TypeOp.C, ObjectList = 0 });
                    command.Messages.Add(new UpFindResource() { TypeOp1 = (int)TypeOp.C, Resource = 0, TypeOp2 = (int)TypeOp.C, Count = 30 });
                    command.Messages.Add(new UpFilterStatus() { TypeOp1 = (int)TypeOp.C, ObjectStatus = 3, TypeOp2 = (int)TypeOp.C, ObjectList = 0 });
                    command.Messages.Add(new UpFindResource() { TypeOp1 = (int)TypeOp.C, Resource = 0, TypeOp2 = (int)TypeOp.C, Count = 10 });

                    command.Messages.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = 9, TypeOp2 = (int)TypeOp.C, MaxDistance = -1 });
                    command.Messages.Add(new UpFilterStatus() { TypeOp1 = (int)TypeOp.C, ObjectStatus = 3, TypeOp2 = (int)TypeOp.C, ObjectList = 0 });
                    command.Messages.Add(new UpFindResource() { TypeOp1 = (int)TypeOp.C, Resource = 0, TypeOp2 = (int)TypeOp.C, Count = 10 });
                    command.Messages.Add(new UpFilterStatus() { TypeOp1 = (int)TypeOp.C, ObjectStatus = 2, TypeOp2 = (int)TypeOp.C, ObjectList = 0 });
                    command.Messages.Add(new UpFindResource() { TypeOp1 = (int)TypeOp.C, Resource = 0, TypeOp2 = (int)TypeOp.C, Count = 40 });

                    break;

                case UnitFindType.GOLD:

                    command.Messages.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = -1, TypeOp2 = (int)TypeOp.C, MaxDistance = 10 });
                    command.Messages.Add(new UpFilterStatus() { TypeOp1 = (int)TypeOp.C, ObjectStatus = 3, TypeOp2 = (int)TypeOp.C, ObjectList = 0 });
                    command.Messages.Add(new UpFindResource() { TypeOp1 = (int)TypeOp.C, Resource = 3, TypeOp2 = (int)TypeOp.C, Count = 40 });

                    command.Messages.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = 9, TypeOp2 = (int)TypeOp.C, MaxDistance = -1 });
                    command.Messages.Add(new UpFilterStatus() { TypeOp1 = (int)TypeOp.C, ObjectStatus = 3, TypeOp2 = (int)TypeOp.C, ObjectList = 0 });
                    command.Messages.Add(new UpFindResource() { TypeOp1 = (int)TypeOp.C, Resource = 3, TypeOp2 = (int)TypeOp.C, Count = 40 });

                    break;

                case UnitFindType.STONE:

                    command.Messages.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = -1, TypeOp2 = (int)TypeOp.C, MaxDistance = 10 });
                    command.Messages.Add(new UpFilterStatus() { TypeOp1 = (int)TypeOp.C, ObjectStatus = 3, TypeOp2 = (int)TypeOp.C, ObjectList = 0 });
                    command.Messages.Add(new UpFindResource() { TypeOp1 = (int)TypeOp.C, Resource = 2, TypeOp2 = (int)TypeOp.C, Count = 40 });

                    command.Messages.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = 9, TypeOp2 = (int)TypeOp.C, MaxDistance = -1 });
                    command.Messages.Add(new UpFilterStatus() { TypeOp1 = (int)TypeOp.C, ObjectStatus = 3, TypeOp2 = (int)TypeOp.C, ObjectList = 0 });
                    command.Messages.Add(new UpFindResource() { TypeOp1 = (int)TypeOp.C, Resource = 2, TypeOp2 = (int)TypeOp.C, Count = 40 });

                    break;

                case UnitFindType.ALL:

                    command.Messages.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = -1, TypeOp2 = (int)TypeOp.C, MaxDistance = 10 });
                    command.Messages.Add(new UpFilterExclude() { CmdId = (int)CmdId.MILITARY, ActionId = -1, OrderId = -1, ClassId = 915 });
                    command.Messages.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 40 });

                    command.Messages.Add(new UpFilterDistance() { TypeOp1 = (int)TypeOp.C, MinDistance = 9, TypeOp2 = (int)TypeOp.C, MaxDistance = -1 });
                    command.Messages.Add(new UpFilterExclude() { CmdId = (int)CmdId.MONK, ActionId = -1, OrderId = -1, ClassId = 915 });
                    command.Messages.Add(new UpFindRemote() { TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 40 });

                    break;

                default: throw new NotImplementedException();
            }

            command.Messages.Add(new UpGetSearchState() { GoalState = 100 }); // remote-total = 102
            command.Messages.Add(new UpModifyGoal() { GoalId = 102, MathOp = (int)MathOp.C_MAX, Value = 1 });
            command.Messages.Add(new Goal() { GoalId = 102 });

            for (int i = 0; i < 40; i++)
            {
                command.Messages.Add(new SetGoal() { GoalId = 200, GoalValue = i });
                command.Messages.Add(new UpModifyGoal() { GoalId = 200, MathOp = (int)MathOp.G_MOD, Value = 102 });
                command.Messages.Add(new Goal() { GoalId = 200 });
                command.Messages.Add(new UpSetTargetObject() { SearchSource = 2, TypeOp = (int)TypeOp.G, Index = 200 });
                command.Messages.Add(new UpObjectData() { ObjectData = (int)ObjectData.ID });
            }

            UnitFindCommands.Add(command);
        }

        internal override IEnumerable<Command> RequestUpdate(Bot bot)
        {
            if (bot.GameState.Tick > 0)
            {
                AddDefaultCommands(bot);
            }

            return UnitFindCommands;
        }

        internal override void Update(Bot bot)
        {
            foreach (var command in UnitFindCommands)
            {
                for (int i = 0; i < 40; i++)
                {
                    var pos = command.Responses.Count - (5 * i) - 1;
                    var id = command.Responses[pos].Unpack<UpObjectDataResult>().Result;
                    
                    if (id > 0)
                    {
                        bot.GameState.AddUnit(id);
                    }
                }
            }   
            
            UnitFindCommands.Clear();
        }

        private void AddDefaultCommands(Bot bot)
        {
            var explored = bot.GameState.Tiles.Values.Where(t => t.Explored).Select(t => t.Position).ToList();
            if (explored.Count == 0)
            {
                explored.Add(bot.GameState.MyPosition);
            }

            for (int i = 0; i < 5; i++)
            {
                var player = bot.Player;

                if (RNG.NextDouble() < 0.5 && bot.GameState.GameTime > TimeSpan.FromSeconds(3))
                {
                    player = 0;

                    if (RNG.NextDouble() < 0.5 && bot.GameState.Players.Count > 0)
                    {
                        player = bot.GameState.Players.Values.ElementAt(RNG.Next(bot.GameState.Players.Count)).PlayerNumber;
                    }
                }

                var type = UnitFindType.CIVILIAN;

                if (RNG.NextDouble() < 0.5)
                {
                    type = UnitFindType.MILLITARY;

                    if (RNG.NextDouble() < 0.5 && bot.GameState.GameTime > TimeSpan.FromSeconds(5))
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
        }
    }
}
