using Unary.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Protos.Expert.Action;
using Protos.Expert.Fact;

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
            Targetable = responses[0].Unpack<UpSetTargetByIdResult>().Result;
            
            if (Targetable)
            {
                var id = responses[1].Unpack<UpObjectDataResult>().Result;

                if (Id == id)
                {
                    TargetId = responses[2].Unpack<UpObjectDataResult>().Result;
                    var x = responses[3].Unpack<UpObjectDataResult>().Result;
                    var y = responses[4].Unpack<UpObjectDataResult>().Result;
                    Position = new Position(x, y);
                    TypeId = responses[5].Unpack<UpObjectDataResult>().Result;
                    PlayerNumber = responses[6].Unpack<UpObjectDataResult>().Result;
                    Hitpoints = responses[7].Unpack<UpObjectDataResult>().Result;
                    Order = (UnitOrder)responses[8].Unpack<UpObjectDataResult>().Result;
                    NextAttack = TimeSpan.FromMilliseconds(responses[9].Unpack<UpObjectDataResult>().Result);
                    Stance = (UnitStance)responses[10].Unpack<UpObjectDataResult>().Result;
                }
            }
        }

        protected override IEnumerable<IMessage> RequestElementUpdate()
        {
            var messages = new List<IMessage>()
            {
                /*new UpFullResetSearch(),
                new UpFindLocal() {TypeOp1 = (int)TypeOp.C, UnitId = -1, TypeOp2 = (int)TypeOp.C, Count = 1},
                new UpSetTargetObject() {SearchSource = 1, TypeOp = (int)TypeOp.C, Index = 0},*/
                new UpSetTargetById() {TypeOp = (int)TypeOp.C, Id = Id},
                new UpObjectData() {ObjectData = (int)ObjectData.ID},
                new UpObjectData() {ObjectData = (int)ObjectData.TARGET_ID},
                new UpObjectData() {ObjectData = (int)ObjectData.POINT_X},
                new UpObjectData() {ObjectData = (int)ObjectData.POINT_Y},
                new UpObjectData() {ObjectData = (int)ObjectData.UPGRADE_TYPE},
                new UpObjectData() {ObjectData = (int)ObjectData.PLAYER},
                new UpObjectData() {ObjectData = (int)ObjectData.HITPOINTS},
                new UpObjectData() {ObjectData = (int)ObjectData.ORDER},
                new UpObjectData() {ObjectData = (int)ObjectData.NEXT_ATTACK},
                new UpObjectData() {ObjectData = (int)ObjectData.ATTACK_STANCE}
            };

            return messages;
        }
    }
}
