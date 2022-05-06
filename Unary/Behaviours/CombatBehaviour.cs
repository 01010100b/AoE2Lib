using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Behaviours
{
    internal abstract class CombatBehaviour : Behaviour
    {
        public Unit Target { get; private set; } = null;
        public Unit Backup { get; private set; } = null;
        public IEnumerable<Unit> NearbyUnits => _NearbyUnits;

        private readonly List<Unit> _NearbyUnits = new();

        protected abstract Unit ChooseTarget(out Unit backup);

        protected abstract void DoCombat();

        protected override sealed bool Tick(bool perform)
        {
            if (Target != null && !Target.Targetable)
            {
                Target = null;
                Backup = null;
            }

            if (!perform)
            {
                return false;
            }

            if (ShouldRareTick(13))
            {
                _NearbyUnits.Clear();

                foreach (var unit in Controller.Unary.GameState.GetPlayers().SelectMany(p => p.Units).Where(u => u.Targetable))
                {
                    _NearbyUnits.Add(unit);
                }
            }

            Target = ChooseTarget(out var backup);
            Backup = backup;

            if (Backup == null)
            {
                Backup = Target;
            }

            if (Target != null)
            {
                DoCombat();
                Controller.Unit.RequestUpdate();
                Target.RequestUpdate();
                Backup.RequestUpdate();

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
