using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unary.Managers;

namespace Unary.Operations
{
    class OldBuildOperation : Operation
    {
        public OldBuildOperation(OperationsManager manager) : base(manager)
        {

        }

        public override void Update()
        {
            if (Units.Count() == 0)
            {
                return;
            }

            var foundations = new Dictionary<int, Unit>();
            var builders = new HashSet<Unit>();
            foreach (var unit in Units.ToList())
            {
                if (unit.GetData(ObjectData.STATUS) == 0 && unit.GetData(ObjectData.CATEGORY) == 80)
                {
                    foundations.Add(unit.Id, unit);
                }
                else if (unit.GetData(ObjectData.CMDID) == (int)CmdId.VILLAGER)
                {
                    builders.Add(unit);
                }
                else
                {
                    RemoveUnit(unit);
                }
            }

            Manager.Unary.Log.Info($"BuildOperation with {builders.Count} builders");

            foreach (var builder in builders.ToList())
            {
                if (foundations.TryGetValue(builder.GetData(ObjectData.TARGET_ID), out Unit foundation))
                {
                    foundations.Remove(foundation.Id);
                    builders.Remove(builder);
                }
            }

            foreach (var builder in builders)
            {
                if (foundations.Count > 0)
                {
                    var best = foundations.Values.First();
                    var cost = double.MaxValue;

                    foreach (var foundation in foundations.Values)
                    {
                        var c = builder.Position.DistanceTo(foundation.Position);
                        if (c < cost)
                        {
                            best = foundation;
                            cost = c;
                        }
                    }

                    builder.TargetUnit(best, UnitAction.DEFAULT, UnitFormation.LINE, UnitStance.NO_ATTACK);
                    foundations.Remove(best.Id);
                }
                else
                {
                    RemoveUnit(builder);
                }
            }
        }
    }
}
