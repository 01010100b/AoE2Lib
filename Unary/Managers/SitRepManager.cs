using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Managers
{
    internal class SitRepManager : Manager
    {
        public class SitRep
        {
            public readonly Tile Tile;
            public bool IsLandAccessible { get; internal set; } = false;
            public bool IsWaterAccessible { get; internal set; } = false;
            public bool CanConstructOn { get; internal set; } = false;
            public int PathDistanceToHome { get; internal set; } = int.MaxValue;

            public SitRep(Tile tile)
            {
                Tile = tile;
            }
        }

        public SitRep this[Tile tile] => GetSitRep(tile);

        private readonly Dictionary<Tile, SitRep> SitReps = new();

        public SitRepManager(Unary unary) : base(unary)
        {

        }

        internal override void Update()
        {
            //throw new NotImplementedException();
        }

        private SitRep GetSitRep(Tile tile)
        {
            if (!SitReps.ContainsKey(tile))
            {
                var sitrep = new SitRep(tile);
                SitReps.Add(tile, sitrep);
            }

            return SitReps[tile];
        }
    }
}
