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
            var seek = CurrentJob != null ? TimeSpan.FromMinutes(1) : TimeSpan.FromSeconds(5);

            if (Unary.GameState.GameTime - LastSeekTime > seek && ShouldRareTick(7))
            {
                if (Unit[ObjectData.CMDID] != (int)CmdId.VILLAGER || Unit[ObjectData.CARRY] <= 0)
                {
                    LookForJob();
                    LastSeekTime = Unary.GameState.GameTime;
                }
            }

            return false;
        }

        private void LookForJob()
        {
            var cmdid = (CmdId)Unit[ObjectData.CMDID];
            var minutes = Unary.Settings.CivilianJobLookAheadMinutes;

            if (cmdid == CmdId.MILITARY || cmdid == CmdId.MONK)
            {
                minutes = Unary.Settings.MilitaryJobLookAheadMinutes;
            }

            var lookahead = TimeSpan.FromMinutes(minutes);
            var best_profit = double.MinValue;
            Job best_job = null;
            var speed = Math.Max(0, Unit[ObjectData.SPEED]) / 100d;
            var position = Unit.Position;

            foreach (var job in Unary.JobManager.GetJobs())
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
