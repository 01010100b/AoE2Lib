using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quaternary.Modules
{
    class UnitManagerModule : Module
    {
        public class UnitGroup : IEnumerable<Unit>
        {
            public int Count => Units.Count;

            private readonly UnitManagerModule Manager;
            private readonly HashSet<Unit> Units = new HashSet<Unit>();

            public UnitGroup(UnitManagerModule manager)
            {
                Manager = manager;
            }

            public bool HasUnit(Unit unit)
            {
                return Units.Contains(unit);
            }

            public void Add(Unit unit)
            {
                Manager.RemoveUnitFromGroups(unit);
                Units.Add(unit);
            }

            public void Remove(Unit unit)
            {
                Units.Remove(unit);
            }

            public IEnumerator<Unit> GetEnumerator()
            {
                return Units.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return Units.GetEnumerator();
            }
        }

        private readonly List<UnitGroup> UnitGroups = new List<UnitGroup>();

        public UnitGroup CreateGroup()
        {
            var group = new UnitGroup(this);
            UnitGroups.Add(group);

            return group;
        }

        public bool IsUnitGrouped(Unit unit)
        {
            return GetUnitGroup(unit) != default(UnitGroup);
        }

        public UnitGroup GetUnitGroup(Unit unit)
        {
            return UnitGroups.FirstOrDefault(g => g.HasUnit(unit));
        }

        private void RemoveUnitFromGroups(Unit unit)
        {
            foreach (var group in UnitGroups)
            {
                group.Remove(unit);
            }
        }

        protected override IEnumerable<Command> RequestUpdate()
        {
            return Enumerable.Empty<Command>();
        }

        protected override void Update()
        {
            
        }
    }
}
