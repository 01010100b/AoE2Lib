using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Behaviours
{
    internal class MicroBehaviour : Behaviour
    {
        private const bool TEST_MICRO = false;

        public Controller Leader { get; private set; }

        protected override bool Tick(bool perform)
        {
            if (!perform)
            {
                return false;
            }

            if (!TEST_MICRO)
            {
                return false;
            }

            Controller.Unit.RequestUpdate();
            EnsureLeader();

            if (Leader != Controller)
            {
                return true;
            }

            var targets = ChooseTargets();

            if (targets.Count > 0)
            {
                var controllers = new List<Controller>();

                foreach (var controller in Controller.Manager.GetControllers())
                {
                    if (controller.TryGetBehaviour<MicroBehaviour>(out var micro))
                    {
                        if (micro.Leader == Controller)
                        {
                            controllers.Add(controller);
                        }
                    }
                }

                DoMicro(controllers, targets);
            }

            return true;
        }

        private void EnsureLeader()
        {
            if (Leader != null)
            {
                if (!Leader.Unit.Targetable)
                {
                    Leader = null;
                }
            }

            if (Leader == null)
            {
                foreach (var controller in Controller.Manager.GetControllers())
                {
                    if (controller.TryGetBehaviour<MicroBehaviour>(out var micro))
                    {
                        if (micro.Leader != null && micro.Leader.Unit.Targetable
                            && micro.Leader.Unit[ObjectData.BASE_TYPE] == Controller.Unit[ObjectData.BASE_TYPE])
                        {
                            Leader = micro.Leader;

                            return;
                        }
                    }
                }

                if (Controller.Unit.Targetable)
                {
                    Leader = Controller;
                }

                return;
            }
        }

        private List<Unit> ChooseTargets()
        {
            var targets = new List<Unit>();

            foreach (var enemy in Controller.Unary.GameState.CurrentEnemies.SelectMany(e => e.Units.Where(u => u.Targetable)))
            {
                targets.Add(enemy);
                enemy.RequestUpdate();
            }

            targets.Sort((a, b) =>
            {
                if (a[ObjectData.HITPOINTS] < b[ObjectData.HITPOINTS])
                {
                    return -1;
                }
                else if (b[ObjectData.HITPOINTS] < a[ObjectData.HITPOINTS])
                {
                    return 1;
                }
                else
                {
                    return a.Position.DistanceTo(Controller.Unit.Position).CompareTo(b.Position.DistanceTo(Controller.Unit.Position));
                }
            });

            return targets;
        }

        private void DoMicro(List<Controller> controllers,  List<Unit> targets)
        {
            const int GL_ERROR = Bot.GOAL_START;
            const int GL_NEXT_ATTACK = Bot.GOAL_START + 1;
            const int GL_LOCAL_TOTAL = Bot.GOAL_START + 2;
            const int GL_LOCAL_LAST = Bot.GOAL_START + 3;
            const int GL_REMOTE_TOTAL = Bot.GOAL_START + 4;
            const int GL_REMOTE_LAST = Bot.GOAL_START + 5;
            const int GL_STEP = Bot.GOAL_START + 6;
            const int GL_PRECISE_X = Bot.GOAL_START + 7;
            const int GL_PRECISE_Y = Bot.GOAL_START + 8;

            var target = targets[0];

            var command = new Command();
            command.Add(new SetGoal() { InConstGoalId = GL_ERROR, InConstValue = 0 });
            command.Add(new SetGoal() { InConstGoalId = GL_NEXT_ATTACK, InConstValue = 10000 });
            command.Add(new SetGoal() { InConstGoalId = GL_STEP, InConstValue = 0 });
            command.Add(new UpFullResetSearch());

            // fill search lists

            foreach (var controller in controllers)
            {
                command.Add(new UpAddObjectById() { InConstSearchSource = 1, InConstId = controller.Unit.Id });
            }

            command.Add(new UpAddObjectById() { InConstSearchSource = 2, InConstId = target.Id });
            command.Add(new UpGetSearchState() { OutGoalState = GL_LOCAL_TOTAL });
            command.Add(new Goal() { InConstGoalId = GL_LOCAL_TOTAL }, "<", 1,
                new SetGoal() { InConstGoalId = GL_ERROR, InConstValue = -1 });
            command.Add(new Goal() { InConstGoalId = GL_REMOTE_TOTAL }, "<", 1,
                new SetGoal() { InConstGoalId = GL_ERROR, InConstValue = -2 });

            // do micro

            var next_attack = Math.Max(1, Controller.Unit[ObjectData.RELOAD_TIME] - 500);

            command.Add(new Goal() { InConstGoalId = GL_ERROR }, "==", 0,
                new UpGetObjectData() { InConstObjectData = (int)ObjectData.NEXT_ATTACK, OutGoalData = GL_NEXT_ATTACK });
            command.Add(new Goal() { InConstGoalId = GL_NEXT_ATTACK }, ">=", next_attack,
                new SetGoal() { InConstGoalId = GL_ERROR, InConstValue = -3 });
            command.Add(new Goal() { InConstGoalId = GL_NEXT_ATTACK }, "<=", 0,
                new SetGoal() { InConstGoalId = GL_STEP, InConstValue = 1 });
            command.Add(new Goal() { InConstGoalId = GL_NEXT_ATTACK }, ">", 0,
                new SetGoal() { InConstGoalId = GL_STEP, InConstValue = 2 });
            command.Add(new Goal() { InConstGoalId = GL_ERROR }, "!=", 0,
                new SetGoal() { InConstGoalId = GL_STEP, InConstValue = 0 });

            command.Add(new Goal() { InConstGoalId = GL_STEP }, "==", 1,
                new UpTargetObjects() { InConstTarget = 0, InConstTargetAction = 0, InConstFormation = 2, InConstAttackStance = -1 });

            var dpos = target.Position - Controller.Unit.Position;
            dpos = 2 * dpos.Normalize();
            var delta = dpos.Rotate(Math.PI / 2);

            if (Controller.Unary.GameState.Tick % 2 == 0)
            {
                delta *= -1;
            }

            var range = (double)Controller.Unit[ObjectData.RANGE];
            
            if (target.Position.DistanceTo(Controller.Unit.Position) < range - 1)
            {
                delta -= dpos;
            }

            var position = Controller.Unit.Position + delta;
            command.Add(new SetGoal() { InConstGoalId = GL_PRECISE_X, InConstValue = position.PreciseX });
            command.Add(new SetGoal() { InConstGoalId = GL_PRECISE_Y, InConstValue = position.PreciseY });

            command.Add(new Goal() { InConstGoalId = GL_STEP }, "==", 2,
                new SetStrategicNumber() { InConstSnId = (int)AoE2Lib.Bots.StrategicNumber.TARGET_POINT_ADJUSTMENT, InConstValue = 6 },
                new UpTargetPoint()
                {
                    InGoalPoint = GL_PRECISE_X,
                    InConstTargetAction = (int)UnitAction.MOVE,
                    InConstFormation = 2,
                    InConstAttackStance = (int)UnitStance.NO_ATTACK
                }
            );

            Controller.Unary.ExecuteCommand(command);
        }
    }
}
