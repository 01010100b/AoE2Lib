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
        public TimeSpan LastTargetable { get; private set; } = TimeSpan.Zero;
        public int TargetId { get; private set; } = -1;
        public Position Position { get; private set; }
        public Position Velocity { get; private set; }
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
        public bool IsBuilding => CmdId == CmdId.CIVILIAN_BUILDING || CmdId == CmdId.MILITARY_BUILDING;

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
            yield return new UpSetTargetById() { TypeOp = TypeOp.C, Id = Id };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.ID, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.TARGET_ID, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.PRECISE_X, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.PRECISE_Y, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.UPGRADE_TYPE, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.PLAYER, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.HITPOINTS, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.ORDER, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.NEXT_ATTACK, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.ATTACK_STANCE, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.MAXHP, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.RANGE, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.SPEED, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.BASE_ATTACK, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.STRIKE_ARMOR, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.PIERCE_ARMOR, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.RELOAD_TIME, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.TRAIN_SITE, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.CLASS, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.CMDID, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.BASE_TYPE, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };

            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.POINT_X, GoalData = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.POINT_Y, GoalData = 101 };
            yield return new UpPointExplored() { GoalPoint = 100 };
        }

        protected override void UpdateElement(IReadOnlyList<Any> responses)
        {
            var id = responses[2].Unpack<GoalResult>().Result;
            PlayerNumber = responses[12].Unpack<GoalResult>().Result;
            var explored = responses[45].Unpack<UpPointExploredResult>().Result;

            if (explored != 15 && IsBuilding == false && PlayerNumber != Bot.PlayerNumber && PlayerNumber != 0)
            {
                return;
            }

            if (Id == id)
            {
                Targetable = true;
                LastTargetable = Bot.GetModule<InfoModule>().GameTime;

                if (explored != 15 && PlayerNumber != Bot.PlayerNumber && PlayerNumber != 0)
                {
                    return;
                }

                TargetId = responses[4].Unpack<GoalResult>().Result;
                var x = responses[6].Unpack<GoalResult>().Result;
                var y = responses[8].Unpack<GoalResult>().Result;
                TypeId = responses[10].Unpack<GoalResult>().Result;
                
                Hitpoints = responses[14].Unpack<GoalResult>().Result;
                Order = (UnitOrder)responses[16].Unpack<GoalResult>().Result;
                NextAttack = TimeSpan.FromMilliseconds(responses[18].Unpack<GoalResult>().Result);
                Stance = (UnitStance)responses[20].Unpack<GoalResult>().Result;
                MaxHitpoints = responses[22].Unpack<GoalResult>().Result;
                Range = responses[24].Unpack<GoalResult>().Result;
                Speed = responses[26].Unpack<GoalResult>().Result / 100d;
                Attack = responses[28].Unpack<GoalResult>().Result;
                MeleeArmor = responses[30].Unpack<GoalResult>().Result;
                PierceArmor = responses[32].Unpack<GoalResult>().Result;
                ReloadTime = TimeSpan.FromMilliseconds(responses[34].Unpack<GoalResult>().Result);
                TrainSiteId = responses[36].Unpack<GoalResult>().Result;
                Class = (UnitClass)responses[38].Unpack<GoalResult>().Result;
                CmdId = (CmdId)responses[40].Unpack<GoalResult>().Result;
                BaseTypeId = responses[42].Unpack<GoalResult>().Result;

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
