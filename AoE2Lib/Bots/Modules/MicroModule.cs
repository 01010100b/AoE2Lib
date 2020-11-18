using AoE2Lib.Bots.GameElements;
using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots.Modules
{
    public class MicroModule : Module
    {
        private readonly List<Command> Commands = new List<Command>();

        public void TargetUnit(Unit unit, Unit target, UnitAction action, UnitFormation formation, UnitStance stance)
        {

        }

        public void TargetPosition(Unit unit, Position target, UnitAction action, UnitFormation formation, UnitStance stance)
        {

        }

        protected override IEnumerable<Command> RequestUpdate()
        {
            foreach (var command in Commands)
            {

            }
            throw new NotImplementedException();
        }

        protected override void Update()
        {
            throw new NotImplementedException();
        }
    }
}
