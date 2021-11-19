using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Algorithms;

namespace Unary
{
    abstract class UnitController
    {
        public readonly Unit Unit;
        public readonly Unary Unary;
        public readonly PotentialField PotentialField = new();

        private readonly List<Unit> RelevantUnits = new();
        private int RefreshRate { get; set; } = 5;

        public UnitController(Unit unit, Unary unary)
        {
            Unit = unit;
            Unary = unary;

            Unary.UnitsManager.SetController(Unit, this);
        }

        public void Update()
        {
            Unit.RequestUpdate();

            if (GetHashCode() % RefreshRate == Unary.GameState.Tick % RefreshRate)
            {
                UpdateRelevantUnits();
            }

            if (Unary.Rng.NextDouble() < 0.2)
            {
                RefreshRate = Math.Min(23, RefreshRate + 1);
            }

            var target = Unit.GetTarget();
            if (target != null && target[ObjectData.CLASS] == (int)UnitClass.PredatorAnimal && target[ObjectData.HITPOINTS] > 0)
            {
                Unary.Log.Debug($"Unit {Unit.Id} fighting animal {target.Id}");

                return;
            }
            
            Tick();
        }

        protected abstract void Tick();

        protected void PerformPotentialFieldStep(Position move, double radius, int min_next_attack = int.MinValue, int max_next_attack = int.MaxValue)
        {
            RefreshRate = 5;

            var op_g_sub = 14;

            var current_pos = Unit.Position;
            var current_pos_move = GetMovementPosition(current_pos, move, radius);
            var pred_pos = PredictPosition(Unit);
            var pred_pos_move = GetMovementPosition(pred_pos, move, radius);

            const int GL_CONTROL = 100;
            const int GL_TEMP = 101;
            const int GL_CURRENT_X = 102;
            const int GL_CURRENT_Y = 103;
            const int GL_CURRENT_MOVE_X = 104;
            const int GL_CURRENT_MOVE_Y = 105;
            const int GL_CURRENT_DISTANCE = 106;
            const int GL_PRED_X = 107;
            const int GL_PRED_Y = 108;
            const int GL_PRED_MOVE_X = 109;
            const int GL_PRED_MOVE_Y = 110;
            const int GL_PRED_DISTANCE = 111;
            const int GL_MOVE_X = 112;
            const int GL_MOVE_Y = 113;

            var command = new Command();

            command.Add(new SetGoal() { InConstGoalId = GL_CONTROL, InConstValue = 0 });
            command.Add(new SetGoal() { InConstGoalId = GL_TEMP, InConstValue = 0 });
            command.Add(new SetGoal() { InConstGoalId = GL_CURRENT_X, InConstValue = current_pos.PreciseX });
            command.Add(new SetGoal() { InConstGoalId = GL_CURRENT_Y, InConstValue = current_pos.PreciseY });
            command.Add(new SetGoal() { InConstGoalId = GL_CURRENT_MOVE_X, InConstValue = current_pos_move.PreciseX });
            command.Add(new SetGoal() { InConstGoalId = GL_CURRENT_MOVE_Y, InConstValue = current_pos_move.PreciseY });
            command.Add(new SetGoal() { InConstGoalId = GL_CURRENT_DISTANCE, InConstValue = -1 });
            command.Add(new SetGoal() { InConstGoalId = GL_PRED_X, InConstValue = pred_pos.PreciseX });
            command.Add(new SetGoal() { InConstGoalId = GL_PRED_Y, InConstValue = pred_pos.PreciseY });
            command.Add(new SetGoal() { InConstGoalId = GL_PRED_MOVE_X, InConstValue = pred_pos_move.PreciseX });
            command.Add(new SetGoal() { InConstGoalId = GL_PRED_MOVE_Y, InConstValue = pred_pos_move.PreciseY });
            command.Add(new SetGoal() { InConstGoalId = GL_PRED_DISTANCE, InConstValue = -1 });
            command.Add(new SetGoal() { InConstGoalId = GL_MOVE_X, InConstValue = -1 });
            command.Add(new SetGoal() { InConstGoalId = GL_MOVE_Y, InConstValue = -1 });

            // checks

            command.Add(new UpSetTargetById() { InConstId = Unit.Id });
            command.Add(new UpGetObjectData() { InConstObjectData = (int)ObjectData.ID, OutGoalData = GL_TEMP });
            command.Add(new Goal() { InConstGoalId = GL_TEMP }, "!=", Unit.Id,
                new SetGoal() { InConstGoalId = GL_CONTROL, InConstValue = -1 });

            command.Add(new UpGetObjectData() { InConstObjectData = (int)ObjectData.NEXT_ATTACK, OutGoalData = GL_TEMP });
            command.Add(new Goal() { InConstGoalId = GL_TEMP }, "<", min_next_attack,
                new SetGoal() { InConstGoalId = GL_CONTROL, InConstValue = -2 });

            command.Add(new Goal() { InConstGoalId = GL_TEMP }, ">", max_next_attack,
                new SetGoal() { InConstGoalId = GL_CONTROL, InConstValue = -3 });

            // choose target pos

            command.Add(new UpBoundPrecisePoint() { InGoalPoint = GL_CURRENT_X, InConstPrecise = 1, InConstBorder = 1 });
            command.Add(new UpSetPreciseTargetPoint() { InGoalPoint = GL_CURRENT_X });
            command.Add(new UpGetObjectData() { InConstObjectData = (int)ObjectData.PRECISE_DISTANCE, OutGoalData = GL_CURRENT_DISTANCE });
            command.Add(new UpBoundPrecisePoint() { InGoalPoint = GL_PRED_X, InConstPrecise = 1, InConstBorder = 1 });
            command.Add(new UpSetPreciseTargetPoint() { InGoalPoint = GL_PRED_X });
            command.Add(new UpGetObjectData() { InConstObjectData = (int)ObjectData.PRECISE_DISTANCE, OutGoalData = GL_PRED_DISTANCE });
            command.Add(new UpModifyGoal() { IoGoalId = GL_CURRENT_DISTANCE, MathOp = op_g_sub, InOpValue = GL_PRED_DISTANCE });

            command.Add(new SetStrategicNumber() { InConstSnId = (int)AoE2Lib.StrategicNumber.TARGET_POINT_ADJUSTMENT, InConstValue = 6 });

            var command_current = new Command();
            command_current.Add(new Goal() { InConstGoalId = GL_CURRENT_DISTANCE }, "<=", 0,
                //new ChatToAll() { InTextString = "Move current" },
                new UpFullResetSearch(),
                new UpAddObjectById() { InConstSearchSource = 1, InConstId = Unit.Id },
                new UpTargetPoint()
                {
                    InGoalPoint = GL_CURRENT_MOVE_X,
                    InConstTargetAction = (int)UnitAction.MOVE,
                    InConstFormation = -1,
                    InConstAttackStance = (int)UnitStance.NO_ATTACK
                },
                new SetGoal() { InConstGoalId = GL_MOVE_X, InConstValue = GL_CURRENT_MOVE_X },
                new SetGoal() { InConstGoalId = GL_MOVE_Y, InConstValue = GL_CURRENT_MOVE_Y },
                new SetGoal() { InConstGoalId = GL_CONTROL, InConstValue = 1 });

            var command_pred = new Command();
            command_pred.Add(new Goal() { InConstGoalId = GL_CURRENT_DISTANCE }, ">", 0,
                //new ChatToAll() { InTextString = "Move pred" },
                new UpFullResetSearch(),
                new UpAddObjectById() { InConstSearchSource = 1, InConstId = Unit.Id },
                new UpTargetPoint()
                {
                    InGoalPoint = GL_PRED_MOVE_X,
                    InConstTargetAction = (int)UnitAction.MOVE,
                    InConstFormation = -1,
                    InConstAttackStance = (int)UnitStance.NO_ATTACK
                },
                new SetGoal() { InConstGoalId = GL_MOVE_X, InConstValue = GL_PRED_MOVE_X },
                new SetGoal() { InConstGoalId = GL_MOVE_Y, InConstValue = GL_PRED_MOVE_Y },
                new SetGoal() { InConstGoalId = GL_CONTROL, InConstValue = 2 });

            command.Add(new Goal() { InConstGoalId = GL_CONTROL }, "==", 0,
                command_current);
            command.Add(new Goal() { InConstGoalId = GL_CONTROL }, "==", 0,
                command_pred);

            command.Add(new Goal() { InConstGoalId = GL_MOVE_X });
            command.Add(new Goal() { InConstGoalId = GL_MOVE_Y });

            Unary.ExecuteCommand(command);
        }

