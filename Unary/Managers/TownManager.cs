using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Managers
{
    // building placements, walling
    internal class TownManager : Manager
    {
        public TownManager(Unary unary) : base(unary)
        {

        }

        public List<Tile> GetSortedBuildingPlacements(UnitType building, IEnumerable<Tile> possible_placements)
        {
            throw new NotImplementedException();
        }

        internal override void Update()
        {
            //throw new NotImplementedException();
        }
    }
}
