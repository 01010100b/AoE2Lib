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
            InGame = responses[0].Unpack<PlayerInGameResult>().Result;
            Civilization = responses[2].Unpack<GoalResult>().Result;

            throw new NotImplementedException();
        }

        protected override IEnumerable<IMessage> RequestElementUpdate()
        {
            var messages = new List<IMessage>()
            {
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
