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
        public int Civilization { get; private set; } = -1;
        public int Score { get; private set; } = -1;
        public int Age { get; private set; } = -1;
        public int Population => CivilianPopulation + MilitaryPopulation;
        public int CivilianPopulation { get; private set; } = -1;
        public int MilitaryPopulation { get; private set; } = -1;
        public int WoodAmount { get; private set; } = -1;
        public int FoodAmount { get; private set; } = -1;
        public int GoldAmount { get; private set; } = -1;
        public int StoneAmount { get; private set; } = -1;
        public PlayerStance Stance { get; private set; } = PlayerStance.NEUTRAL;
        
        internal Player(Bot bot, int player) : base(bot)
        {
            PlayerNumber = player;
        }
        
        protected override IEnumerable<IMessage> RequestElementUpdate()
        {
            yield return new PlayerInGame() { PlayerNumber = PlayerNumber };
            yield return new UpGetPlayerFact() { Player = PlayerNumber, FactId = (int)FactId.CIVILIZATION, Param = 0, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetPlayerFact() { Player = PlayerNumber, FactId = (int)FactId.CURRENT_SCORE, Param = 0, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetPlayerFact() { Player = PlayerNumber, FactId = (int)FactId.CURRENT_AGE, Param = 0, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetPlayerFact() { Player = PlayerNumber, FactId = (int)FactId.CIVILIAN_POPULATION, Param = 0, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetPlayerFact() { Player = PlayerNumber, FactId = (int)FactId.MILITARY_POPULATION, Param = 0, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetPlayerFact() { Player = PlayerNumber, FactId = (int)FactId.WOOD_AMOUNT, Param = 0, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetPlayerFact() { Player = PlayerNumber, FactId = (int)FactId.FOOD_AMOUNT, Param = 0, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetPlayerFact() { Player = PlayerNumber, FactId = (int)FactId.GOLD_AMOUNT, Param = 0, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetPlayerFact() { Player = PlayerNumber, FactId = (int)FactId.STONE_AMOUNT, Param = 0, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new StanceToward() { PlayerNumber = PlayerNumber, Stance = (int)PlayerStance.ALLY };
            yield return new StanceToward() { PlayerNumber = PlayerNumber, Stance = (int)PlayerStance.ENEMY };
            yield return new StanceToward() { PlayerNumber = PlayerNumber, Stance = (int)PlayerStance.NEUTRAL };
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
