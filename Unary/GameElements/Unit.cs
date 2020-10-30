using Unary.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace Unary.GameElements
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

        public readonly int Id;
        public int TargetId { get; private set; } = -1;
        public Position Position { get; private set; } = new Position(-1, -1);
        public int TypeId { get; private set; } = -1;
        public int PlayerNumber { get; private set; } = -1;
        public int Hitpoints { get; private set; } = -1;
        public UnitOrder Order { get; private set; } = UnitOrder.NONE;
        public TimeSpan NextAttack { get; private set; } = TimeSpan.MinValue;
        public UnitStance Stance { get; private set; } = UnitStance.AGGRESSIVE;
        public bool Targetable { get; private set; } = false;

        public Unit(int id) : base()
        {
            Id = id;
        }

        public override bool Equals(object obj)
        {
            if (obj is Unit unit)
            {
                return Id == unit.Id;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        protected override void UpdateElement(List<Any> responses)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<IMessage> RequestElementUpdate()
        {
            throw new NotImplementedException();
        }
    }
}
