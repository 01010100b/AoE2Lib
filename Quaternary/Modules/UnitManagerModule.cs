using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quaternary.Modules
{
    internal class UnitManagerModule : Module
    {
        internal class UnitGroup
        {
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

            public void AddUnit(Unit unit)
            {
                Manager.RemoveUnitFromGroups(unit);
                Units.Add(unit);
            }

            public void RemoveUnit(Unit unit)
            {
                Units.Remove(unit);
            }

            public IEnumerable<Unit> GetUnits()
            {
                return Units;
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
                group.RemoveUnit(unit);
            }
        }

        protected override IEnumerable<Command> RequestUpdate()
        {
            throw new NotImplementedException();
        }

        protected override void Update()
        {
            throw new NotImplementedException();
        }
    }
}
