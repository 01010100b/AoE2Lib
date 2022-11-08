using AoE2Lib.Bots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Jobs;

namespace Unary.Behaviours
{
    internal class JobSeekingBehaviour : Behaviour
    {
        private TimeSpan LastSeekTime { get; set; } = TimeSpan.FromHours(-1);
        private Job CurrentJob => Controller.CurrentJob;

        public override int GetPriority() => 10000;

        protected override bool Tick(bool perform)
        {
            var seek = CurrentJob != null ? TimeSpan.FromMinutes(1) : TimeSpan.FromSeconds(3);

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
            var cmdid = (CmdId)Controller.Unit[ObjectData.CMDID];
            var minutes = Controller.Unary.Settings.CivilianJobLookAheadMinutes;

            if (cmdid == CmdId.MILITARY || cmdid == CmdId.MONK)
            {
                minutes = Controller.Unary.Settings.MilitaryJobLookAheadMinutes;
            }

            var lookahead = TimeSpan.FromMinutes(minutes);
            var best_profit = double.MinValue;
            Job best_job = null;
            var speed = Math.Max(0, Controller.Unit[ObjectData.SPEED]) / 100d;
            var position = Controller.Unit.Position;

            foreach (var job in Controller.Unary.JobManager.GetJobs())
            {
                var pay = job.GetPay(Controller);

                if (pay > 0)
                {
                    var distance = position.DistanceTo(job.Location);
                    var travel = TimeSpan.FromSeconds(distance / Math.Max(0.01, speed));
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
