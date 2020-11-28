using AoE2Lib.Bots.GameElements;
using AoE2Lib.Mods;
using AoE2Lib.Utils;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static AoE2Lib.Bots.Modules.SpendingModule;

namespace AoE2Lib.Bots.Modules
{
    public class UnitsModule : Module
    {
        public bool AutoUpdateUnits { get; set; } = true;
        public IReadOnlyDictionary<int, Unit> Units => _Units;
        private readonly Dictionary<int, Unit> _Units = new Dictionary<int, Unit>();

        public bool AutoFindUnits { get; set; } = true;
        private readonly List<Command> UnitFindCommands = new List<Command>();
        private readonly Random RNG = new Random(Guid.NewGuid().GetHashCode());

        private readonly Dictionary<int, int> LastBuildTick = new Dictionary<int, int>();

        public void FindUnits(Position position, int range, int player, UnitFindType type)
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

                    command.Add(new UpFilterDistance() { TypeOp1 = TypeOp.C, MinDistance = -1, TypeOp2 = TypeOp.C, MaxDistance = range });
                    command.Add(new UpFilterInclude() { CmdId = (int)CmdId.MILITARY, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Add(new UpFindRemote() { TypeOp1 = TypeOp.C, UnitId = -1, TypeOp2 = TypeOp.C, Count = 35 });
                    command.Add(new UpFilterInclude() { CmdId = (int)CmdId.MONK, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Add(new UpFindRemote() { TypeOp1 = TypeOp.C, UnitId = -1, TypeOp2 = TypeOp.C, Count = 5 });

                    command.Add(new UpFilterDistance() { TypeOp1 = TypeOp.C, MinDistance = range - 1, TypeOp2 = TypeOp.C, MaxDistance = -1 });
                    command.Add(new UpFilterInclude() { CmdId = (int)CmdId.MONK, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Add(new UpFindRemote() { TypeOp1 = TypeOp.C, UnitId = -1, TypeOp2 = TypeOp.C, Count = 5 });
                    command.Add(new UpFilterInclude() { CmdId = (int)CmdId.MILITARY, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Add(new UpFindRemote() { TypeOp1 = TypeOp.C, UnitId = -1, TypeOp2 = TypeOp.C, Count = 40 });

                    break;

                case UnitFindType.CIVILIAN:

                    command.Add(new UpFilterDistance() { TypeOp1 = TypeOp.C, MinDistance = -1, TypeOp2 = TypeOp.C, MaxDistance = range });
                    command.Add(new UpFilterInclude() { CmdId = (int)CmdId.VILLAGER, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Add(new UpFindRemote() { TypeOp1 = TypeOp.C, UnitId = -1, TypeOp2 = TypeOp.C, Count = 30 });
                    command.Add(new UpFilterInclude() { CmdId = (int)CmdId.TRADE, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Add(new UpFindRemote() { TypeOp1 = TypeOp.C, UnitId = -1, TypeOp2 = TypeOp.C, Count = 5 });
                    command.Add(new UpFilterInclude() { CmdId = (int)CmdId.FISHING_SHIP, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Add(new UpFindRemote() { TypeOp1 = TypeOp.C, UnitId = -1, TypeOp2 = TypeOp.C, Count = 5 });

                    command.Add(new UpFilterDistance() { TypeOp1 = TypeOp.C, MinDistance = range - 1, TypeOp2 = TypeOp.C, MaxDistance = -1 });
                    command.Add(new UpFilterInclude() { CmdId = (int)CmdId.FISHING_SHIP, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Add(new UpFindRemote() { TypeOp1 = TypeOp.C, UnitId = -1, TypeOp2 = TypeOp.C, Count = 5 });
                    command.Add(new UpFilterInclude() { CmdId = (int)CmdId.TRADE, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Add(new UpFindRemote() { TypeOp1 = TypeOp.C, UnitId = -1, TypeOp2 = TypeOp.C, Count = 5 });
                    command.Add(new UpFilterInclude() { CmdId = (int)CmdId.VILLAGER, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Add(new UpFindRemote() { TypeOp1 = TypeOp.C, UnitId = -1, TypeOp2 = TypeOp.C, Count = 40 });

                    break;

                case UnitFindType.BUILDING:

                    command.Add(new UpFilterDistance() { TypeOp1 = TypeOp.C, MinDistance = -1, TypeOp2 = TypeOp.C, MaxDistance = range });

                    command.Add(new UpFilterInclude() { CmdId = (int)CmdId.MILITARY_BUILDING, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Add(new UpFilterStatus() { TypeOp1 = TypeOp.C, ObjectStatus = 2, TypeOp2 = TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindStatusRemote() { TypeOp1 = TypeOp.C, UnitId = -1, TypeOp2 = TypeOp.C, Count = 8 });
                    command.Add(new UpFilterStatus() { TypeOp1 = TypeOp.C, ObjectStatus = 2, TypeOp2 = TypeOp.C, ObjectList = 1 });
                    command.Add(new UpFindStatusRemote() { TypeOp1 = TypeOp.C, UnitId = -1, TypeOp2 = TypeOp.C, Count = 8 });
                    command.Add(new UpFilterStatus() { TypeOp1 = TypeOp.C, ObjectStatus = 0, TypeOp2 = TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindStatusRemote() { TypeOp1 = TypeOp.C, UnitId = -1, TypeOp2 = TypeOp.C, Count = 2 });
                    command.Add(new UpFilterStatus() { TypeOp1 = TypeOp.C, ObjectStatus = 0, TypeOp2 = TypeOp.C, ObjectList = 1 });
                    command.Add(new UpFindStatusRemote() { TypeOp1 = TypeOp.C, UnitId = -1, TypeOp2 = TypeOp.C, Count = 2 });

                    command.Add(new UpFilterInclude() { CmdId = (int)CmdId.CIVILIAN_BUILDING, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Add(new UpFilterStatus() { TypeOp1 = TypeOp.C, ObjectStatus = 2, TypeOp2 = TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindStatusRemote() { TypeOp1 = TypeOp.C, UnitId = -1, TypeOp2 = TypeOp.C, Count = 8 });
                    command.Add(new UpFilterStatus() { TypeOp1 = TypeOp.C, ObjectStatus = 2, TypeOp2 = TypeOp.C, ObjectList = 1 });
                    command.Add(new UpFindStatusRemote() { TypeOp1 = TypeOp.C, UnitId = -1, TypeOp2 = TypeOp.C, Count = 8 });
                    command.Add(new UpFilterStatus() { TypeOp1 = TypeOp.C, ObjectStatus = 0, TypeOp2 = TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindStatusRemote() { TypeOp1 = TypeOp.C, UnitId = -1, TypeOp2 = TypeOp.C, Count = 2 });
                    command.Add(new UpFilterStatus() { TypeOp1 = TypeOp.C, ObjectStatus = 0, TypeOp2 = TypeOp.C, ObjectList = 1 });
                    command.Add(new UpFindStatusRemote() { TypeOp1 = TypeOp.C, UnitId = -1, TypeOp2 = TypeOp.C, Count = 2 });

                    command.Add(new UpFilterDistance() { TypeOp1 = TypeOp.C, MinDistance = range - 1, TypeOp2 = TypeOp.C, MaxDistance = -1 });

                    command.Add(new UpFilterInclude() { CmdId = (int)CmdId.MILITARY_BUILDING, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Add(new UpFilterStatus() { TypeOp1 = TypeOp.C, ObjectStatus = 2, TypeOp2 = TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindStatusRemote() { TypeOp1 = TypeOp.C, UnitId = -1, TypeOp2 = TypeOp.C, Count = 8 });
                    command.Add(new UpFilterStatus() { TypeOp1 = TypeOp.C, ObjectStatus = 2, TypeOp2 = TypeOp.C, ObjectList = 1 });
                    command.Add(new UpFindStatusRemote() { TypeOp1 = TypeOp.C, UnitId = -1, TypeOp2 = TypeOp.C, Count = 8 });
                    command.Add(new UpFilterStatus() { TypeOp1 = TypeOp.C, ObjectStatus = 0, TypeOp2 = TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindStatusRemote() { TypeOp1 = TypeOp.C, UnitId = -1, TypeOp2 = TypeOp.C, Count = 2 });
                    command.Add(new UpFilterStatus() { TypeOp1 = TypeOp.C, ObjectStatus = 0, TypeOp2 = TypeOp.C, ObjectList = 1 });
                    command.Add(new UpFindStatusRemote() { TypeOp1 = TypeOp.C, UnitId = -1, TypeOp2 = TypeOp.C, Count = 2 });

                    command.Add(new UpFilterInclude() { CmdId = (int)CmdId.CIVILIAN_BUILDING, ActionId = -1, OrderId = -1, OnMainland = -1 });
                    command.Add(new UpFilterStatus() { TypeOp1 = TypeOp.C, ObjectStatus = 2, TypeOp2 = TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindStatusRemote() { TypeOp1 = TypeOp.C, UnitId = -1, TypeOp2 = TypeOp.C, Count = 8 });
                    command.Add(new UpFilterStatus() { TypeOp1 = TypeOp.C, ObjectStatus = 2, TypeOp2 = TypeOp.C, ObjectList = 1 });
                    command.Add(new UpFindStatusRemote() { TypeOp1 = TypeOp.C, UnitId = -1, TypeOp2 = TypeOp.C, Count = 8 });
                    command.Add(new UpFilterStatus() { TypeOp1 = TypeOp.C, ObjectStatus = 0, TypeOp2 = TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindStatusRemote() { TypeOp1 = TypeOp.C, UnitId = -1, TypeOp2 = TypeOp.C, Count = 2 });
                    command.Add(new UpFilterStatus() { TypeOp1 = TypeOp.C, ObjectStatus = 0, TypeOp2 = TypeOp.C, ObjectList = 1 });
                    command.Add(new UpFindStatusRemote() { TypeOp1 = TypeOp.C, UnitId = -1, TypeOp2 = TypeOp.C, Count = 2 });

                    break;

                case UnitFindType.WOOD:

                    command.Add(new UpFilterDistance() { TypeOp1 = TypeOp.C, MinDistance = -1, TypeOp2 = TypeOp.C, MaxDistance = range });
                    command.Add(new UpFilterStatus() { TypeOp1 = TypeOp.C, ObjectStatus = 2, TypeOp2 = TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindResource() { TypeOp1 = TypeOp.C, Resource = (int)Resource.WOOD, TypeOp2 = TypeOp.C, Count = 30 });
                    command.Add(new UpFilterStatus() { TypeOp1 = TypeOp.C, ObjectStatus = 3, TypeOp2 = TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindResource() { TypeOp1 = TypeOp.C, Resource = (int)Resource.WOOD, TypeOp2 = TypeOp.C, Count = 10 });

                    command.Add(new UpFilterDistance() { TypeOp1 = TypeOp.C, MinDistance = range - 1, TypeOp2 = TypeOp.C, MaxDistance = -1 });
                    command.Add(new UpFilterStatus() { TypeOp1 = TypeOp.C, ObjectStatus = 3, TypeOp2 = TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindResource() { TypeOp1 = TypeOp.C, Resource = (int)Resource.WOOD, TypeOp2 = TypeOp.C, Count = 10 });
                    command.Add(new UpFilterStatus() { TypeOp1 = TypeOp.C, ObjectStatus = 2, TypeOp2 = TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindResource() { TypeOp1 = TypeOp.C, Resource = (int)Resource.WOOD, TypeOp2 = TypeOp.C, Count = 40 });

                    break;

                case UnitFindType.FOOD:

                    command.Add(new UpFilterDistance() { TypeOp1 = TypeOp.C, MinDistance = -1, TypeOp2 = TypeOp.C, MaxDistance = range });
                    command.Add(new UpFilterStatus() { TypeOp1 = TypeOp.C, ObjectStatus = 2, TypeOp2 = TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindResource() { TypeOp1 = TypeOp.C, Resource = (int)Resource.FOOD, TypeOp2 = TypeOp.C, Count = 30 });
                    command.Add(new UpFilterStatus() { TypeOp1 = TypeOp.C, ObjectStatus = 3, TypeOp2 = TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindResource() { TypeOp1 = TypeOp.C, Resource = (int)Resource.FOOD, TypeOp2 = TypeOp.C, Count = 10 });

                    command.Add(new UpFilterDistance() { TypeOp1 = TypeOp.C, MinDistance = range - 1, TypeOp2 = TypeOp.C, MaxDistance = -1 });
                    command.Add(new UpFilterStatus() { TypeOp1 = TypeOp.C, ObjectStatus = 3, TypeOp2 = TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindResource() { TypeOp1 = TypeOp.C, Resource = (int)Resource.FOOD, TypeOp2 = TypeOp.C, Count = 10 });
                    command.Add(new UpFilterStatus() { TypeOp1 = TypeOp.C, ObjectStatus = 2, TypeOp2 = TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindResource() { TypeOp1 = TypeOp.C, Resource = (int)Resource.FOOD, TypeOp2 = TypeOp.C, Count = 40 });

                    break;

                case UnitFindType.GOLD:

                    command.Add(new UpFilterDistance() { TypeOp1 = TypeOp.C, MinDistance = -1, TypeOp2 = TypeOp.C, MaxDistance = range });
                    command.Add(new UpFilterStatus() { TypeOp1 = TypeOp.C, ObjectStatus = 3, TypeOp2 = TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindResource() { TypeOp1 = TypeOp.C, Resource = (int)Resource.GOLD, TypeOp2 = TypeOp.C, Count = 40 });

                    command.Add(new UpFilterDistance() { TypeOp1 = TypeOp.C, MinDistance = range - 1, TypeOp2 = TypeOp.C, MaxDistance = -1 });
                    command.Add(new UpFilterStatus() { TypeOp1 = TypeOp.C, ObjectStatus = 3, TypeOp2 = TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindResource() { TypeOp1 = TypeOp.C, Resource = (int)Resource.GOLD, TypeOp2 = TypeOp.C, Count = 40 });

                    break;

                case UnitFindType.STONE:

                    command.Add(new UpFilterDistance() { TypeOp1 = TypeOp.C, MinDistance = -1, TypeOp2 = TypeOp.C, MaxDistance = range });
                    command.Add(new UpFilterStatus() { TypeOp1 = TypeOp.C, ObjectStatus = 3, TypeOp2 = TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindResource() { TypeOp1 = TypeOp.C, Resource = (int)Resource.STONE, TypeOp2 = TypeOp.C, Count = 40 });

                    command.Add(new UpFilterDistance() { TypeOp1 = TypeOp.C, MinDistance = range - 1, TypeOp2 = TypeOp.C, MaxDistance = -1 });
                    command.Add(new UpFilterStatus() { TypeOp1 = TypeOp.C, ObjectStatus = 3, TypeOp2 = TypeOp.C, ObjectList = 0 });
                    command.Add(new UpFindResource() { TypeOp1 = TypeOp.C, Resource = (int)Resource.STONE, TypeOp2 = TypeOp.C, Count = 40 });

                    break;

                case UnitFindType.ALL:

                    command.Add(new UpFilterDistance() { TypeOp1 = TypeOp.C, MinDistance = -1, TypeOp2 = TypeOp.C, MaxDistance = range });
                    command.Add(new UpFilterExclude() { CmdId = (int)CmdId.MILITARY, ActionId = -1, OrderId = -1, ClassId = (int)UnitClass.Tree });
                    command.Add(new UpFindRemote() { TypeOp1 = TypeOp.C, UnitId = -1, TypeOp2 = TypeOp.C, Count = 40 });

                    command.Add(new UpFilterDistance() { TypeOp1 = TypeOp.C, MinDistance = range - 1, TypeOp2 = TypeOp.C, MaxDistance = -1 });
                    command.Add(new UpFilterExclude() { CmdId = (int)CmdId.MONK, ActionId = -1, OrderId = -1, ClassId = (int)UnitClass.Tree });
                    command.Add(new UpFindRemote() { TypeOp1 = TypeOp.C, UnitId = -1, TypeOp2 = TypeOp.C, Count = 40 });

                    break;

                default: throw new NotImplementedException();
            }

            command.Add(new UpGetSearchState() { GoalState = 100 }); // remote-total = 102
            command.Add(new UpModifyGoal() { GoalId = 102, MathOp = MathOp.C_MAX, Value = 1 });
            command.Add(new Goal() { GoalId = 102 });

            for (int i = 0; i < 40; i++)
            {
                command.Add(new SetGoal() { GoalId = 200, GoalValue = i });
                command.Add(new UpModifyGoal() { GoalId = 200, MathOp = MathOp.G_MOD, Value = 102 });
                command.Add(new Goal() { GoalId = 200 });
                command.Add(new UpSetTargetObject() { SearchSource = 2, TypeOp = TypeOp.G, Index = 200 });
                command.Add(new UpObjectData() { ObjectData = (int)ObjectData.ID });
            }

            UnitFindCommands.Add(command);
        }

        public void Train(UnitDef unit, int max = int.MaxValue, int concurrent = int.MaxValue, int priority = 0)
        {
            Debug.Assert(!unit.IsBuilding);
            Create(unit, Position.FromPoint(-1, -1), max, concurrent, priority);
        }

        public void Build(UnitDef unit, Position position, int max = int.MaxValue, int concurrent = int.MaxValue, int priority = 0)
        {
            Debug.Assert(unit.IsBuilding);
            Create(unit, position, max, concurrent, priority);
        }

        private void Create(UnitDef unit, Position position, int max = int.MaxValue, int concurrent = int.MaxValue, int priority = 0)
        {
            var info = Bot.GetModule<InfoModule>();
            info.AddUnitType(unit);

            var type = info.UnitTypes[unit.Id];
            type.RequestUpdate();

            if (!type.Updated)
            {
                Bot.Log.Info($"UnitsModule: type not updated {unit.Id} {unit.Name}");
                return;
            }
            else if (!type.IsAvailable)
            {
                Bot.Log.Info($"UnitsModule: type not availabe {unit.Id} {unit.Name}");
                return;
            }
            else if (type.CountTotal >= max)
            {
                Bot.Log.Info($"UnitsModule: type over max {unit.Id} {unit.Name}");
                return;
            }
            else if (type.Pending >= concurrent)
            {
                Bot.Log.Info($"UnitsModule: type over concurrent {unit.Id} {unit.Name}");
                return;
            }
            else if (type.IsBuilding)
            {
                var map = Bot.GetModule<MapModule>();
                if (!map.IsOnMap(position))
                {
                    Bot.Log.Info($"UnitsModule: type not on map {unit.Id} {unit.Name}");
                    return;
                }

                if (!map.GetTile(position).Explored)
                {
                    Bot.Log.Info($"UnitsModule: type not explored {unit.Id} {unit.Name}");
                    return;
                }
            }

            var command = new SpendingCommand()
            {
                Name = $"Create {unit.Id} {unit.Name}",
                Priority = priority,
                WoodCost = type.WoodCost,
                FoodCost = type.FoodCost,
                GoldCost = type.GoldCost,
                StoneCost = type.StoneCost
            };

            if (type.CanCreate)
            {
                var tick = -1;
                if (LastBuildTick.ContainsKey(type.Id))
                {
                    tick = LastBuildTick[type.Id];
                }

                if (Bot.Tick > tick + 1 || (concurrent > type.Pending + 1 && max > type.CountTotal + 1))
                {
                    if (type.IsBuilding)
                    {
                        command.Add(new SetGoal() { GoalId = 100, GoalValue = position.PointX });
                        command.Add(new SetGoal() { GoalId = 101, GoalValue = position.PointY });
                        command.Add(new UpBuildLine() { TypeOp = TypeOp.C, BuildingId = unit.FoundationId, GoalPoint1 = 100, GoalPoint2 = 100 });

                        Bot.Log.Info($"UnitsModule: Building type {unit.Id} {unit.Name} {unit.FoundationId} at {position.PointX} {position.PointY}");
                    }
                    else
                    {
                        command.Add(new Train() { UnitType = unit.FoundationId });

                        Bot.Log.Info($"UnitsModule: Training type {unit.Id} {unit.Name} {unit.FoundationId}");
                    }

                    LastBuildTick[type.Id] = Bot.Tick;
                }
            }
            else
            {
                Bot.Log.Info($"UnitsModule: Can not create type {unit.Id} {unit.Name} {unit.FoundationId}");
            }

            Bot.GetModule<SpendingModule>().Add(command);
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
            var info = Bot.GetModule<InfoModule>();
            foreach (var unit in Units.Values)
            {
                unit.Update();

                if (unit.UnitType == null)
                {
                    var id = unit.TypeId;

                    if (!info.UnitTypes.ContainsKey(id))
                    {
                        if (Bot.Mod.UnitDefs.TryGetValue(id, out UnitDef def))
                        {
                            info.AddUnitType(def);
                        }
                    }

                    if (info.UnitTypes.TryGetValue(id, out UnitType type))
                    {
                        unit.UnitType = type;
                    }
                }
            }

            foreach (var command in UnitFindCommands.Where(c => c.HasResponses)
            {
                var responses = command.GetResponses();
                for (int i = 0; i < 40; i++)
                {
                    var index = responses.Count - (5 * i) - 1;
                    var id = responses[index].Unpack<UpObjectDataResult>().Result;

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
            // find = 300 us
            // update = 50 us
            const int NUM_FINDS = 5;
            const int NUM_UPDATES = 20;

            if (AutoUpdateUnits)
            {
                var units = Units.Values.ToList();
                units.Sort((a, b) => a.LastUpdateGameTime.CompareTo(b.LastUpdateGameTime));

                for (int i = 0; i < Math.Min(units.Count, NUM_UPDATES / 2); i++)
                {
                    units[i].RequestUpdate();
                    units[RNG.Next(units.Count)].RequestUpdate();
                }
            }

            if (!AutoFindUnits)
            {
                return;
            }

            var explored = Bot.GetModule<MapModule>().GetTiles().Where(t => t.Explored).Select(t => t.Position).ToList();
            if (explored.Count == 0)
            {
                explored.Add(Position.FromPoint(0, 0));
            }

            var gametime = Bot.GetModule<InfoModule>().GameTime;
            var players = Bot.GetModule<PlayersModule>().Players;

            for (int i = 0; i < NUM_FINDS; i++)
            {
                var player = Bot.PlayerNumber;

                if (RNG.NextDouble() < 0.5)
                {
                    player = 0;

                    if (RNG.NextDouble() < 0.5 && players.Count > 0)
                    {
                        player = players.Values.ElementAt(RNG.Next(players.Count)).PlayerNumber;
                    }
                }

                var type = UnitFindType.MILLITARY;

                if (RNG.NextDouble() < 0.5)
                {
                    type = UnitFindType.CIVILIAN;

                    if (RNG.NextDouble() < 0.5)
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

                FindUnits(position, 10, player, type);
            }

            if (Bot.Tick < 3)
            {
                var pos = Bot.GetModule<InfoModule>().MyPosition;
                var player = Bot.PlayerNumber;

                FindUnits(pos, 20, player, UnitFindType.CIVILIAN);
                FindUnits(pos, 20, player, UnitFindType.MILLITARY);
                FindUnits(pos, 20, player, UnitFindType.BUILDING);
                FindUnits(pos, 20, player, UnitFindType.FOOD);
                FindUnits(pos, 20, 0, UnitFindType.FOOD);
            }
        }
    }
}
