using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unary.Managers;

namespace Unary.Operations
{
    class BuildOperation : Operation
    {
        public readonly List<Unit> Foundations = new List<Unit>();

        public BuildOperation(OperationsManager manager) : base(manager)
        {
        }

        public override void Update()
        {
            var builders = Units.ToList();
            Foundations.RemoveAll(f => f[AoE2Lib.ObjectData.STATUS] != 0 || f.Targetable == false);

            if (builders.Count == 0 || Foundations.Count == 0)
            {
                return;
            }

            foreach (var builder in builders)
            {
                builder.RequestUpdate();
            }

            foreach (var foundation in Foundations)
            {
                foundation.RequestUpdate();
            }

            var free_foundations = new HashSet<int>();
            foreach (var foundation in Foundations)
            {
                free_foundations.Add(foundation.Id);
            }

            var free_builders = new HashSet<Unit>();
            foreach (var builder in builders)
            {
                if (!free_foundations.Contains(builder[AoE2Lib.ObjectData.TARGET_ID]))
                {
                    free_builders.Add(builder);
                }
            }

            foreach (var builder in builders)
            {
                if (free_foundations.Contains(builder[AoE2Lib.ObjectData.TARGET_ID]))
                {
                    free_foundations.Remove(builder[AoE2Lib.ObjectData.TARGET_ID]);
                }
            }

            if (free_builders.Count == 0 || free_foundations.Count == 0)
            {
                return;
            }

            foreach (var builder in free_builders)
            {
                var id = free_foundations.First();
                var foundation = Foundations.First(f => f.Id == id);

                builder.TargetUnit(foundation, AoE2Lib.UnitAction.DEFAULT, null, null);
                free_foundations.Remove(id);

                if (free_foundations.Count == 0)
                {
                    break;
                }
            }
        }
    }
}
