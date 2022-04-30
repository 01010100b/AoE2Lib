﻿using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Behaviours;

namespace Unary.Managers
{
    // controllers
    internal class UnitsManager : Manager
    {
        //private const int GROUPS = 10;

        public IEnumerable<Controller> Villagers => Controllers.Values.Where(c => c.Unit[ObjectData.CMDID] == (int)CmdId.VILLAGER);
        public IEnumerable<Controller> Constructions => Controllers.Values.Where(c =>
        {
            var b = c.GetBehaviour<ConstructionBehaviour>();

            if (b == null)
            {
                return false;
            }
            else
            {
                return b.MaxBuilders > 0;
            }
        });
        public IEnumerable<Controller> EatingSpots => Controllers.Values.Where(c =>
        {
            var b = c.GetBehaviour<EatingSpotBehaviour>();

            if (b == null)
            {
                return false;
            }
            else
            {
                return b.Target != null;
            }
        });

        private readonly Dictionary<Unit, Controller> Controllers = new();

        public UnitsManager(Unary unary) : base(unary)
        {

        }

        public IEnumerable<Controller> GetControllers() => Controllers.Values;

        internal override void Update()
        {
            UpdateControllers();
        }

        private void UpdateControllers()
        {
            foreach (var unit in Unary.GameState.MyPlayer.Units.Where(u => u.Targetable))
            {
                if (!Controllers.ContainsKey(unit))
                {
                    var controller = new Controller(unit, Unary);
                    Controllers.Add(unit, controller);
                }
            }

            foreach (var controller in Controllers.Values.ToList())
            {
                if (controller.Unit.Targetable)
                {
                    controller.Tick();
                }
                else
                {
                    Controllers.Remove(controller.Unit);
                }
            }
        }
    }
}
