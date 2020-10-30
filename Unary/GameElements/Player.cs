using Unary.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Protos.Expert.Fact;
using Protos.Expert.Action;

namespace Unary.GameElements
{
    public class Player : GameElement
    {
        public enum PlayerStance
        {
            ALLY, NEUTRAL, ALL, ENEMY
        }

        public readonly int PlayerNumber;
        public bool IsValid { get; private set; } = false;
        public bool InGame { get; private set; } = false;
        public int Civilization { get; private set; } = -1;
        public int Score { get; private set; } = -1;
        public int Age { get; private set; } = -1;
        public int CivilianPopulation { get; private set; } = -1;
        public int MilitaryPopulation { get; private set; } = -1;
        public int WoodAmount { get; internal set; } = -1;
        public int FoodAmount { get; internal set; } = -1;
        public int GoldAmount { get; internal set; } = -1;
        public int StoneAmount { get; internal set; } = -1;
        public int PopulationHeadroom { get; internal set; } = -1;
        public int HousingHeadroom { get; internal set; } = -1;
        public PlayerStance Stance { get; private set; } = PlayerStance.NEUTRAL;

        public Player(int player) : base()
        {
            PlayerNumber = player;
        }

        protected override void UpdateElement(List<Any> responses)
        {
            IsValid = responses[0].Unpack<PlayerValidResult>().Result;
            InGame = responses[1].Unpack<PlayerInGameResult>().Result;
            Civilization = responses[3].Unpack<GoalResult>().Result;
            Score = responses[5].Unpack<GoalResult>().Result;
            Age = responses[7].Unpack<GoalResult>().Result;
            CivilianPopulation = responses[9].Unpack<GoalResult>().Result;
            MilitaryPopulation = responses[11].Unpack<GoalResult>().Result;
            WoodAmount = responses[13].Unpack<GoalResult>().Result;
            FoodAmount = responses[15].Unpack<GoalResult>().Result;
            GoldAmount = responses[17].Unpack<GoalResult>().Result;
            StoneAmount = responses[19].Unpack<GoalResult>().Result;
            PopulationHeadroom = responses[21].Unpack<GoalResult>().Result;
            HousingHeadroom = responses[23].Unpack<GoalResult>().Result;

            if (responses[24].Unpack<PlayersStanceResult>().Result)
            {
                Stance = PlayerStance.ALLY;
            }
            else if (responses[25].Unpack<PlayersStanceResult>().Result)
            {
                Stance = PlayerStance.NEUTRAL;
            }
            else if (responses[26].Unpack<PlayersStanceResult>().Result)
            {
                Stance = PlayerStance.ENEMY;
            }
        }

        protected override IEnumerable<IMessage> RequestElementUpdate()
        {
            var messages = new List<IMessage>()
            {
                new PlayerValid() {PlayerNumber = PlayerNumber},
                new PlayerInGame() { PlayerNumber = PlayerNumber },
                new UpGetPlayerFact() { Player = PlayerNumber, FactId = (int)FactId.CIVILIZATION, Param = 0, GoalData = 100 },
                new Goal() { GoalId = 100 },
                new UpGetPlayerFact() { Player = PlayerNumber, FactId = (int)FactId.CURRENT_SCORE, Param = 0, GoalData = 100 },
                new Goal() { GoalId = 100 },
                new UpGetPlayerFact() { Player = PlayerNumber, FactId = (int)FactId.CURRENT_AGE, Param = 0, GoalData = 100 },
                new Goal() { GoalId = 100 },
                new UpGetPlayerFact() { Player = PlayerNumber, FactId = (int)FactId.CIVILIAN_POPULATION, Param = 0, GoalData = 100 },
                new Goal() { GoalId = 100 },
                new UpGetPlayerFact() { Player = PlayerNumber, FactId = (int)FactId.MILITARY_POPULATION, Param = 0, GoalData = 100 },
                new Goal() { GoalId = 100 },
                new UpGetPlayerFact() { Player = PlayerNumber, FactId = (int)FactId.WOOD_AMOUNT, Param = 0, GoalData = 100 },
                new Goal() { GoalId = 100 },
                new UpGetPlayerFact() { Player = PlayerNumber, FactId = (int)FactId.FOOD_AMOUNT, Param = 0, GoalData = 100 },
                new Goal() { GoalId = 100 },
                new UpGetPlayerFact() { Player = PlayerNumber, FactId = (int)FactId.GOLD_AMOUNT, Param = 0, GoalData = 100 },
                new Goal() { GoalId = 100 },
                new UpGetPlayerFact() { Player = PlayerNumber, FactId = (int)FactId.STONE_AMOUNT, Param = 0, GoalData = 100 },
                new Goal() { GoalId = 100 },
                new UpGetPlayerFact() { Player = PlayerNumber, FactId = (int)FactId.POPULATION_HEADROOM, Param = 0, GoalData = 100 },
                new Goal() { GoalId = 100 },
                new UpGetPlayerFact() { Player = PlayerNumber, FactId = (int)FactId.HOUSING_HEADROOM, Param = 0, GoalData = 100 },
                new Goal() { GoalId = 100 },
                new PlayersStance() { PlayerNumber = PlayerNumber, Stance = (int)PlayerStance.ALLY },
                new PlayersStance() { PlayerNumber = PlayerNumber, Stance = (int)PlayerStance.NEUTRAL },
                new PlayersStance() { PlayerNumber = PlayerNumber, Stance = (int)PlayerStance.ENEMY }
            };

            return messages;
        }
    }
}
