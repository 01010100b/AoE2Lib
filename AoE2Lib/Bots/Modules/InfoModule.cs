using AoE2Lib.Bots.GameElements;
using AoE2Lib.Utils;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AoE2Lib.Bots.Modules
{
    public class InfoModule : Module
    {
        public TimeSpan GameTime { get; private set; } = TimeSpan.Zero;
        public TimeSpan GameTimePerTick { get; private set; } = TimeSpan.FromSeconds(0.7);
        public int PlayerNumber => Bot.PlayerNumber;
        public Position MyPosition { get; private set; }
        public int WoodAmount { get; private set; }
        public int FoodAmount { get; private set; }
        public int GoldAmount { get; private set; }
        public int StoneAmount { get; private set; }
        public int WoodEscrowAmount { get; private set; }
        public int FoodEscrowAmount { get; private set; }
        public int GoldEscrowAmount { get; private set; }
        public int StoneEscrowAmount { get; private set; }
        public int PopulationHeadroom { get; private set; }
        public int HousingHeadroom { get; private set; }
        public int PopulationCap { get; private set; }
        public readonly Dictionary<Resource, bool> ResourceFound = new Dictionary<Resource, bool>();
        public readonly Dictionary<Resource, int> DropsiteMinDistance = new Dictionary<Resource, int>();
        public readonly Dictionary<StrategicNumber, int> StrategicNumbers = new Dictionary<StrategicNumber, int>();

        private readonly Command CommandInfo = new Command();
        private readonly double[] TickTimes = new double[] { 0.7, 0.7, 0.7, 0.7, 0.7, 0.7, 0.7, 0.7, 0.7, 0.7 };

        internal InfoModule() : base()
        {
            foreach (var resource in new[] { Resource.FOOD, Resource.WOOD, Resource.GOLD, Resource.STONE })
            {
                ResourceFound[resource] = false;
            }

            foreach (var resource in Enum.GetValues(typeof(Resource)).Cast<Resource>())
            {
                DropsiteMinDistance[resource] = -1;
            }
        }

        internal bool GetResourceFound(Resource resource)
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

        internal int GetDropsiteMinDistance(Resource resource)
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

        internal int GetStrategicNumber(StrategicNumber sn)
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

        internal void SetStrategicNumber(StrategicNumber sn, int val)
        {
            StrategicNumbers[sn] = val;
        }

        protected override IEnumerable<Command> RequestUpdate()
        {
            CommandInfo.Reset();

            CommandInfo.Add(new GameTime());
            CommandInfo.Add(new UpGetPoint() { OutGoalPoint = 50, InConstPositionType = (int)PositionType.SELF });
            CommandInfo.Add(new Goal() { InConstGoalId = 50 });
            CommandInfo.Add(new Goal() { InConstGoalId = 51 });

            CommandInfo.Add(new WoodAmount());
            CommandInfo.Add(new FoodAmount());
            CommandInfo.Add(new GoldAmount());
            CommandInfo.Add(new StoneAmount());

            CommandInfo.Add(new EscrowAmount() { InConstResource = (int)Resource.WOOD });
            CommandInfo.Add(new EscrowAmount() { InConstResource = (int)Resource.FOOD });
            CommandInfo.Add(new EscrowAmount() { InConstResource = (int)Resource.GOLD });
            CommandInfo.Add(new EscrowAmount() { InConstResource = (int)Resource.STONE });

            CommandInfo.Add(new PopulationHeadroom());
            CommandInfo.Add(new HousingHeadroom());
            CommandInfo.Add(new PopulationCap());

            foreach (var resource in new[] {Resource.FOOD, Resource.WOOD, Resource.GOLD, Resource.STONE})
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

        protected override void Update()
        {
            if (CommandInfo.HasResponses)
            {
                var responses = CommandInfo.GetResponses();

                var current_time = GameTime;
                GameTime = TimeSpan.FromSeconds(responses[0].Unpack<GameTimeResult>().Result);

                TickTimes[Bot.Tick % TickTimes.Length] = (GameTime - current_time).TotalSeconds;
                GameTimePerTick += TimeSpan.FromSeconds(TickTimes.Average());
                GameTimePerTick /= 2;

                var x = responses[2].Unpack<GoalResult>().Result;
                var y = responses[3].Unpack<GoalResult>().Result;
                var pos = Position.FromPoint(x, y);
                if (pos.PointX >= 0 && pos.PointY >= 0 && pos.PointX < Bot.GameState.Map.Width && pos.PointY < Bot.GameState.Map.Height)
                {
                    MyPosition = pos;
                }

                WoodAmount = responses[4].Unpack<WoodAmountResult>().Result;
                FoodAmount = responses[5].Unpack<FoodAmountResult>().Result;
                GoldAmount = responses[6].Unpack<GoldAmountResult>().Result;
                StoneAmount = responses[7].Unpack<StoneAmountResult>().Result;

                WoodEscrowAmount = responses[8].Unpack<EscrowAmountResult>().Result;
                FoodEscrowAmount = responses[9].Unpack<EscrowAmountResult>().Result;
                GoldEscrowAmount = responses[10].Unpack<EscrowAmountResult>().Result;
                StoneEscrowAmount = responses[11].Unpack<EscrowAmountResult>().Result;

                PopulationHeadroom = responses[12].Unpack<PopulationHeadroomResult>().Result;
                HousingHeadroom = responses[13].Unpack<HousingHeadroomResult>().Result;
                PopulationCap = responses[14].Unpack<PopulationCapResult>().Result;

                var index = 15;
                foreach (var resource in new[] { Resource.FOOD, Resource.WOOD, Resource.GOLD, Resource.STONE })
                {
                    ResourceFound[resource] = responses[index].Unpack<ResourceFoundResult>().Result;
                    index++;
                }

                foreach (var resource in Enum.GetValues(typeof(Resource)).Cast<Resource>())
                {
                    DropsiteMinDistance[resource] = responses[index].Unpack<DropsiteMinDistanceResult>().Result;
                    index++;
                }

                foreach (var sn in StrategicNumbers)
                {
                    index++;
                }

                foreach (var sn in Enum.GetValues(typeof(StrategicNumber)).Cast<StrategicNumber>())
                {
                    StrategicNumbers[sn] = responses[index].Unpack<StrategicNumberResult>().Result;
                    index++;
                }
            }
        }
    }
}
