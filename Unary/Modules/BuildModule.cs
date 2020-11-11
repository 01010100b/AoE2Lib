using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Utils;

namespace Unary.Modules
{
    public class BuildModule : Module
    {
        internal override IEnumerable<Command> RequestUpdate(Bot bot)
        {
            return new List<Command>();
            //throw new NotImplementedException();
        }

        internal override void Update(Bot bot)
        {
            //throw new NotImplementedException();
        }

        private bool CanBuildAtPosition(GameState state, Position position, int size)
        {
            var elevation = int.MinValue;

            for (int x = position.X; x <= position.X + size; x++)
            {
                for (int y = position.Y; y <= position.Y + size; y++)
                {
                    if (x >= state.MapWidthHeight || y >= state.MapWidthHeight)
                    {
                        return false;
                    }

                    var pos = new Position(x, y);
                    var tile = state.Tiles[pos];

                    if (!tile.Explored)
                    {
                        return false;
                    }

                    if (elevation == int.MinValue)
                    {
                        elevation = tile.Elevation;
                    }

                    if (tile.Elevation != elevation)
                    {
                        return false;
                    }

                    if (!IsTerrainBuildable(tile.Terrain))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool IsTerrainBuildable(int terrain)
        {
            throw new NotImplementedException();
        }
    }
}