        protected void MoveTo(Position position, double radius)
        {
            if (Unit.Position.DistanceTo(position) > radius)
            {
                Unit.Target(position);
            }
        }

        private Position GetMovementPosition(Position current, Position move_position, double move_radius)
        {
            var friendlies = RelevantUnits
                .Where(u => u.Targetable && u.PlayerNumber == Unary.PlayerNumber && u != Unit)
                .Select(u => new KeyValuePair<double, Position>(1, u.Position))
                .ToList();

            var enemies = RelevantUnits
                .Where(u => u.Targetable && Unary.GameState.GetPlayer(u.PlayerNumber).Stance == PlayerStance.ENEMY)
                .Select(u => new ValueTuple<double, Position, double>(1d, u.Position, u[ObjectData.RANGE] + 1))
                .ToList();

            var best_pos = current;
            var best_val = double.MaxValue;
            var positions = new List<Position>();

            var angle = 0d;
            var radius = 0.5d;
            for (int i = 0; i < 8; i++)
            {
                var dpos = Position.FromPolar(angle, radius);
                var pos = current + dpos;
                positions.Add(pos);
                angle += Math.PI / 4;
            }

            angle = 0d;
            radius = 1.1 * Unary.GameState.GameTimePerTick.TotalSeconds * Unit[ObjectData.SPEED] / 100d;
            for (int i = 0; i < 16; i++)
            {
                var dpos = Position.FromPolar(angle, radius);
                var pos = current + dpos;
                positions.Add(pos);
                angle += Math.PI / 8;
            }

            positions.Add(current);

            foreach (var position in positions)
            {
                var field = PotentialField.GetStrengthAtPosition(position, 
                    move_position, move_radius,
                    friendlies,
                    enemies);

                if (field <= best_val)
                {
                    best_pos = position;
                    best_val = field;
                }
            }

            return best_pos;
        }

        private void UpdateRelevantUnits()
        {
            RelevantUnits.Clear();

            foreach (var tile in Unary.GameState.Map.GetTilesInRange(Unit.Position, 20))
            {
                foreach (var unit in tile.Units.Where(u => u.Targetable))
                {
                    RelevantUnits.Add(unit);
                }
            }
        }

        private Position PredictPosition(Unit unit)
        {
            var max_d = Unary.GameState.GameTimePerTick.TotalSeconds * unit[ObjectData.SPEED] / 100d;
            var dpos = Position.FromPrecise(unit[ObjectData.PRECISE_MOVE_X], unit[ObjectData.PRECISE_MOVE_Y]);
            dpos -= unit.Position;
            if (dpos.Norm > max_d)
            {
                dpos = max_d * (dpos / dpos.Norm); 
            }

            return unit.Position + dpos;
        }
    }
}
