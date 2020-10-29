using Unary.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;

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
        public DateTime NextAttack { get; private set; } = DateTime.UtcNow;
        public UnitStance Stance { get; private set; } = UnitStance.AGGRESSIVE;
        public bool Targetable { get; private set; } = false;
        public DateTime TargetableUpdate { get; private set; } = DateTime.MinValue;

        public Unit(int id) : base()
        {
            Id = id;
        }

        internal void UpdateTargetable(bool targetable)
        {
            Targetable = targetable;
            TargetableUpdate = DateTime.UtcNow;
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

        protected override void UpdateElement(IEnumerable<IMessage> responses)
        {
            throw new NotImplementedException();
        }
    }
}
