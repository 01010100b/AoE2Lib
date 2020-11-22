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
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.CARRY, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.GARRISONED, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.GARRISON_COUNT, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.FRAME_DELAY, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.TASKS_COUNT, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.ATTACKER_COUNT, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.ATTACKER_ID, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.TRAIN_TIME, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.BLAST_RADIUS, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.BLAST_LEVEL, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.PROGRESS_TYPE, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.PROGRESS_VALUE, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.MIN_RANGE, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.TARGET_TIME, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.HERESY, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.FAITH, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.REDEMPTION, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.ATONEMENT, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.THEOCRACY, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.SPIES, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.BALLISTICS, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.GATHER_TYPE, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.LANGUAGE_ID, GoalData = 100 };
            yield return new Goal() { GoalId = 100 };

            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.POINT_X, GoalData = 100 };
            yield return new UpGetObjectData() { ObjectData = (int)ObjectData.POINT_Y, GoalData = 101 };
            yield return new UpPointExplored() { GoalPoint = 100 };
        }

        protected override void UpdateElement(IReadOnlyList<Any> responses)
        {
            var id = responses[2].Unpack<GoalResult>().Result;
            PlayerNumber = responses[12].Unpack<GoalResult>().Result;
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
                Carry = responses[44].Unpack<GoalResult>().Result;
                Garrisoned = responses[46].Unpack<GoalResult>().Result == 1;
                GarrisonCount = responses[48].Unpack<GoalResult>().Result;
                FrameDelay = responses[50].Unpack<GoalResult>().Result;
                TasksCount = responses[52].Unpack<GoalResult>().Result;
                AttackerCount = responses[54].Unpack<GoalResult>().Result;
                AttackerId = responses[56].Unpack<GoalResult>().Result;
                TrainTime = TimeSpan.FromSeconds(responses[58].Unpack<GoalResult>().Result);
                BlastRadius = responses[60].Unpack<GoalResult>().Result / 100d;
                BlastLevel = responses[62].Unpack<GoalResult>().Result;
                ProgressType = responses[64].Unpack<GoalResult>().Result;
                ProgressValue = responses[66].Unpack<GoalResult>().Result;
                MinRange = responses[68].Unpack<GoalResult>().Result;
                TargetTime = TimeSpan.FromMilliseconds(responses[70].Unpack<GoalResult>().Result);
                Heresy = responses[72].Unpack<GoalResult>().Result == 1;
                Faith = responses[74].Unpack<GoalResult>().Result == 1;
                Redemption = responses[76].Unpack<GoalResult>().Result == 1;
                Atonement = responses[78].Unpack<GoalResult>().Result == 1;
                Theocracy = responses[80].Unpack<GoalResult>().Result == 1;
                Spies = responses[82].Unpack<GoalResult>().Result == 1;
                Ballistics = responses[84].Unpack<GoalResult>().Result == 1;
                GatherType = responses[86].Unpack<GoalResult>().Result;
                LanguageId = responses[88].Unpack<GoalResult>().Result;

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
