using AoE2Lib.Bots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Jobs;

namespace Unary.Behaviours
{
    internal class JobBehaviour : Behaviour
    {
        private TimeSpan LastSeekTime { get; set; } = TimeSpan.FromHours(-1);
        private Job CurrentJob => Controller.CurrentJob;

        public override int GetPriority() => 1000;

        protected override bool Tick(bool perform)
        {
            var seek = CurrentJob != null ? TimeSpan.FromMinutes(1) : TimeSpan.FromSeconds(5);

            if (Controller.Unary.GameState.GameTime - LastSeekTime > seek && ShouldRareTick(3))
            {
                if (Controller.Unit[ObjectData.CMDID] != (int)CmdId.VILLAGER || Controller.Unit[ObjectData.CARRY] <= 0)
                {
                    LookForJob();
                    LastSeekTime = Controller.Unary.GameState.GameTime;
                }
            }

            return false;
        }

        private void LookForJob()
        {
            var lookahead = TimeSpan.FromMinutes(Controller.Unary.Settings.CivilianJobLookAheadMinutes);
            var best_profit = double.MinValue;
            Job best_job = null;
            var speed = Controller.Unit[ObjectData.SPEED] / 100d;
            var position = Controller.Unit.Position;

            foreach (var job in Controller.Unary.UnitsManager.GetJobs())
            {
                var pay = job.GetPay(Controller);

                if (pay > 0)
                {
                    var distance = position.DistanceTo(job.Location);
                    var travel = TimeSpan.FromSeconds(distance / speed);
                    var time = lookahead - travel;
                    var profit = pay * time.TotalSeconds;

                    if (profit > best_profit)
                    {
                        best_profit = profit;
                        best_job = job;
                    }
                }
            }

            if (best_job != CurrentJob)
            {
                if (CurrentJob != null)
                {
                    CurrentJob.Leave(Controller);
                }

                if (best_job != null)
                {
                    best_job.Join(Controller);
                }
            }
        }
    }
}
