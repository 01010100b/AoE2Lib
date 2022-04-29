using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Managers
{
    internal class TownManager : Manager
    {
        public TownManager(Unary unary) : base(unary)
        {

        }

        public List<Tile> GetBuildingPlacements(UnitType building, IEnumerable<Tile> possible_placements)
        {
            throw new NotImplementedException();
        }

        internal override void Update()
        {
            throw new NotImplementedException();
        }
    }
}
