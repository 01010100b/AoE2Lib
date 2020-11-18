using AoE2Lib.Bots.Modules;
using AoE2Lib.Utils;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots.GameElements
{
    public class Unit : GameElement
    {
        public readonly int Id;
        public UnitType UnitType { get; internal set; } = null;
        public bool Targetable { get; private set; } = true;
        public DateTime LastTargetable { get; private set; } = DateTime.UtcNow;
        public int TargetId { get; private set; } = -1;
        public Position Position { get; private set; } = Position.FromPoint(-1, -1);
        public Position Velocity { get; private set; } = Position.FromPoint(0, 0);
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
        public int BaseTypeId { get; private set; } = -1;

        protected internal Unit(Bot bot, int id) : base(bot)
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

        protected override IEnumerable<IMessage> RequestElementUpdate()
        {
            yield return new UpSetTargetById() { TypeOp = (int)TypeOp.C, Id = Id };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.ID };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.TARGET_ID };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.PRECISE_X };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.PRECISE_Y };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.UPGRADE_TYPE };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.PLAYER };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.HITPOINTS };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.ORDER };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.NEXT_ATTACK };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.ATTACK_STANCE };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.MAXHP };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.RANGE };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.SPEED };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.BASE_ATTACK };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.STRIKE_ARMOR };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.PIERCE_ARMOR };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.RELOAD_TIME };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.TRAIN_SITE };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.CLASS };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.CMDID };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.BASE_TYPE };

            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.POINT_X, GoalData = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.POINT_Y, GoalData = 101 };
            yield return new UpPointExplored() { GoalPoint = 100 };
        }

        protected override void UpdateElement(IReadOnlyList<Any> responses)
        {
            var id = responses[1].Unpack<UpObjectDataResult>().Result;
            var explored = responses[24].Unpack<UpPointExploredResult>().Result;

            if (Id == id)
            {
                Targetable = true;
                LastTargetable = DateTime.UtcNow;

                if (explored != 15)
                {
                    return;
                }

                TargetId = responses[2].Unpack<UpObjectDataResult>().Result;
                var x = responses[3].Unpack<UpObjectDataResult>().Result;
                var y = responses[4].Unpack<UpObjectDataResult>().Result;
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
                BaseTypeId = responses[21].Unpack<UpObjectDataResult>().Result;

                var pos = Position.FromPrecise(x, y);
                var ticks = Math.Max(1, Bot.Tick - LastUpdateTick);
                var seconds = Math.Max(0.001, ticks * Bot.GetModule<InfoModule>().GameSecondsPerTick);
                var v = (pos - Position) / seconds;
                if (v.Norm < Speed * 2)
                {
                    Velocity = ((ticks * Velocity) + (3 * v)) / (3 + ticks);
                }

                Position = pos;
            }
            else
            {
                Targetable = false;
            }
        }

        
    }
}
