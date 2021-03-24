using AoE2Lib;
using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unary.Operations;

namespace Unary.Managers
{
    class EconomyManager : Manager
    {
        private readonly List<Operation> Operations = new List<Operation>();

        public EconomyManager(Unary unary) : base(unary)
        {

        }

        public override void Update()
        {
            var ops = Unary.OperationsManager;
            var free_vills = ops.FreeUnits.Where(u => u[ObjectData.CMDID] == (int)CmdId.VILLAGER).ToList();

            if (free_vills.Count > 0)
            {
                if (Operations.OfType<EatOperation>().Count() == 0)
                {
                    var op = new EatOperation(ops);
                    op.AddUnit(free_vills.First());

                    Operations.Add(op);
                }
            }

            foreach (var op in Operations.ToList())
            {
                if (op is EatOperation eat)
                {
                    eat.Focus = Unary.InfoModule.MyPosition + new Position(-1, 1);

                    var sheep = eat.Units.FirstOrDefault(u => u[ObjectData.CLASS] == (int)UnitClass.Livestock && u[ObjectData.HITPOINTS] > 0);

                    if (sheep == null)
                    {
                        sheep = ops.FreeUnits.FirstOrDefault(u => u[ObjectData.CLASS] == (int)UnitClass.Livestock && u[ObjectData.HITPOINTS] > 0);

                        if (sheep == null)
                        {
                            foreach (var scout in ops.Operations.OfType<ScoutOperation>().SelectMany(o => o.Units))
                            {
                                if (scout[ObjectData.CLASS] == (int)UnitClass.Livestock && scout[ObjectData.HITPOINTS] > 0)
                                {
                                    sheep = scout;
                                }
                            }
                        }

                        eat.AddUnit(sheep);
                    }

                    if (sheep != null && eat.Units.Count(u => u[ObjectData.CMDID] == (int)CmdId.VILLAGER) < 6)
                    {
                        var vill = free_vills.FirstOrDefault(u => u[ObjectData.CMDID] == (int)CmdId.VILLAGER);
                        eat.AddUnit(vill);
                    }

                    if (eat.Units.Count() == 0)
                    {
                        eat.Stop();
                        Operations.Remove(eat);
                    }
                }
                else
                {
                    op.Stop();
                    Operations.Remove(op);
                }
            }
        }
    }
}
