using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots.GameElements
{
    public class Player : GameElement
    {
        public readonly int PlayerNumber;
        public bool InGame { get; private set; } = false;
        public int Civilization { get; private set; }
        public int Score { get; private set; }
        public int Age { get; private set; }
        public int CivilianPopulation { get; private set; }
        public int MilitaryPopulation { get; private set; }
        public int WoodAmount { get; private set; }
        public int FoodAmount { get; private set; }
        public int GoldAmount { get; private set; }
        public int StoneAmount { get; private set; }
        public PlayerStance Stance { get; private set; } = PlayerStance.NEUTRAL;
        public int Population => CivilianPopulation + MilitaryPopulation;
        public IEnumerable<Unit> Units => UnitsInternal;
        
        internal readonly List<Unit> UnitsInternal = new List<Unit>();

        internal Player(Bot bot, int player) : base(bot)
        {
            PlayerNumber = player;
        }
        
        protected override IEnumerable<IMessage> RequestElementUpdate()
        {
            // TODO use UpPlayerFact instead
            yield return new PlayerInGame() { InPlayerAnyPlayer = PlayerNumber };
            yield return new UpGetPlayerFact() { InPlayerAnyPlayer = PlayerNumber, InConstFactId = (int)FactId.CIVILIZATION, InConstParam = 0, OutGoalData = 100 };
            yield return new Goal() { InConstGoalId = 100 };
            yield return new UpGetPlayerFact() { InPlayerAnyPlayer = PlayerNumber, InConstFactId = (int)FactId.CURRENT_SCORE, InConstParam = 0, OutGoalData = 100 };
            yield return new Goal() { InConstGoalId = 100 };
            yield return new UpGetPlayerFact() { InPlayerAnyPlayer = PlayerNumber, InConstFactId = (int)FactId.CURRENT_AGE, InConstParam = 0, OutGoalData = 100 };
            yield return new Goal() { InConstGoalId = 100 };
            yield return new UpGetPlayerFact() { InPlayerAnyPlayer = PlayerNumber, InConstFactId = (int)FactId.CIVILIAN_POPULATION, InConstParam = 0, OutGoalData = 100 };
            yield return new Goal() { InConstGoalId = 100 };
            yield return new UpGetPlayerFact() { InPlayerAnyPlayer = PlayerNumber, InConstFactId = (int)FactId.MILITARY_POPULATION, InConstParam = 0, OutGoalData = 100 };
            yield return new Goal() { InConstGoalId = 100 };
            yield return new UpGetPlayerFact() { InPlayerAnyPlayer = PlayerNumber, InConstFactId = (int)FactId.WOOD_AMOUNT, InConstParam = 0, OutGoalData = 100 };
            yield return new Goal() { InConstGoalId = 100 };
            yield return new UpGetPlayerFact() { InPlayerAnyPlayer = PlayerNumber, InConstFactId = (int)FactId.FOOD_AMOUNT, InConstParam = 0, OutGoalData = 100 };
            yield return new Goal() { InConstGoalId = 100 };
            yield return new UpGetPlayerFact() { InPlayerAnyPlayer = PlayerNumber, InConstFactId = (int)FactId.GOLD_AMOUNT, InConstParam = 0, OutGoalData = 100 };
            yield return new Goal() { InConstGoalId = 100 };
            yield return new UpGetPlayerFact() { InPlayerAnyPlayer = PlayerNumber, InConstFactId = (int)FactId.STONE_AMOUNT, InConstParam = 0, OutGoalData = 100 };
            yield return new Goal() { InConstGoalId = 100 };
            yield return new StanceToward() { InPlayerAnyPlayer = PlayerNumber, InConstESPlayerStance = (int)PlayerStance.ALLY };
            yield return new StanceToward() { InPlayerAnyPlayer = PlayerNumber, InConstESPlayerStance = (int)PlayerStance.ENEMY };
            yield return new StanceToward() { InPlayerAnyPlayer = PlayerNumber, InConstESPlayerStance = (int)PlayerStance.NEUTRAL };
        }

        protected override void UpdateElement(IReadOnlyList<Any> responses)
        {
            InGame = responses[0].Unpack<PlayerInGameResult>().Result;
            Civilization = responses[2].Unpack<GoalResult>().Result;
            Score = responses[4].Unpack<GoalResult>().Result;
            Age = responses[6].Unpack<GoalResult>().Result;
            CivilianPopulation = responses[8].Unpack<GoalResult>().Result;
            MilitaryPopulation = responses[10].Unpack<GoalResult>().Result;
            WoodAmount = responses[12].Unpack<GoalResult>().Result;
            FoodAmount = responses[14].Unpack<GoalResult>().Result;
            GoldAmount = responses[16].Unpack<GoalResult>().Result;
            StoneAmount = responses[18].Unpack<GoalResult>().Result;

            if (responses[19].Unpack<StanceTowardResult>().Result)
            {
                Stance = PlayerStance.ALLY;
            }
            else if (responses[20].Unpack<StanceTowardResult>().Result)
            {
                Stance = PlayerStance.ENEMY;
            }
            else if (responses[21].Unpack<StanceTowardResult>().Result)
            {
                Stance = PlayerStance.NEUTRAL;
            }
        }
    }
}
