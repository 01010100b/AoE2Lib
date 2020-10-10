using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots
{
    public class Unit : GameElement
    {
        public enum UnitOrder
        {
            NONE = -1, ATTACK, DEFEND, BUILD, HEAL, CONVERT, EXPLORE, STOP,
            RUNAWAY, RETREAT, GATHER, MOVE, PATROL, FOLLOW, HUNT, TRANSPORT,
            TRADE, EVADE, ENTER, REPAIR, TRAIN, RESEARCH, UNLOAD, RELIC = 31
        }

        public enum UnitStance
        {
            AGGRESSIVE, DEFENSIVE, STAND_GROUND, NO_ATTACK
        }

        public readonly int Id; // 45000
        public int TargetId { get; private set; } = -1; // 45000
        public Position Position { get; private set; } = new Position(-1, -1); // 500 x 500
        public int TypeId { get; private set; } = -1; // 2000
        public int PlayerNumber { get; private set; } = -1; // 10
        public int Hitpoints { get; private set; } = -1; // 1000
        public UnitOrder Order { get; private set; } = UnitOrder.NONE; // 40
        public DateTime NextAttack { get; private set; } = DateTime.UtcNow; // 20
        public UnitStance Stance { get; private set; } = UnitStance.AGGRESSIVE; // 4

        public Unit(int id)
        {
            Id = id;
        }

        internal void Update(int goal0, int goal1, int goal2)
        {
            var id = goal0 % 45000;
            goal0 /= 45000;

            if (id != Id)
            {
                throw new ArgumentException("Incorrect unit id:" + id);
            }

            TargetId = goal0 % 45000;

            var x = goal1 % 500;
            goal1 /= 500;
            var y = goal1 % 500;
            goal1 /= 500;
            Position = new Position(x, y);
            TypeId = goal1 % 2000;

            PlayerNumber = goal2 % 10;
            goal2 /= 10;
            Hitpoints = goal2 % 1000;
            goal2 /= 1000;
            Order = (UnitOrder)((goal2 % 40) - 1);
            goal2 /= 40;
            var timer = goal2 % 20;
            goal2 /= 20;
            NextAttack = DateTime.UtcNow + TimeSpan.FromSeconds(timer / 5d);
            Stance = (UnitStance)(goal2 % 4);

            ElementUpdated();
        }
    }
}
