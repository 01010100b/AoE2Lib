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

namespace AoE2Lib.Bots.Modules
{
    public class InfoModule : Module
    {
        public TimeSpan GameTime { get; private set; } = TimeSpan.Zero;
        public double GameSecondsPerTick { get; private set; } = 1;
        public int PlayerNumber => Bot.PlayerNumber;
        public Position MyPosition { get; private set; }
        public int WoodAmount { get; private set; } = -1;
        public int FoodAmount { get; private set; } = -1;
        public int GoldAmount { get; private set; } = -1;
        public int StoneAmount { get; private set; } = -1;
        public int PopulationHeadroom { get; private set; } = -1;
        public int HousingHeadroom { get; private set; } = -1;
        public int PopulationCap { get; private set; } = -1;

        public readonly Dictionary<StrategicNumber, int> StrategicNumbers = new Dictionary<StrategicNumber, int>();
        public IReadOnlyDictionary<int, UnitType> UnitTypes => _UnitTypes;
        private readonly Dictionary<int, UnitType> _UnitTypes = new Dictionary<int, UnitType>();

        private readonly Command Command = new Command();

        public void AddUnitType(UnitDef unit)
        {
            if (!UnitTypes.ContainsKey(unit.Id))
            {
                _UnitTypes.Add(unit.Id, new UnitType(Bot, unit));
                Bot.Log.Info($"InfoModule: Added unit {unit.Id}");
            }
        }

        protected override IEnumerable<Command> RequestUpdate()
        {
            Command.Reset();

            Command.Add(new GameTime());
            Command.Add(new UpGetPoint() { GoalPoint = 50, PositionType = (int)PositionType.SELF });
            Command.Add(new Goal() { GoalId = 50 });
            Command.Add(new Goal() { GoalId = 51 });
            Command.Add(new WoodAmount());
            Command.Add(new FoodAmount());
            Command.Add(new GoldAmount());
            Command.Add(new StoneAmount());
            Command.Add(new PopulationHeadroom());
            Command.Add(new HousingHeadroom());
            Command.Add(new PopulationCap());

            foreach (var sn in StrategicNumbers)
            {
                Command.Add(new SetStrategicNumber() { StrategicNumber = (int)sn.Key, Value = sn.Value });
            }

            foreach (var sn in Enum.GetValues(typeof(StrategicNumber)).Cast<StrategicNumber>())
            {
                Command.Add(new Protos.Expert.Fact.StrategicNumber() { StrategicNumber_ = (int)sn });
            }

            yield return Command;

            foreach (var info in UnitTypes.Values)
            {
                info.RequestUpdate();
                yield return info.Command;
            }
        }

        protected override void Update()
        {
            var responses = Command.GetResponses();
            if (responses.Count > 0)
            {
                var current_time = GameTime;

                GameTime = TimeSpan.FromSeconds(responses[0].Unpack<GameTimeResult>().Result);
                GameSecondsPerTick *= 19;
                GameSecondsPerTick += (GameTime - current_time).TotalSeconds;
                GameSecondsPerTick /= 20;

                var x = responses[2].Unpack<GoalResult>().Result;
                var y = responses[3].Unpack<GoalResult>().Result;
                MyPosition = Position.FromPoint(x, y);

                WoodAmount = responses[4].Unpack<WoodAmountResult>().Result;
                FoodAmount = responses[5].Unpack<FoodAmountResult>().Result;
                GoldAmount = responses[6].Unpack<GoldAmountResult>().Result;
                StoneAmount = responses[7].Unpack<StoneAmountResult>().Result;

                PopulationHeadroom = responses[8].Unpack<PopulationHeadroomResult>().Result;
                HousingHeadroom = responses[9].Unpack<HousingHeadroomResult>().Result;
                PopulationCap = responses[10].Unpack<PopulationCapResult>().Result;

                var index = 11 + StrategicNumbers.Count;
                foreach (var sn in Enum.GetValues(typeof(StrategicNumber)).Cast<StrategicNumber>())
                {
                    StrategicNumbers[sn] = responses[index].Unpack<StrategicNumberResult>().Result;
                    index++;
                }
            }

            Command.Reset();

            foreach (var info in UnitTypes.Values)
            {
                info.Update();
            }
        }
    }
}
