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
        public bool Targetable { get; private set; } = true;
        public int TargetId { get; private set; } = -1;
        public Position Position { get; private set; } = new Position(-1, -1);
        public int TypeId { get; private set; } = -1;
        public int PlayerNumber { get; private set; } = -1;
        public int Hitpoints { get; private set; } = -1;
        public UnitOrder Order { get; private set; } = UnitOrder.NONE;
        public TimeSpan NextAttack { get; private set; } = TimeSpan.MinValue;
        public UnitStance Stance { get; private set; } = UnitStance.AGGRESSIVE;
        public int MaxHitpoints { get; private set; } = -1;
        public int Range { get; private set; } = -1;
        public double Speed { get; private set; } = -1;
        public int Attack { get; private set; } = -1;
        public int MeleeArmor { get; private set; } = 0;
        public int PierceArmor { get; private set; } = 0;
        public TimeSpan ReloadTime { get; private set; } = TimeSpan.MinValue;
        public int TrainSiteId { get; private set; } = -1;
        public UnitClass Class { get; private set; } = UnitClass.Miscellaneous;
        public CmdId CmdId { get; private set; } = CmdId.FLAG;

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
            var id = responses[1].Unpack<UpObjectDataResult>().Result;

            if (Id == id)
            {
                Targetable = true;

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
                MaxHitpoints = responses[11].Unpack<UpObjectDataResult>().Result;
                Range = responses[12].Unpack<UpObjectDataResult>().Result;
                Speed = responses[13].Unpack<UpObjectDataResult>().Result / 100d;
                Attack = responses[14].Unpack<UpObjectDataResult>().Result;
                MeleeArmor = responses[15].Unpack<UpObjectDataResult>().Result;
                PierceArmor = responses[16].Unpack<UpObjectDataResult>().Result;
                ReloadTime = TimeSpan.FromMilliseconds(responses[17].Unpack<UpObjectDataResult>().Result);
                TrainSiteId = responses[18].Unpack<UpObjectDataResult>().Result;
                Class = (UnitClass)responses[19].Unpack<UpObjectDataResult>().Result;
                CmdId = (CmdId)responses[20].Unpack<UpObjectDataResult>().Result;
            }
            else
            {
                Targetable = false;
            }
        }

        protected override IEnumerable<IMessage> RequestElementUpdate()
        {
            var messages = new List<IMessage>()
            {
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
                new UpObjectData() {ObjectData = (int)ObjectData.ATTACK_STANCE},
                new UpObjectData() {ObjectData = (int)ObjectData.MAXHP},
                new UpObjectData() {ObjectData = (int)ObjectData.RANGE},
                new UpObjectData() {ObjectData = (int)ObjectData.SPEED},
                new UpObjectData() {ObjectData = (int)ObjectData.BASE_ATTACK},
                new UpObjectData() {ObjectData = (int)ObjectData.STRIKE_ARMOR},
                new UpObjectData() {ObjectData = (int)ObjectData.PIERCE_ARMOR},
                new UpObjectData() {ObjectData = (int)ObjectData.RELOAD_TIME},
                new UpObjectData() {ObjectData = (int)ObjectData.TRAIN_SITE},
                new UpObjectData() {ObjectData = (int)ObjectData.CLASS},
                new UpObjectData() {ObjectData = (int)ObjectData.CMDID}
            };

            return messages;
        }
    }
}
