﻿using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public bool IsComputer { get; private set; } = false;
        public PlayerStance Stance { get; private set; } = PlayerStance.NEUTRAL;
        public bool IsEnemy => Stance == PlayerStance.ENEMY && PlayerNumber != 0;
        public bool IsAlly => Stance == PlayerStance.ALLY && PlayerNumber != Bot.PlayerNumber;
        public bool InGame => GetFact(FactId.PLAYER_IN_GAME) == 1;
        public int Score => GetFact(FactId.CURRENT_SCORE);
        public int CivilianPopulation => GetFact(FactId.CIVILIAN_POPULATION);
        public int MilitaryPopulation => GetFact(FactId.MILITARY_POPULATION);
        public int PopulationCap => GetFact(FactId.POPULATION_CAP);
        public int Civilization => GetFact(FactId.CIVILIZATION);
        public IEnumerable<Unit> Units => KnownUnits;

        internal readonly List<Unit> KnownUnits = new();
        internal readonly List<Unit> UnknownUnits = new();
        private readonly Dictionary<FactId, int> Facts = new();
        private readonly Dictionary<int, int> Goals = new();
        private readonly Dictionary<int, int> StrategicNumbers = new();

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

        public int GetGoal(int goal)
        {
            if (goal < 1 || goal > 512)
            {
                throw new ArgumentOutOfRangeException(nameof(goal));
            }

            if (!IsComputer)
            {
                Goals.Clear();

                return -1;
            }

            if (PlayerNumber != Bot.PlayerNumber && Stance != PlayerStance.ALLY)
            {
                Goals.Clear();

                return -1;
            }

            if (!Goals.ContainsKey(goal))
            {
                Goals.Add(goal, -1);
            }

            return Goals[goal];
        }

        public int GetStrategicNumber(int sn)
        {
            if (sn < 0 || sn > 511)
            {
                throw new ArgumentOutOfRangeException(nameof(sn));
            }

            if (!IsComputer)
            {
                StrategicNumbers.Clear();

                return -1;
            }

            if (PlayerNumber != Bot.PlayerNumber && Stance != PlayerStance.ALLY)
            {
                StrategicNumbers.Clear();

                return -1;
            }

            if (!StrategicNumbers.ContainsKey(sn))
            {
                StrategicNumbers.Add(sn, -1);
            }

            return StrategicNumbers[sn];
        }

        public int GetStrategicNumber(StrategicNumber sn)
        {
            return GetStrategicNumber((int)sn);
        }

        protected override IEnumerable<IMessage> RequestElementUpdate()
        {
            const int GL_TEMP = Bot.GOAL_START;

            if (!IsValid)
            {
                yield return new PlayerValid() { InPlayerAnyPlayer = PlayerNumber };
                yield break;
            }

            yield return new StanceToward() { InPlayerAnyPlayer = PlayerNumber, InConstESPlayerStance = (int)PlayerStance.ALLY };
            yield return new StanceToward() { InPlayerAnyPlayer = PlayerNumber, InConstESPlayerStance = (int)PlayerStance.ENEMY };
            yield return new StanceToward() { InPlayerAnyPlayer = PlayerNumber, InConstESPlayerStance = (int)PlayerStance.NEUTRAL };

            yield return new PlayerComputer() { InPlayerAnyPlayer = PlayerNumber };

            foreach (var fact in FACTS)
            {
                yield return new UpGetPlayerFact() { InPlayerAnyPlayer = PlayerNumber, InConstFactId = (int)fact, InConstParam = 0, OutGoalData = GL_TEMP };
                yield return new Goal() { InConstGoalId = GL_TEMP };
            }

            var ids = ObjectPool.Get(() => new List<int>(), x => x.Clear());
            
            ids.AddRange(Goals.Keys);
            ids.Sort();
            foreach (var id in ids)
            {
                yield return new UpAlliedGoal() { InGoalId = id, InPlayerComputerAllyPlayer = PlayerNumber };
            }

            ids.Clear();
            ids.AddRange(StrategicNumbers.Keys);
            ids.Sort();
            foreach (var id in ids)
            {
                yield return new UpAlliedSn() { InSnId = id, InPlayerComputerAllyPlayer = PlayerNumber };
            }

            ObjectPool.Add(ids);
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

            IsComputer = responses[3].Unpack<PlayerComputerResult>().Result;

            var index = 4;
            foreach (var fact in FACTS)
            {
                var val = responses[index + 1].Unpack<GoalResult>().Result;
                Facts[fact] = val;
                index += 2;
            }

            var ids = ObjectPool.Get(() => new List<int>(), x => x.Clear());

            ids.AddRange(Goals.Keys);
            ids.Sort();
            foreach (var id in ids)
            {
                var val = responses[index].Unpack<UpAlliedGoalResult>().Result;
                Goals[id] = val;
                index++;
            }

            ids.Clear();
            ids.AddRange(StrategicNumbers.Keys.Cast<int>());
            ids.Sort();
            foreach (var id in ids)
            {
                var val = responses[index].Unpack<UpAlliedSnResult>().Result;
                StrategicNumbers[id] = val;
                index++;
            }

            ObjectPool.Add(ids);
        }
    }
}
