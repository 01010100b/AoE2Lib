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
        private static readonly FactId[] FACTS =
        {
            FactId.POPULATION_CAP,
            FactId.POPULATION_HEADROOM,
            FactId.HOUSING_HEADROOM,
            FactId.IDLE_FARM_COUNT,
            FactId.FOOD_AMOUNT,
            FactId.WOOD_AMOUNT,
            FactId.STONE_AMOUNT,
            FactId.GOLD_AMOUNT,
            FactId.CURRENT_AGE,
            FactId.CURRENT_SCORE,
            FactId.CIVILIZATION,
            FactId.PLAYER_NUMBER,
            FactId.PLAYER_IN_GAME,
            FactId.POPULATION,
            FactId.MILITARY_POPULATION,
            FactId.CIVILIAN_POPULATION,
            FactId.PLAYER_DISTANCE,
            FactId.ENEMY_BUILDINGS_IN_TOWN,
            FactId.ENEMY_UNITS_IN_TOWN,
            FactId.ENEMY_VILLAGERS_IN_TOWN,
            FactId.CURRENT_AGE_TIME
        };

        public readonly int PlayerNumber;
        public bool IsValid { get; private set; } = false;
        public PlayerStance Stance { get; private set; } = PlayerStance.NEUTRAL;
        public bool InGame => GetFact(FactId.PLAYER_IN_GAME) != 0;
        public int Civilization => GetFact(FactId.CIVILIZATION);
        public int Score => GetFact(FactId.CURRENT_SCORE);
        public int Age => GetFact(FactId.CURRENT_AGE);
        public int CivilianPopulation => GetFact(FactId.CIVILIAN_POPULATION);
        public int MilitaryPopulation => GetFact(FactId.MILITARY_POPULATION);
        public int WoodAmount => GetFact(FactId.WOOD_AMOUNT);
        public int FoodAmount => GetFact(FactId.FOOD_AMOUNT);
        public int GoldAmount => GetFact(FactId.GOLD_AMOUNT);
        public int StoneAmount => GetFact(FactId.STONE_AMOUNT);
        
        internal readonly List<Unit> Units = new List<Unit>();
        private readonly Dictionary<FactId, int> Facts = new Dictionary<FactId, int>();

        internal Player(Bot bot, int player) : base(bot)
        {
            PlayerNumber = player;
        }

        public int GetFact(FactId fact)
        {
            if (Facts.TryGetValue(fact, out int val))
            {
                return val;
            }
            else
            {
                return -1;
            }
        }

        public IEnumerable<Unit> GetUnits()
        {
            return Units;
        }

        public void FindUnits(Position position, int range)
        {
            Bot.GameState.FindUnits(PlayerNumber, position, range);
        }
        
        protected override IEnumerable<IMessage> RequestElementUpdate()
        {
            if (!IsValid)
            {
                yield return new PlayerValid() { InPlayerAnyPlayer = PlayerNumber };
                yield break;
            }

            yield return new StanceToward() { InPlayerAnyPlayer = PlayerNumber, InConstESPlayerStance = (int)PlayerStance.ALLY };
            yield return new StanceToward() { InPlayerAnyPlayer = PlayerNumber, InConstESPlayerStance = (int)PlayerStance.ENEMY };
            yield return new StanceToward() { InPlayerAnyPlayer = PlayerNumber, InConstESPlayerStance = (int)PlayerStance.NEUTRAL };

            foreach (var fact in FACTS)
            {
                yield return new UpGetPlayerFact() { InPlayerAnyPlayer = PlayerNumber, InConstFactId = (int)fact, InConstParam = 0, OutGoalData = 100 };
                yield return new Goal() { InConstGoalId = 100 };
            }
            
        }

        protected override void UpdateElement(IReadOnlyList<Any> responses)
        {
            if (!IsValid)
            {
                IsValid = responses[0].Unpack<PlayerValidResult>().Result;

                return;
            }

            if (responses[0].Unpack<StanceTowardResult>().Result)
            {
                Stance = PlayerStance.ALLY;
            }
            else if (responses[1].Unpack<StanceTowardResult>().Result)
            {
                Stance = PlayerStance.ENEMY;
            }
            else if (responses[2].Unpack<StanceTowardResult>().Result)
            {
                Stance = PlayerStance.NEUTRAL;
            }

            var index = 2;
            foreach (var fact in FACTS)
            {
                index += 2;
                var val = responses[index].Unpack<GoalResult>().Result;
                Facts[fact] = val;
            }
        }
    }
}
