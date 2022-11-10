using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Jobs
{
    internal class ConstructionManagementJob : ManagementJob
    {
        public override string Name => "Construction management";

        public ConstructionManagementJob(Unary unary) : base(unary)
        {
        }

        public IEnumerable<Tile> GetBuildTiles(UnitType building, IEnumerable<KeyValuePair<Tile, double>> tiles)
        {
            var sorted = ObjectPool.Get(() => new List<KeyValuePair<Tile, double>>(), x => x.Clear());
            sorted.AddRange(tiles);
            sorted.Sort((a, b) => b.Value.CompareTo(a.Value));

            foreach (var kvp in sorted)
            {
                if (Unary.MapManager.CanBuild(building, kvp.Key))
                {
                    yield return kvp.Key;
                }
            }

            ObjectPool.Add(sorted);
        }

        protected override void Initialize()
        {
        }

        protected override void OnClosed()
        {
        }

        protected override void Update()
        {
        }
    }
}
