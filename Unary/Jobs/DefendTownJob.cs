﻿using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Behaviours;

namespace Unary.Jobs
{
    internal class DefendTownJob : CombatJob
    {
        public override string Name => "Defend town";

        public override Position Location => Unary.TownManager.MyPosition;

        public DefendTownJob(Unary unary) : base(unary)
        {
        }

        public override double GetPay(Controller worker)
        {
            if (worker.HasBehaviour<CombatBehaviour>())
            {
                return 10;
            }
            else
            {
                return -1;
            }
        }

        public override void OnRemoved()
        {
            
        }

        public override void Update()
        {
            var targets = ObjectPool.Get(() => new Dictionary<Unit, double>(), x => x.Clear());
            
            foreach (var target in Unary.UnitsManager.Enemies)
            {
                targets.Add(target, 1);
            }

            PerformCombat(GetWorkers(), targets);

            ObjectPool.Add(targets);
        }

        protected override void OnWorkerJoining(Controller worker)
        {
        }

        protected override void OnWorkerLeaving(Controller worker)
        {
            if (worker.TryGetBehaviour<CombatBehaviour>(out var behaviour))
            {
                behaviour.Target = null;
                behaviour.Backup = null;
                behaviour.Threat = null;
            }
        }
    }
}
