using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.Modules;
using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Quaternary.Modules.UnitManagerModule;

namespace Quaternary.Modules
{
    internal class ScoutingModule : Module
    {
        private UnitGroup Scouts { get; set; }
        private readonly Random RNG = new Random(Guid.NewGuid().GetHashCode());

        protected override IEnumerable<Command> RequestUpdate()
        {
            if (Scouts == null)
            {
                Scouts = Bot.GetModule<UnitManagerModule>().CreateGroup();
            }

            GetScouts();
            Scout();

            foreach (var scout in Scouts)
            {
                scout.RequestUpdate();
            }

            return Enumerable.Empty<Command>();
        }

        protected override void Update()
        {

        }

        private void GetScouts()
        {
            if (Scouts.Count > 0)
            {
                return;
            }

            var units = Bot.GetModule<UnitsModule>().Units.Values
                .Where(u => u.PlayerNumber == Bot.PlayerNumber && u.IsBuilding == false && u.CmdId == CmdId.MILITARY)
                .ToList();

            units.Sort((a, b) => b.Speed.CompareTo(a.Speed));

            var manager = Bot.GetModule<UnitManagerModule>();
            foreach (var unit in units)
            {
                if (!manager.IsUnitGrouped(unit))
                {
                    Scouts.Add(unit);
                    break;
                }
            }
        }

        private void Scout()
        {
            if (Scouts.Count == 0)
            {
                return;
            }

            var gametime = Bot.GetModule<InfoModule>().GameTime;

            if (RNG.NextDouble() < 1 / (10 + (gametime.TotalMinutes * 2)))
            {
                var scout = Scouts.First();
                var map = Bot.GetModule<MapModule>();
                var pos = Position.FromPoint(RNG.Next(map.Width), RNG.Next(map.Height));

                scout.TargetPosition(pos, UnitAction.MOVE, UnitFormation.LINE, UnitStance.AGGRESSIVE);

                Bot.Log.Info($"ScoutingModule: Send scout {scout.Id} to {pos.PointX} {pos.PointY}");
            }
        }
    }
}
