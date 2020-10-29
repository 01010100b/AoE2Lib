using Unary.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Google.Protobuf;

namespace Unary.GameElements
{
    public class Player : GameElement
    {
        public enum PlayerStance
        {
            ALLY, NEUTRAL, ALL, ENEMY
        }

        public readonly int PlayerNumber;
        public int Civilization { get; private set; } = -1;
        public int Score { get; private set; } = -1;
        public int Age { get; private set; } = -1;
        public int CivilianPopulation { get; private set; } = -1;
        public int MilitaryPopulation { get; private set; } = -1;
        public PlayerStance Stance { get; private set; } = PlayerStance.NEUTRAL;

        public Player(int player) : base()
        {
            PlayerNumber = player;
        }

        protected override void UpdateElement(IEnumerable<IMessage> responses)
        {
            throw new NotImplementedException();
        }
    }
}
