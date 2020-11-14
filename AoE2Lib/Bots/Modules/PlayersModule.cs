using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots.Modules
{
    public class PlayersModule : Module
    {
        public class Player
        {
            public readonly int PlayerNumber;
            public bool InGame { get; internal set; } = false;
            public int Civilization { get; internal set; } = -1;
            public int Score { get; internal set; } = -1;
            public int Age { get; internal set; } = -1;
            public int Population => CivilianPopulation + MilitaryPopulation;
            public int CivilianPopulation { get; internal set; } = -1;
            public int MilitaryPopulation { get; internal set; } = -1;
            public int WoodAmount { get; internal set; } = -1;
            public int FoodAmount { get; internal set; } = -1;
            public int GoldAmount { get; internal set; } = -1;
            public int StoneAmount { get; internal set; } = -1;
            public PlayerStance Stance { get; private set; } = PlayerStance.NEUTRAL;

            internal readonly Command Command = new Command();

            internal Player(int player)
            {
                PlayerNumber = player;
            }
        }

        public IReadOnlyDictionary<int, Player> Players => _Players;
        private Dictionary<int, Player> _Players = new Dictionary<int, Player>();

        protected internal override IEnumerable<Command> RequestUpdate()
        {
            throw new NotImplementedException();
        }

        protected internal override void Update()
        {
            throw new NotImplementedException();
        }
    }
}
