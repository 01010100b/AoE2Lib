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
        public readonly Dictionary<StrategicNumber, int> StrategicNumbers = new Dictionary<StrategicNumber, int>();

        private readonly Command CommandInfo = new Command();
        private readonly Command CommandSn = new Command();
        private readonly double[] TickTimes = new double[] { 0.7, 0.7, 0.7, 0.7, 0.7, 0.7, 0.7, 0.7, 0.7, 0.7 };

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

            foreach (var sn in StrategicNumbers)
            {
                CommandInfo.Add(new SetStrategicNumber() { InConstSnId = (int)sn.Key, InConstValue = sn.Value });
            }

            yield return CommandInfo;

            CommandSn.Reset();

            foreach (var sn in Enum.GetValues(typeof(StrategicNumber)).Cast<StrategicNumber>())
            {
                CommandSn.Add(new Protos.Expert.Fact.StrategicNumber() { InConstSnId = (int)sn });
            }

            yield return CommandSn;
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
                if (Bot.MapModule.IsOnMap(pos))
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
            }

            if (CommandSn.HasResponses)
            {
                var responses = CommandSn.GetResponses();

                var index = 0;
                foreach (var sn in Enum.GetValues(typeof(StrategicNumber)).Cast<StrategicNumber>())
                {
                    StrategicNumbers[sn] = responses[index].Unpack<StrategicNumberResult>().Result;
                    index++;
                }
            }
        }
    }
}
