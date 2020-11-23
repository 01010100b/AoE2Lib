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
        public TimeSpan LastTargetableGameTime { get; private set; } = TimeSpan.Zero;
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
        public CmdId CmdId { get; private set; }
        public int BaseTypeId { get; private set; } = -1;
        public int Carry { get; private set; }
        public bool Garrisoned { get; private set; }
        public int GarrisonCount { get; private set; }
        public int FrameDelay { get; private set; }
        public int TasksCount { get; private set; }
        public int AttackerCount { get; private set; }
        public int AttackerId { get; private set; } = -1;
        public TimeSpan TrainTime { get; private set; }
        public double BlastRadius { get; private set; }
        public int BlastLevel { get; private set; }
        public int ProgressType { get; private set; } = -2;
        public int ProgressValue { get; private set; }
        public int MinRange { get; private set; }
        public TimeSpan TargetTime { get; private set; }
        public bool Heresy { get; private set; }
        public bool Faith { get; private set; }
        public bool Redemption { get; private set; }
        public bool Atonement { get; private set; }
        public bool Theocracy { get; private set; }
        public bool Spies { get; private set; }
        public bool Ballistics { get; private set; }
        public int GatherType { get; private set; }
        public int LanguageId { get; private set; }

        public bool IsBuilding => CmdId == CmdId.CIVILIAN_BUILDING || CmdId == CmdId.MILITARY_BUILDING;

        internal Unit(Bot bot, int id) : base(bot)
        {
            Id = id;
        }

        public void TargetUnit(Unit target, UnitAction action, UnitFormation formation, UnitStance stance)
        {
            Bot.GetModule<MicroModule>().TargetUnit(this, target, action, formation, stance);
        }

        public void TargetPosition(Position target, UnitAction action, UnitFormation formation, UnitStance stance)
        {
            Bot.GetModule<MicroModule>().TargetPosition(this, target, action, formation, stance);
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
            yield return new UpObjectData() { ObjectData = (int)ObjectData.CARRY };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.GARRISONED };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.GARRISON_COUNT };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.FRAME_DELAY };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.TASKS_COUNT };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.ATTACKER_COUNT };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.ATTACKER_ID };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.TRAIN_TIME };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.BLAST_RADIUS };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.BLAST_LEVEL };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.PROGRESS_TYPE };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.PROGRESS_VALUE };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.MIN_RANGE };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.TARGET_TIME };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.HERESY };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.FAITH };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.REDEMPTION };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.ATONEMENT };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.THEOCRACY };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.SPIES };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.BALLISTICS };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.GATHER_TYPE };
            yield return new UpObjectData() { ObjectData = (int)ObjectData.LANGUAGE_ID };

            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.POINT_X, GoalData = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.POINT_Y, GoalData = 101 };
            yield return new UpPointExplored() { GoalPoint = 100 };
        }

        protected override void UpdateElement(IReadOnlyList<Any> responses)
        {
            var id = responses[1].Unpack<UpObjectDataResult>().Result;
            PlayerNumber = responses[6].Unpack<UpObjectDataResult>().Result;
            var explored = responses[responses.Count - 1].Unpack<UpPointExploredResult>().Result;

            if (explored != 15 && IsBuilding == false && PlayerNumber != Bot.PlayerNumber && PlayerNumber != 0)
            {
                return;
            }

            if (Id == id)
            {
                Targetable = true;
                LastTargetableGameTime = Bot.GetModule<InfoModule>().GameTime;

                if (explored != 15 && PlayerNumber != Bot.PlayerNumber && PlayerNumber != 0)
                {
                    return;
                }

                TargetId = responses[2].Unpack<UpObjectDataResult>().Result;
                var x = responses[3].Unpack<UpObjectDataResult>().Result;
                var y = responses[4].Unpack<UpObjectDataResult>().Result;
                TypeId = responses[5].Unpack<UpObjectDataResult>().Result;
                
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
                Carry = responses[22].Unpack<UpObjectDataResult>().Result;
                Garrisoned = responses[23].Unpack<UpObjectDataResult>().Result == 1;
                GarrisonCount = responses[24].Unpack<UpObjectDataResult>().Result;
                FrameDelay = responses[25].Unpack<UpObjectDataResult>().Result;
                TasksCount = responses[26].Unpack<UpObjectDataResult>().Result;
                AttackerCount = responses[27].Unpack<UpObjectDataResult>().Result;
                AttackerId = responses[28].Unpack<UpObjectDataResult>().Result;
                TrainTime = TimeSpan.FromSeconds(responses[29].Unpack<UpObjectDataResult>().Result);
                BlastRadius = responses[30].Unpack<UpObjectDataResult>().Result / 100d;
                BlastLevel = responses[31].Unpack<UpObjectDataResult>().Result;
                ProgressType = responses[32].Unpack<UpObjectDataResult>().Result;
                ProgressValue = responses[33].Unpack<UpObjectDataResult>().Result;
                MinRange = responses[34].Unpack<UpObjectDataResult>().Result;
                TargetTime = TimeSpan.FromMilliseconds(responses[35].Unpack<UpObjectDataResult>().Result);
                Heresy = responses[36].Unpack<UpObjectDataResult>().Result == 1;
                Faith = responses[37].Unpack<UpObjectDataResult>().Result == 1;
                Redemption = responses[38].Unpack<UpObjectDataResult>().Result == 1;
                Atonement = responses[39].Unpack<UpObjectDataResult>().Result == 1;
                Theocracy = responses[40].Unpack<UpObjectDataResult>().Result == 1;
                Spies = responses[41].Unpack<UpObjectDataResult>().Result == 1;
                Ballistics = responses[42].Unpack<UpObjectDataResult>().Result == 1;
                GatherType = responses[43].Unpack<UpObjectDataResult>().Result;
                LanguageId = responses[44].Unpack<UpObjectDataResult>().Result;

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
